namespace Ocelot.Gateway.Configuration
{
    public class ServicePorts
    {
        public const string SectionName = "ServicePorts";

        public int? IdentitySsoPort { get; set; }
        public int? CoreFinanceApiPort { get; set; }
        public int? MoneyManagementApiPort { get; set; }
        public int? PlanningInvestmentApiPort { get; set; }
        public int? ExcelApiPort { get; set; }
        public int? GatewayPort  { get; set; }
    }
}
