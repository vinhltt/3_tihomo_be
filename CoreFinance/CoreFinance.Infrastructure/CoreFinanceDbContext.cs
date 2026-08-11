using CoreFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.EntityFramework.Extensions;

#pragma warning disable CS8618, CS9264

namespace CoreFinance.Infrastructure;

/// <summary>
///     (EN) Represents the database context for the Core Finance application.<br />
///     (VI) Biểu thị ngữ cảnh cơ sở dữ liệu cho ứng dụng Core Finance.
/// </summary>
public class CoreFinanceDbContext : DbContext
{
    /// <summary>
    ///     (EN) The default connection string name for the database.<br />
    ///     (VI) Tên chuỗi kết nối mặc định cho cơ sở dữ liệu.
    /// </summary>
    public const string DEFAULT_CONNECTION_STRING = "CoreFinanceDb";

    // ReSharper disable once NotAccessedField.Local
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     (EN) Initializes a new instance of the <see cref="CoreFinanceDbContext" /> class.<br />
    ///     (VI) Khởi tạo một phiên bản mới của lớp <see cref="CoreFinanceDbContext" />.
    /// </summary>
    public CoreFinanceDbContext()
    {
    }
    public CoreFinanceDbContext(DbContextOptions<CoreFinanceDbContext> options)
    {
    }

    /// <summary>
    ///     (EN) Initializes a new instance of the <see cref="CoreFinanceDbContext" /> class with specified options and
    ///     configuration.<br />
    ///     (VI) Khởi tạo một phiên bản mới của lớp <see cref="CoreFinanceDbContext" /> với các tùy chọn và cấu hình được chỉ
    ///     định.
    /// </summary>
    /// <param name="options">
    ///     The DbContext options. (EN)<br />
    ///     Các tùy chọn DbContext. (VI)
    /// </param>
    /// <param name="configuration">
    ///     The application configuration. (EN)<br />
    ///     Cấu hình ứng dụng. (VI)
    /// </param>
    public CoreFinanceDbContext(DbContextOptions<CoreFinanceDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     (EN) Gets or sets the DbSet for Accounts.<br />
    ///     (VI) Lấy hoặc thiết lập DbSet cho các Tài khoản.
    /// </summary>
    public DbSet<Account> Accounts { get; set; }

    /// <summary>
    ///     (EN) Gets or sets the DbSet for Transactions.<br />
    ///     (VI) Lấy hoặc thiết lập DbSet cho các Giao dịch.
    /// </summary>
    public DbSet<Transaction> Transactions { get; set; }

    /// <summary>
    ///     (EN) Gets or sets the DbSet for ExpectedTransactions.<br />
    ///     (VI) Lấy hoặc thiết lập DbSet cho các Giao dịch Dự kiến.
    /// </summary>
    public DbSet<ExpectedTransaction> ExpectedTransactions { get; set; }

    /// <summary>
    ///     (EN) Gets or sets the DbSet for RecurringTransactionTemplates.<br />
    ///     (VI) Lấy hoặc thiết lập DbSet cho các Mẫu Giao dịch Định kỳ.
    /// </summary>
    public DbSet<RecurringTransactionTemplate> RecurringTransactionTemplates { get; set; }

    /// <summary>
    ///     (EN) Configures the database context.<br />
    ///     (VI) Cấu hình ngữ cảnh cơ sở dữ liệu.
    /// </summary>
    /// <param name="optionsBuilder">
    ///     The options builder. (EN)<br />
    ///     Bộ xây dựng tùy chọn. (VI)
    /// </param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        var connectionString = _configuration.GetConnectionString(DEFAULT_CONNECTION_STRING);
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        modelBuilder.UseQueryFilter();

        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            // Add unique index on Code (partial index for nullable column)
            // (EN) Unique index ensures no duplicate codes across accounts
            // (VI) Index duy nhất đảm bảo không có mã trùng lặp giữa các tài khoản
            entity.HasIndex(a => a.Code)
                  .IsUnique()
                  .HasFilter("code IS NOT NULL")  // PostgreSQL syntax, snake_case naming
                  .HasDatabaseName("ix_accounts_code");
        });
    }
}