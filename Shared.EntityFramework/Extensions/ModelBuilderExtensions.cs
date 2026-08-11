using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework.BaseEfModels;

namespace Shared.EntityFramework.Extensions;

/// <summary>
///     Extensions for configuring model builders.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ConfigureFilterSoftDelete<T>(
        ModelBuilder builder
    )
        where T : BaseEntity
    {
        builder.Entity<T>()
            .HasQueryFilter(e => e.IsDeleted == null);
    }

    /// <summary>
    ///     Applies the soft delete query filter to all entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The model builder with soft delete filter applied.</returns>
    public static ModelBuilder UseQueryFilter(
        this ModelBuilder modelBuilder
    )
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(entityType =>
                typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            var method = typeof(ModelBuilderExtensions)
                .GetMethod(nameof(ConfigureFilterSoftDelete));

            if (method == null)
                throw new InvalidOperationException("Method {nameof(ConfigureFilterSoftDelete)} not found.");

            var genericMethod = method.MakeGenericMethod(entityType.ClrType);
            genericMethod.Invoke(null, [modelBuilder]);
        }

        return modelBuilder;
    }
}