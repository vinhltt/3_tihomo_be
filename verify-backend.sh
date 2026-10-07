#!/usr/bin/env bash
# Backend verification runner: independent of TiHoMo.sln and CI discovery.
# Usage: bash verify-backend.sh --sdk-root <dotnet-root> --results-dir <absolute-dir> [--mode build|all|unit|characterization]
#   build            : restore + build every project (checkpoint only, NOT full-suite acceptance)
#   all              : build + run every test suite, no unit/category filter (default; full acceptance)
#   unit             : build + run the 3 infrastructure-free suites with unit-tests.runsettings (scoped lane)
#   characterization : build + run Category=Characterization cases of 4 suites; needs the real stack (scoped lane)
# Exit 0 only when every project is built and every selected suite ran with valid TRX evidence and passed.
# Exit 0 of a scoped lane (unit/characterization) is NOT whole-backend green: other suites are reported 'outside-lane'.
set -u

SDK_ROOT="" RESULTS_DIR="" MODE="all"
while [ $# -gt 0 ]; do
  case "$1" in
    --sdk-root) SDK_ROOT="${2:-}"; shift 2 ;;
    --results-dir) RESULTS_DIR="${2:-}"; shift 2 ;;
    --mode) MODE="${2:-}"; shift 2 ;;
    *) echo "unknown argument: $1" >&2; exit 2 ;;
  esac
done
[ -x "$SDK_ROOT/dotnet" ] || { echo "--sdk-root must contain an executable dotnet: '$SDK_ROOT'" >&2; exit 2; }
case "$RESULTS_DIR" in /*) ;; *) echo "--results-dir must be an absolute path" >&2; exit 2 ;; esac
case "$MODE" in build|all|unit|characterization) ;; *) echo "--mode must be build, all, unit or characterization" >&2; exit 2 ;; esac

# Required external configuration for lanes that hit the real stack. Names only, never values.
if [ "$MODE" = all ] || [ "$MODE" = characterization ]; then
  MISSING_ENV=""
  for v in TIHOMO_BACKEND_URLS TIHOMO_TEST_PG TIHOMO_TEST_DB_COREFINANCE TIHOMO_TEST_DB_PLANNING TIHOMO_TEST_IDENTITY_DB \
           TIHOMO_TEST_RABBITMQ TIHOMO_TEST_JWT_SECRET TIHOMO_TEST_JWT_ISSUER TIHOMO_TEST_JWT_AUDIENCE; do
    [ -n "${!v:-}" ] || MISSING_ENV="$MISSING_ENV $v"
  done
  [ -z "$MISSING_ENV" ] || { echo "FAIL: --mode $MODE needs the real stack; missing required environment:$MISSING_ENV (use run-backend-characterization.sh)" >&2; exit 2; }
fi

cd "$(dirname "$0")" || exit 2

# Scoped lanes: explicit suite lists. The unit filter lives ONLY in unit-tests.runsettings (TestCaseFilter), never duplicated here.
UNIT_SETTINGS="$PWD/unit-tests.runsettings"
UNIT_LANE=(CoreFinance.Application.Tests Identity.Application.Tests Shared.EntityFramework.Tests)
CHAR_LANE=(CoreFinance.Api.Tests Identity.Application.Tests MoneyManagement.Application.Tests PlanningInvestment.Application.Tests)
FULL_EMPTY_OK=(CoreFinance.Tests)   # the only suite approved to report no-tests-discovered, full mode only
in_list() { local x=$1 i; shift; for i in "$@"; do [ "$i" = "$x" ] && return 0; done; return 1; }
command -v python3 >/dev/null || { echo "python3 is required to validate TRX files" >&2; exit 2; }
# trx_counts <file>: prints "total passed failed notExecuted" derived from the ACTUAL UnitTestResult elements, or exits 1 with a reason.
# Rejects: not well-formed XML, root != TestRun, not exactly one ResultSummary/Counters, Aborted/unknown summary outcome, Counters that
# disagree with the actual results, a summary outcome that disagrees with them, or any result outcome other than Passed/Failed/NotExecuted.
trx_counts() {
  python3 - "$1" <<'PY'
import sys, xml.etree.ElementTree as ET
def bad(m): sys.stderr.write(m); sys.exit(1)
try: root = ET.parse(sys.argv[1]).getroot()
except Exception as e: bad("not well-formed XML: %s" % e)
loc = lambda e: e.tag.rsplit('}', 1)[-1]
if loc(root) != "TestRun": bad("root element is %s, not TestRun" % loc(root))
summ = [e for e in root.iter() if loc(e) == "ResultSummary"]
if len(summ) != 1: bad("expected exactly one ResultSummary, found %d" % len(summ))
outcome = summ[0].get("outcome")
if outcome not in ("Completed", "Failed"): bad("ResultSummary outcome '%s' (aborted/unknown)" % outcome)
cs = [e for e in summ[0] if loc(e) == "Counters"]
if len(cs) != 1: bad("expected exactly one Counters in ResultSummary, found %d" % len(cs))
try: c = {k: int(cs[0].attrib[k]) for k in ("total", "executed", "passed", "failed", "notExecuted")}
except Exception as e: bad("Counters attributes missing or non-numeric: %s" % e)
res = [e.get("outcome") for e in root.iter() if loc(e) == "UnitTestResult"]
n, ap, af, an = len(res), res.count("Passed"), res.count("Failed"), res.count("NotExecuted")
if n != ap + af + an: bad("unsupported UnitTestResult outcome(s): %s" % sorted(set(res) - {"Passed", "Failed", "NotExecuted"}))
if (c["total"], c["passed"], c["failed"], c["notExecuted"]) != (n, ap, af, an): bad("Counters %s disagree with actual results total=%d passed=%d failed=%d notExecuted=%d" % (c, n, ap, af, an))
if (outcome == "Failed") != (af > 0): bad("summary outcome %s disagrees with %d failed results" % (outcome, af))
print(n, ap, af, an)
PY
}
[ "$MODE" != unit ] || [ -f "$UNIT_SETTINGS" ] || { echo "missing $UNIT_SETTINGS" >&2; exit 2; }
# Isolated CLI home, but one shared package cache outside the results dir (a per-run cache is ~1.6 GB and fills tmpfs)
export DOTNET_ROOT="$SDK_ROOT" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_CLI_HOME="$RESULTS_DIR/.dotnet-home"
export NUGET_PACKAGES="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
export PATH="$SDK_ROOT:$PATH"
# WSL/Linux inotify limit (128) breaks ASP.NET test hosts; leave no idle MSBuild nodes holding memory
export DOTNET_USE_POLLING_FILE_WATCHER=1 MSBUILDDISABLENODEREUSE=1
mkdir -p "$RESULTS_DIR" || exit 2

# ---- explicit inventory (paths relative to src/be) ----
SUITES=(
  CoreFinance/CoreFinance.Api.Tests/CoreFinance.Api.Tests.csproj
  CoreFinance/CoreFinance.Application.Tests/CoreFinance.Application.Tests.csproj
  CoreFinance/CoreFinance.Tests/CoreFinance.Tests.csproj
  Identity/Identity.Api.Tests/Identity.Api.Tests.csproj
  Identity/Identity.Application.Tests/Identity.Application.Tests.csproj
  MoneyManagement/MoneyManagement.Application.Tests/MoneyManagement.Application.Tests.csproj
  PlanningInvestment/PlanningInvestment.Application.Tests/PlanningInvestment.Application.Tests.csproj
  Shared.EntityFramework.Tests/Shared.EntityFramework.Tests.csproj
)
NON_SUITES=(
  CoreFinance/CoreFinance.Api/CoreFinance.Api.csproj
  CoreFinance/CoreFinance.Application/CoreFinance.Application.csproj
  CoreFinance/CoreFinance.Contracts/CoreFinance.Contracts.csproj
  CoreFinance/CoreFinance.Domain/CoreFinance.Domain.csproj
  CoreFinance/CoreFinance.Infrastructure/CoreFinance.Infrastructure.csproj
  Identity/Identity.Api/Identity.Api.csproj
  Identity/Identity.Application/Identity.Application.csproj
  Identity/Identity.Contracts/Identity.Contracts.csproj
  Identity/Identity.Domain/Identity.Domain.csproj
  Identity/Identity.Infrastructure/Identity.Infrastructure.csproj
  MoneyManagement/MoneyManagement.Api/MoneyManagement.Api.csproj
  MoneyManagement/MoneyManagement.Application/MoneyManagement.Application.csproj
  MoneyManagement/MoneyManagement.Contracts/MoneyManagement.Contracts.csproj
  MoneyManagement/MoneyManagement.Domain/MoneyManagement.Domain.csproj
  MoneyManagement/MoneyManagement.Infrastructure/MoneyManagement.Infrastructure.csproj
  PlanningInvestment/PlanningInvestment.Api/PlanningInvestment.Api.csproj
  PlanningInvestment/PlanningInvestment.Application/PlanningInvestment.Application.csproj
  PlanningInvestment/PlanningInvestment.Contracts/PlanningInvestment.Contracts.csproj
  PlanningInvestment/PlanningInvestment.Domain/PlanningInvestment.Domain.csproj
  PlanningInvestment/PlanningInvestment.Infrastructure/PlanningInvestment.Infrastructure.csproj
  ExcelApi/ExcelApi.csproj
  Ocelot.Gateway/Ocelot.Gateway.csproj
  MessageQueueTester/MessageQueueTester.csproj
  Tests.Shared/Tests.Shared.csproj
)
# Shared-source marker: an intentionally empty csproj (no TFM), compiled via consumers' projitems.
MARKERS=(Shared/Shared.Contracts/Shared.Contracts.csproj)

FAILED=0 EXCEPTIONS=0
fail() { echo "FAIL: $*"; FAILED=1; }

# ---- inventory vs discovery: never silently skip a project ----
INVENTORY=$(printf '%s\n' "${SUITES[@]}" "${NON_SUITES[@]}" "${MARKERS[@]}" | sort)
DISCOVERED=$(find . -name '*.csproj' -not -path '*/obj/*' -not -path '*/bin/*' -not -path '*/node_modules/*' | sed 's|^\./||' | sort)
MISSING=$(comm -23 <(echo "$INVENTORY") <(echo "$DISCOVERED"))
NEW=$(comm -13 <(echo "$INVENTORY") <(echo "$DISCOVERED"))
[ -z "$MISSING" ] || fail "inventory project missing on disk: $MISSING"
[ -z "$NEW" ] || fail "project on disk not in inventory: $NEW"

for m in "${MARKERS[@]}"; do
  [ -s "$m" ] && fail "marker $m is no longer empty; reclassify it"
done

{
  echo "date=$(date -u +%FT%TZ)"
  echo "revision=$(git rev-parse HEAD 2>/dev/null) dirty=$(git status --short . 2>/dev/null | wc -l)"
  echo "sdk=$(dotnet --version)"
  dotnet --list-runtimes
  echo "mode=$MODE suites=${#SUITES[@]} nonSuites=${#NON_SUITES[@]} markers=${#MARKERS[@]}"
} | tee "$RESULTS_DIR/environment.txt"

name_of() { basename "$1" .csproj; }
SUMMARY="$RESULTS_DIR/summary.tsv"
printf 'project\tkind\tbuild\ttest\n' > "$SUMMARY"

# Build in dependency order via the test/api projects: build each project explicitly, never skip on failure.
mkdir -p "$RESULTS_DIR/build"
declare -A BUILD_RESULT
for p in "${NON_SUITES[@]}" "${SUITES[@]}"; do
  n=$(name_of "$p")
  if dotnet build "$p" -nologo -nodeReuse:false -v:minimal -bl:"$RESULTS_DIR/build/$n.binlog" > "$RESULTS_DIR/build/$n.log" 2>&1; then
    BUILD_RESULT[$p]=pass
  else
    BUILD_RESULT[$p]=FAIL; fail "build $p (see $RESULTS_DIR/build/$n.log)"
  fi
done

for p in "${NON_SUITES[@]}"; do printf '%s\tproject\t%s\t-\n' "$(name_of "$p")" "${BUILD_RESULT[$p]}" >> "$SUMMARY"; done

# Per-suite failure exceptions approved by the user live in known-failures.<Suite>.txt (one escaped TRX testName per line); full mode only.
# Rules: no new failure, no unexpected pass, no skipped/not-executed test, no vanished test. Empty suites are reported, never passed.
# A suite counts as run only with a fresh, parseable TRX and a consistent dotnet test exit code; zero discovery is never a pass.
for p in "${SUITES[@]}"; do
  n=$(name_of "$p"); t="not-run"; args=(); selected=1
  case "$MODE" in
    all) ;;
    unit) if in_list "$n" "${UNIT_LANE[@]}"; then args=(--settings "$UNIT_SETTINGS"); else selected=0; fi ;;
    characterization) if in_list "$n" "${CHAR_LANE[@]}"; then args=(--filter "Category=Characterization"); else selected=0; fi ;;
    build) selected=0 ;;
  esac
  if [ "$MODE" = build ]; then t="not-run"
  elif [ "$selected" = 0 ]; then t="outside-lane"
  elif [ "${BUILD_RESULT[$p]}" != pass ]; then t=FAIL
  else
    d="$RESULTS_DIR/trx/$n"; rm -rf "$d"; mkdir -p "$d"
    dotnet test "$p" --no-build --no-restore "${args[@]}" --logger "trx;LogFileName=$n.trx" --results-directory "$d" > "$d/console.log" 2>&1
    rc=$?; trx="$d/$n.trx"
    empty_ok=0; [ "$MODE" = all ] && in_list "$n" "${FULL_EMPTY_OK[@]}" && empty_ok=1
    if [ ! -f "$trx" ]; then
      if [ "$rc" -eq 0 ] && grep -q "No test is available" "$d/console.log" && [ "$empty_ok" = 1 ]; then
        t="no-tests-discovered"   # not a pass
      else
        t=FAIL; fail "suite $p: no TRX or zero discovery not allowed here (dotnet test rc=$rc; see $d/console.log)"
      fi
    else
      # Validate the TRX with a real XML parser: root TestRun, ResultSummary outcome, and Counters must equal the actual UnitTestResult outcomes.
      if ! counts=$(trx_counts "$trx" 2>"$d/trx-error.txt"); then
        t=FAIL; fail "suite $p: invalid TRX ($(cat "$d/trx-error.txt")); see $trx"; printf '%s\tsuite\t%s\t%s\n' "$n" "${BUILD_RESULT[$p]}" "$t" >> "$SUMMARY"; continue
      fi
      read -r total passed failed notexec <<<"$counts"
      kf=/dev/null; [ "$MODE" = all ] && [ -f "known-failures.$n.txt" ] && kf="known-failures.$n.txt"
      grep -oP '<UnitTestResult [^>]*?testName="\K[^"]*(?="[^>]*?outcome="Failed")' "$trx" | sort > "$d/failed.txt"
      sort "$kf" > "$d/known.sorted"
      newfail=$(comm -13 "$d/known.sorted" "$d/failed.txt"); unexpected_pass=$(comm -23 "$d/known.sorted" "$d/failed.txt")
      if [ "$(wc -l < "$d/failed.txt")" -ne "$failed" ]; then t=FAIL; fail "suite $p: failed-name extraction ($(wc -l < "$d/failed.txt")) disagrees with TRX failed count ($failed)"
      elif grep -q "Test Run Aborted" "$d/console.log"; then t=FAIL; fail "suite $p: test run aborted (testhost crash; see $d/console.log)"
      elif [ "$notexec" -ne 0 ]; then t=FAIL; fail "suite $p has $notexec not-executed/skipped tests"
      elif [ -n "$newfail" ]; then t=FAIL; fail "suite $p new failures: $(wc -l <<<"$newfail") (see $d/failed.txt)"
      elif [ -n "$unexpected_pass" ]; then t=FAIL; fail "suite $p known failures now pass or vanished (investigate, do not force): $(wc -l <<<"$unexpected_pass")"
      elif [ "$failed" -gt 0 ] && [ "$rc" -eq 0 ]; then t=FAIL; fail "suite $p: $failed failed results but dotnet test rc=0 (inconsistent; see $d/console.log)"
      elif [ "$failed" -gt 0 ]; then t="pass+known-failures($failed/$total)"; EXCEPTIONS=1
      elif [ "$rc" -ne 0 ]; then t=FAIL; fail "suite $p: dotnet test rc=$rc with no failed result (testhost/infra error; see $d/console.log)"
      elif [ "$total" -gt 0 ]; then t="pass($passed/$total)"
      elif [ "$empty_ok" = 1 ]; then t="no-tests-discovered"
      else t=FAIL; fail "suite $p selected 0 tests in --mode $MODE"; fi
    fi
  fi
  printf '%s\tsuite\t%s\t%s\n' "$n" "${BUILD_RESULT[$p]}" "$t" >> "$SUMMARY"
done


for m in "${MARKERS[@]}"; do printf '%s\tshared-source-marker\t-\t-\n' "$(name_of "$m")" >> "$SUMMARY"; done

echo; echo "== summary ($SUMMARY)"; column -t -s $'\t' "$SUMMARY"
case "$MODE" in
  build) echo "NOTE: --mode build is a checkpoint, not full-suite acceptance." ;;
  unit) echo "NOTE: lane=unit settings=$UNIT_SETTINGS targeted=${UNIT_LANE[*]}; outside-lane suites were NOT run. PASS here is not whole-backend acceptance (use --mode all)." ;;
  characterization) echo "NOTE: lane=characterization filter=Category=Characterization targeted=${CHAR_LANE[*]}; outside-lane suites were NOT run, frozen44 not applied. PASS here is not whole-backend acceptance (use --mode all)." ;;
esac
if [ "$FAILED" -ne 0 ]; then echo "RESULT: FAIL"; exit 1; fi
if [ "$EXCEPTIONS" -ne 0 ]; then echo "RESULT: PASS WITH APPROVED BASELINE EXCEPTIONS (known-failures.*.txt; not full green)"; else echo "RESULT: PASS"; fi
