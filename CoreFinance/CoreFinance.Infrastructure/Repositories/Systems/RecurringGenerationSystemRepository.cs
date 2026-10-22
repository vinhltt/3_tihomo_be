using CoreFinance.Domain.BaseRepositories.Systems;
using Microsoft.EntityFrameworkCore;

namespace CoreFinance.Infrastructure.Repositories.Systems;

/// <summary>
///     (EN) PostgreSQL implementation of <see cref="IRecurringGenerationSystemRepository" />.<br />
///     (VI) Triển khai PostgreSQL của <see cref="IRecurringGenerationSystemRepository" />.
/// </summary>
public class RecurringGenerationSystemRepository(CoreFinanceDbContext context) : IRecurringGenerationSystemRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetAutoGenerateOwnerIdsAsync(CancellationToken cancellationToken = default)
    {
        return await context.RecurringTransactionTemplates.AsNoTracking()
            .Where(t => t.IsActive && t.AutoGenerate && t.UserId != null)
            .Select(t => t.UserId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
