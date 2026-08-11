using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanningInvestment.Domain.Entities;

#pragma warning disable CS8618, CS9264

namespace PlanningInvestment.Infrastructure;

/// <summary>
///     Database context for the Planning Investment application (EN)<br />
///     Ngữ cảnh cơ sở dữ liệu cho ứng dụng Lập kế hoạch Đầu tư (VI)
/// </summary>
public class PlanningInvestmentDbContext : DbContext
{
    /// <summary>
    ///     The default connection string name for the database (EN)<br />
    ///     Tên chuỗi kết nối mặc định cho cơ sở dữ liệu (VI)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string DEFAULT_CONNECTION_STRING = "PlanningInvestmentDb";

    // ReSharper disable once NotAccessedField.Local
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the PlanningInvestmentDbContext class (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp PlanningInvestmentDbContext (VI)
    /// </summary>
    public PlanningInvestmentDbContext()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the PlanningInvestmentDbContext class with specified options and configuration (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp PlanningInvestmentDbContext với các tùy chọn và cấu hình được chỉ định (VI)
    /// </summary>
    /// <param name="options">The DbContext options (EN)<br />Các tùy chọn DbContext (VI)</param>
    /// <param name="configuration">The application configuration (EN)<br />Cấu hình ứng dụng (VI)</param>
    public PlanningInvestmentDbContext(DbContextOptions<PlanningInvestmentDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets or sets the DbSet for Debts (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Khoản nợ (VI)
    /// </summary>
    public DbSet<Debt> Debts { get; set; }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>
    ///     Configures the database model for the Planning Investment context (EN)<br />
    ///     Cấu hình mô hình cơ sở dữ liệu cho ngữ cảnh Lập kế hoạch Đầu tư (VI)
    /// </summary>
    /// <param name="modelBuilder">The model builder instance (EN)<br />Phiên bản xây dựng mô hình (VI)</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    ///     Configures the database connection if not already configured (EN)<br />
    ///     Cấu hình kết nối cơ sở dữ liệu nếu chưa được cấu hình (VI)
    /// </summary>
    /// <param name="optionsBuilder">The options builder (EN)<br />Trình xây dựng tùy chọn (VI)</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        var connectionString = _configuration.GetConnectionString(DEFAULT_CONNECTION_STRING);
        optionsBuilder.UseNpgsql(connectionString);
    }
}
