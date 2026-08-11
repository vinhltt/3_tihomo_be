namespace MoneyManagement.Application.DTOs.Jar;

public class JarBalanceSummary
{
    public Guid UserId { get; set; }
    public List<JarBalanceDetail> JarBalances { get; set; } = [];
    public decimal TotalBalance { get; set; }
    public decimal TotalTarget { get; set; }
    public decimal OverallProgress { get; set; }
    public DateTime GeneratedAt { get; set; }
    public JarBalanceStatistics Statistics { get; set; } = new();
}

public class JarBalanceDetail
{
    public Guid JarId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JarType { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal AllocationPercentage { get; set; }
    public decimal Progress { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public decimal MonthlyContribution { get; set; }
    public decimal? EstimatedTargetDate { get; set; }
}

public class JarBalanceStatistics
{
    public int TotalJars { get; set; }
    public int ActiveJars { get; set; }
    public int JarsAtTarget { get; set; }
    public int JarsNearTarget { get; set; }
    public decimal AverageProgress { get; set; }
    public string TopPerformingJar { get; set; } = string.Empty;
    public string LeastProgressJar { get; set; } = string.Empty;
    public decimal TotalMonthlyAllocation { get; set; }
}