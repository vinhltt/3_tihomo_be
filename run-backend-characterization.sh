#!/usr/bin/env bash
# Starts disposable PostgreSQL + RabbitMQ (podman, loopback only), applies each service's own migrations by
# starting the 6 APIs against fixture DBs, runs the characterization tests, then tears everything down.
# Usage: bash run-backend-characterization.sh --sdk-root <dotnet-root> --results-dir <absolute-dir> [--mode all|characterization] [--keep-up] [--with-observability]
#   --mode all (default) runs the 8-suite full acceptance; --mode characterization runs only Category=Characterization cases (scoped lane)
#   --with-observability  also start Tempo (OTLP :4317) and Loki (:3100) from ../../config
# thm2_identity is ALWAYS restored from Identity/Fixtures/identity-baseline-openiddict58.sql: a schema + legacy rows written by the
# OpenIddict 5.8 / net9 baseline, so the net10 Identity migration and OpenIddictStoreParityTests run against real legacy data.
# Never touches existing containers/volumes: only containers labelled thm2=1 created here are removed.
set -u
SDK_ROOT="" RESULTS_DIR="" KEEP=0 OBS=0 MODE="all"
while [ $# -gt 0 ]; do
  case "$1" in
    --sdk-root) SDK_ROOT="${2:-}"; shift 2 ;;
    --results-dir) RESULTS_DIR="${2:-}"; shift 2 ;;
    --mode) MODE="${2:-}"; shift 2 ;;
    --keep-up) KEEP=1; shift ;;
    --with-observability) OBS=1; shift ;;
    *) echo "unknown argument: $1" >&2; exit 2 ;;
  esac
done
[ -x "$SDK_ROOT/dotnet" ] || { echo "--sdk-root must contain dotnet: '$SDK_ROOT'" >&2; exit 2; }
case "$RESULTS_DIR" in /*) ;; *) echo "--results-dir must be absolute" >&2; exit 2 ;; esac
case "$MODE" in all|characterization) ;; *) echo "--mode must be all or characterization" >&2; exit 2 ;; esac
command -v podman >/dev/null || { echo "podman is required for disposable PostgreSQL/RabbitMQ" >&2; exit 2; }

cd "$(dirname "$0")" || exit 2
export DOTNET_ROOT="$SDK_ROOT" PATH="$SDK_ROOT:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 \
       DOTNET_USE_POLLING_FILE_WATCHER=1 MSBUILDDISABLENODEREUSE=1
mkdir -p "$RESULTS_DIR/services"

PG_PASS=thm2_pg_pass RMQ_PASS=thm2_rmq_pass
PG="Host=127.0.0.1;Port=5432;Username=postgres;Password=$PG_PASS"
PIDS=()

port_free() { ! (timeout 1 bash -c "echo >/dev/tcp/127.0.0.1/$1" 2>/dev/null); }
cleanup() {
  [ "$KEEP" = 1 ] && { echo "--keep-up: leaving infra and services running"; return; }
  for p in "${PIDS[@]}"; do kill "$p" 2>/dev/null; done
  sleep 2; for p in "${PIDS[@]}"; do kill -9 "$p" 2>/dev/null; done
  # label filter: never remove a same-named container this script did not create
  podman rm -f $(podman ps -aq --filter label=thm2=1 --filter 'name=^thm2-(pg|rmq|tempo|loki)$') >/dev/null 2>&1
}
trap cleanup EXIT

# ---- infra: always fresh (disposable; a stopped leftover from a reboot must not be reused) ----
# Only containers labelled thm2=1 are ever removed; a same-named container we did not create stops the run untouched.
for c in thm2-pg thm2-rmq thm2-tempo thm2-loki; do
  if podman container exists "$c" 2>/dev/null; then
    [ "$(podman inspect -f '{{index .Config.Labels "thm2"}}' "$c" 2>/dev/null)" = 1 ] \
      || { echo "FAIL: container $c exists but is not labelled thm2=1 (not ours); refusing to remove it" >&2; exit 1; }
    podman rm -f "$c" >/dev/null 2>&1
  fi
done
port_free 5432 || { echo "FAIL: 127.0.0.1:5432 busy (not ours)" >&2; exit 1; }
port_free 5672 || { echo "FAIL: 127.0.0.1:5672 busy (not ours)" >&2; exit 1; }
podman run -d --name thm2-pg --label thm2=1 -p 127.0.0.1:5432:5432 -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=$PG_PASS docker.io/library/postgres:17.6 >/dev/null || exit 1
podman run -d --name thm2-rmq --label thm2=1 -p 127.0.0.1:5672:5672 -e RABBITMQ_DEFAULT_USER=tihomo \
  -e RABBITMQ_DEFAULT_PASS=$RMQ_PASS -e RABBITMQ_DEFAULT_VHOST=/ docker.io/library/rabbitmq:4-management >/dev/null || exit 1
for i in $(seq 1 60); do podman exec thm2-pg pg_isready -U postgres >/dev/null 2>&1 && break; sleep 1; done
podman exec thm2-pg pg_isready -U postgres >/dev/null 2>&1 || { echo "FAIL: postgres not ready" >&2; exit 1; }
for i in $(seq 1 60); do podman exec thm2-rmq rabbitmq-diagnostics -q ping >/dev/null 2>&1 && break; sleep 1; done
if [ "$OBS" = 1 ]; then
  # Same Tempo/Loki versions and configs as docker-compose.infras.yml, loopback only
  CFG="$(cd .. && cd .. && pwd)/config"
  for p in 4317 3200 3100; do port_free $p || { echo "FAIL: 127.0.0.1:$p busy (not ours)" >&2; exit 1; }; done
  podman run -d --name thm2-tempo --label thm2=1 --user root -p 127.0.0.1:4317:4317 -p 127.0.0.1:3200:3200 \
    -v "$CFG/tempo.yml:/etc/tempo/tempo.yaml:ro" docker.io/grafana/tempo:2.8.0 -config.file=/etc/tempo/tempo.yaml >/dev/null || exit 1
  podman run -d --name thm2-loki --label thm2=1 -p 127.0.0.1:3100:3100 \
    -v "$CFG/loki.yml:/etc/loki/local-config.yaml:ro" docker.io/grafana/loki:3.4.1 -config.file=/etc/loki/local-config.yaml >/dev/null || exit 1
  for i in $(seq 1 90); do [ "$(curl -s -m 2 -o /dev/null -w '%{http_code}' http://127.0.0.1:3200/ready)" = 200 ] \
    && [ "$(curl -s -m 2 -o /dev/null -w '%{http_code}' http://127.0.0.1:3100/ready)" = 200 ] && break; sleep 1; done
  echo "observability: tempo :3200 (otlp :4317), loki :3100"
fi
podman exec thm2-rmq rabbitmq-diagnostics -q ping >/dev/null 2>&1 || { echo "FAIL: rabbitmq not ready" >&2; exit 1; }
for d in thm2_identity thm2_corefinance thm2_money thm2_planning; do
  podman exec thm2-pg psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$d'" | grep -q 1 \
    || podman exec thm2-pg psql -U postgres -tAc "CREATE DATABASE $d" >/dev/null
done
# Baseline Identity DB (OpenIddict 5.8 schema at migration 20250706135843 + synthetic legacy client/tokens); see Identity/Fixtures/README.md
FIXTURE="Identity/Fixtures/identity-baseline-openiddict58.sql"
[ -f "$FIXTURE" ] || { echo "FAIL: baseline fixture missing: $FIXTURE" >&2; exit 1; }
podman exec -i thm2-pg psql -U postgres -d thm2_identity -v ON_ERROR_STOP=1 -q < "$FIXTURE" >/dev/null \
  || { echo "FAIL: loading $FIXTURE" >&2; exit 1; }
echo "identity: loaded baseline fixture (net10 Identity migrates it on start)"

# ---- one shared signing config so a gateway-issued token validates everywhere ----
JWT_SECRET="ThisIsAVerySecretKeyForJWTTokenGenerationAndValidation123456789"
JWT_ENV=("JwtSettings__SecretKey=$JWT_SECRET" JwtSettings__Issuer=TiHoMo.Identity JwtSettings__Audience=TiHoMo.Clients)
RMQ_ENV=(RabbitMQ__Host=127.0.0.1 RabbitMQ__Username=tihomo "RabbitMQ__Password=$RMQ_PASS")
if [ "$OBS" = 1 ]; then
  # Traces to Tempo at full sampling for the proof; Loki sinks already default to localhost:3100 (Excel, Gateway)
  OTEL_ENV=(OtelSettings__ExporterOtlpEndpoint=http://127.0.0.1:4317 OtelSettings__TracesSamplerArg=1.0)
else
  OTEL_ENV=(OtelSettings__ExporterOtlpEndpoint=http://127.0.0.1:4317)   # unreachable unless --with-observability
fi

# Services run from a separate publish output so verify-backend.sh can rebuild bin/ while they keep running.
start() { # name project port env... -- all env passed as VAR=value arguments
  local name=$1 proj=$2 port=$3; shift 3
  local out="$RESULTS_DIR/publish/$name" dll
  port_free "$port" || { echo "FAIL: port $port busy before starting $name" >&2; exit 1; }
  dotnet publish "$proj" -c Debug -o "$out" -nodeReuse:false -v:q > "$RESULTS_DIR/services/$name.publish.log" 2>&1 \
    || { echo "FAIL: publish $name (see $RESULTS_DIR/services/$name.publish.log)" >&2; exit 1; }
  dll="$out/$(basename "$proj" .csproj).dll"
  # exec: the recorded PID is dotnet itself, so cleanup really stops the service (no orphan holding the port)
  (cd "$(dirname "$proj")" && exec env ASPNETCORE_URLS="http://127.0.0.1:$port" "${OTEL_ENV[@]}" "$@" dotnet "$dll" \
    > "$RESULTS_DIR/services/$name.log" 2>&1) &
  PIDS+=($!)
}
start identity     Identity/Identity.Api/Identity.Api.csproj                                   5001 ASPNETCORE_ENVIRONMENT=Development "${JWT_ENV[@]}" "ConnectionStrings__IdentityDb=$PG;Database=thm2_identity"
start money        MoneyManagement/MoneyManagement.Api/MoneyManagement.Api.csproj             5002 ASPNETCORE_ENVIRONMENT=Development "${JWT_ENV[@]}" "ConnectionStrings__MoneyManagementDb=$PG;Database=thm2_money"
start planning     PlanningInvestment/PlanningInvestment.Api/PlanningInvestment.Api.csproj    5003 ASPNETCORE_ENVIRONMENT=Development "${JWT_ENV[@]}" "ConnectionStrings__PlanningInvestmentDb=$PG;Database=thm2_planning"
start corefinance  CoreFinance/CoreFinance.Api/CoreFinance.Api.csproj                         5004 ASPNETCORE_ENVIRONMENT=Development "${JWT_ENV[@]}" "${RMQ_ENV[@]}" "ConnectionStrings__CoreFinanceDb=$PG;Database=thm2_corefinance"
start excel        ExcelApi/ExcelApi.csproj                                                    5005 ASPNETCORE_ENVIRONMENT=Development "${JWT_ENV[@]}" "${RMQ_ENV[@]}"
start gateway      Ocelot.Gateway/Ocelot.Gateway.csproj                                        5000 ASPNETCORE_ENVIRONMENT=Local "${JWT_ENV[@]}"

# ---- readiness: every service must answer /health; a failure is a failed run, never a skip ----
ok=1
for pair in gateway:5000 identity:5001 money:5002 planning:5003 corefinance:5004 excel:5005; do
  n=${pair%%:*}; p=${pair##*:}; up=0
  for i in $(seq 1 120); do
    [ "$(curl -s -m 3 -o /dev/null -w '%{http_code}' "http://127.0.0.1:$p/health")" = 200 ] && { up=1; break; }; sleep 1
  done
  [ $up = 1 ] && echo "ready: $n :$p" || { echo "FAIL: $n :$p not healthy (see $RESULTS_DIR/services/$n.log)"; ok=0; }
done
[ $ok = 1 ] || exit 1

# Test-only settings: the external tests read these; in-process WebApplicationFactory tests keep their own config.
export TIHOMO_BACKEND_URLS="gateway=http://127.0.0.1:5000,identity=http://127.0.0.1:5001,money=http://127.0.0.1:5002,planning=http://127.0.0.1:5003,corefinance=http://127.0.0.1:5004,excel=http://127.0.0.1:5005"
export TIHOMO_TEST_PG="$PG" TIHOMO_TEST_RABBITMQ="amqp://tihomo:$RMQ_PASS@127.0.0.1:5672/"
export TIHOMO_TEST_DB_COREFINANCE=thm2_corefinance TIHOMO_TEST_DB_PLANNING=thm2_planning TIHOMO_TEST_DB_MONEY=thm2_money
export TIHOMO_TEST_IDENTITY_DB="$PG;Database=thm2_identity"
export TIHOMO_TEST_JWT_SECRET="$JWT_SECRET" TIHOMO_TEST_JWT_ISSUER=TiHoMo.Identity TIHOMO_TEST_JWT_AUDIENCE=TiHoMo.Clients

# The selected lane runs with the real stack up; verify-backend.sh builds first, services run from their own publish outputs.
bash verify-backend.sh --sdk-root "$SDK_ROOT" --results-dir "$RESULTS_DIR/verify" --mode "$MODE"
