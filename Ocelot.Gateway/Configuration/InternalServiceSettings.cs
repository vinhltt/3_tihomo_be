namespace Ocelot.Gateway.Configuration
{
    public class InternalServiceSettings
    {
        public const string SectionName = "InternalServiceSettings";
        public string? IdentityService { get; set; }
        public string? CoreFinanceService { get; set; }
        public string? MoneyManagementService { get; set; }
        public string? PlanningInvestmentService { get; set; }
        public string? ExcelService { get; set; }
        
        public string? GetServiceUrl(string serviceName)
        {
            return serviceName.ToLower() switch
            {
                "identity" => IdentityService,
                "core-finance" => CoreFinanceService,
                "money-management" => MoneyManagementService,
                "planning-investment" => PlanningInvestmentService,
                "excel" => ExcelService,
                _ => null
            };
        }
    }
}
