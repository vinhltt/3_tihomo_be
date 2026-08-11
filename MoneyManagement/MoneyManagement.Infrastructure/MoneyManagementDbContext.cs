using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoneyManagement.Contracts.Extensions;
using MoneyManagement.Domain.Entities;

#pragma warning disable CS8618, CS9264

namespace MoneyManagement.Infrastructure;

/// <summary>
///     Database context for the Money Management application (EN)<br />
///     Ngữ cảnh cơ sở dữ liệu cho ứng dụng Quản lý Tiền (VI)
/// </summary>
public class MoneyManagementDbContext : DbContext
{
    /// <summary>
    ///     The default connection string name for the database (EN)<br />
    ///     Tên chuỗi kết nối mặc định cho cơ sở dữ liệu (VI)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string DEFAULT_CONNECTION_STRING = "MoneyManagementDb";

    // ReSharper disable once NotAccessedField.Local
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the MoneyManagementDbContext class (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp MoneyManagementDbContext (VI)
    /// </summary>
    public MoneyManagementDbContext()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the MoneyManagementDbContext class with specified options and configuration (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp MoneyManagementDbContext với các tùy chọn và cấu hình được chỉ định (VI)
    /// </summary>
    /// <param name="options">The DbContext options (EN)<br />Các tùy chọn DbContext (VI)</param>
    /// <param name="configuration">The application configuration (EN)<br />Cấu hình ứng dụng (VI)</param>
    public MoneyManagementDbContext(DbContextOptions<MoneyManagementDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets or sets the DbSet for Budgets (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Ngân sách (VI)
    /// </summary>
    public DbSet<Budget> Budgets { get; set; }

    /// <summary>
    ///     Gets or sets the DbSet for Jars (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Lọ (VI)
    /// </summary>
    public DbSet<Jar> Jars { get; set; }

    /// <summary>
    ///     Gets or sets the DbSet for SharedExpenses (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Chi tiêu Chung (VI)
    /// </summary>
    public DbSet<SharedExpense> SharedExpenses { get; set; }

    /// <summary>
    ///     Gets or sets the DbSet for SharedExpenseParticipants (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Người tham gia Chi tiêu Chung (VI)
    /// </summary>
    public DbSet<SharedExpenseParticipant> SharedExpenseParticipants { get; set; }

    /// <summary>
    ///     Configures the database context (EN)<br />
    ///     Cấu hình ngữ cảnh cơ sở dữ liệu (VI)
    /// </summary>
    /// <param name="optionsBuilder">The options builder (EN)<br />Bộ xây dựng tùy chọn (VI)</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        var connectionString = _configuration.GetConnectionString(DEFAULT_CONNECTION_STRING);
        if (!string.IsNullOrEmpty(connectionString)) optionsBuilder.UseNpgsql(connectionString);
    }

    /// <summary>
    ///     Configures the model and relationships (EN)<br />
    ///     Cấu hình mô hình và các mối quan hệ (VI)
    /// </summary>
    /// <param name="modelBuilder">The model builder (EN)<br />Bộ xây dựng mô hình (VI)</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseQueryFilter();

        // Configure SharedExpense -> SharedExpenseParticipant relationship
        modelBuilder.Entity<SharedExpenseParticipant>()
            .HasOne(p => p.SharedExpense)
            .WithMany(e => e.Participants)
            .HasForeignKey(p => p.SharedExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.UserId, b.Status })
            .HasDatabaseName("IX_Budget_UserId_Status");

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.UserId, b.Period })
            .HasDatabaseName("IX_Budget_UserId_Period");

        modelBuilder.Entity<Jar>()
            .HasIndex(j => new { j.UserId, j.JarType })
            .HasDatabaseName("IX_Jar_UserId_JarType");

        modelBuilder.Entity<SharedExpense>()
            .HasIndex(s => new { s.CreatedByUserId, s.Status })
            .HasDatabaseName("IX_SharedExpense_CreatedByUserId_Status");

        modelBuilder.Entity<SharedExpenseParticipant>()
            .HasIndex(p => new { p.SharedExpenseId, p.UserId })
            .HasDatabaseName("IX_SharedExpenseParticipant_SharedExpenseId_UserId");
    }
}