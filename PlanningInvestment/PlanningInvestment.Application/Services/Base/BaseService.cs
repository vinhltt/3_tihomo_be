using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlanningInvestment.Domain.UnitOfWorks;
using Shared.EntityFramework.BaseEfModels;
using Shared.Contracts.Exceptions;
using Shared.Contracts.Utilities;
using Shared.EntityFramework.DTOs;

namespace PlanningInvestment.Application.Services.Base;

/// <summary>
///     Base service class providing common CRUD operations for entities. (EN)<br />
///     Lớp dịch vụ cơ bản cung cấp các thao tác CRUD chung cho các thực thể. (VI)
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TCreateRequest">The type of the create request DTO.</typeparam>
/// <typeparam name="TUpdateRequest">The type of the update request DTO.</typeparam>
/// <typeparam name="TViewModel">The type of the view model DTO.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public abstract class BaseService<TEntity, TCreateRequest, TUpdateRequest, TViewModel,
    TKey>(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger logger
)
    : IBaseService<TEntity, TCreateRequest, TUpdateRequest, TViewModel, TKey>
    where TEntity : BaseEntity<TKey>, new()
    where TCreateRequest : BaseCreateRequest, new()
    where TUpdateRequest : BaseUpdateRequest<TKey>, new()
    where TViewModel : BaseViewModel<TKey>, new()
{
    protected readonly IMapper Mapper = mapper;

    /// <summary>
    ///     Deletes an entity permanently by its identifier asynchronously. (EN)<br />
    ///     Xóa vĩnh viễn một thực thể dựa trên định danh của nó một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    public async Task<int?> DeleteHardAsync(TKey id)
    {
        logger.LogTrace("{DeleteHardAsync} request: {id}", nameof(DeleteHardAsync), id);
        return await unitOfWork.Repository<TEntity, TKey>().DeleteHardAsync(id!);
    }

    /// <summary>
    ///     Soft deletes an entity by its identifier asynchronously. (EN)<br />
    ///     Xóa mềm một thực thể dựa trên định danh của nó một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    public async Task<int?> DeleteSoftAsync(TKey id)
    {
        logger.LogTrace("{DeleteSoftAsync} request: {id}", nameof(DeleteSoftAsync), id);
        return await unitOfWork.Repository<TEntity, TKey>().DeleteSoftAsync(id!);
    }

    /// <summary>
    ///     Gets all entities as view models asynchronously. (EN)<br />
    ///     Lấy tất cả các thực thể dưới dạng view model một cách bất đồng bộ. (VI)
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of view models.</returns>
    public virtual async Task<IEnumerable<TViewModel>?> GetAllDtoAsync()
    {
        var query = unitOfWork.Repository<TEntity, TKey>();
        var result = await Mapper.ProjectTo<TViewModel>(query.GetNoTrackingEntities()).ToListAsync();
        logger.LogTrace("{GetAllDtoAsync} result: {result}", nameof(GetAllDtoAsync), result.TryParseToString());
        return result;
    }

    /// <summary>
    ///     Gets an entity by its identifier as a view model asynchronously. (EN)<br />
    ///     Lấy một thực thể dựa trên định danh của nó dưới dạng view model một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the view model, or null if the
    ///     entity is not found.
    /// </returns>
    public virtual async Task<TViewModel?> GetByIdAsync(TKey id)
    {
        logger.LogTrace("{GetByIdAsync} request: {id}", nameof(GetByIdAsync), id.TryParseToString());
        var entity = await unitOfWork.Repository<TEntity, TKey>().GetByIdNoTrackingAsync(id);
        var result = Mapper.Map<TViewModel>(entity);
        logger.LogTrace("{GetByIdAsync} result: {result}", nameof(GetByIdAsync), result.TryParseToString());
        return result;
    }

    /// <summary>
    ///     Updates an entity asynchronously. (EN)<br />
    ///     Cập nhật một thực thể một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="request">The update request containing the new data.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the updated view model, or null if
    ///     the update failed.
    /// </returns>
    public virtual async Task<TViewModel?> UpdateAsync(TKey id, TUpdateRequest request)
    {
        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {
            logger.LogTrace("{UpdateAsync} request: {id}, {request}", nameof(UpdateAsync), id,
                request.TryParseToString());
            if (id is null || !id.Equals(request.Id))
                throw new KeyNotFoundException();
            var entity = await unitOfWork.Repository<TEntity, TKey>().GetByIdAsync(id);
            logger.LogTrace("{UpdateAsync} old entity: {entity}", nameof(UpdateAsync), entity.TryParseToString());

            if (entity == null)
            {
                logger.LogError("{UpdateAsync} Entity not found for ID: {id}", nameof(UpdateAsync),
                    id.TryParseToString());
                throw new EntityNotFoundException($"Entity with ID {id} not found.");
            }

            entity = Mapper.Map(request, entity);
            logger.LogTrace("{UpdateAsync} new entity: {entity}", nameof(UpdateAsync), entity.TryParseToString());
            var effectedCount = await unitOfWork.Repository<TEntity, TKey>().UpdateAsync(entity);
            logger.LogTrace("{UpdateAsync} effectedCount: {effectedCount}", nameof(UpdateAsync), effectedCount);
            if (effectedCount <= 0) throw new UpdateFailedException();
            var result = Mapper.Map<TViewModel>(entity);
            logger.LogTrace("{UpdateAsync} result: {result}", nameof(UpdateAsync), result.TryParseToString());
            await trans.CommitAsync();
            return result;
        }
        catch
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    ///     Creates a new entity asynchronously. (EN)<br />
    ///     Tạo một thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="request">The create request containing the data for the new entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the created view model, or null if
    ///     the creation failed.
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    public virtual async Task<TViewModel?> CreateAsync(TCreateRequest request)
    {
        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {
            logger.LogTrace("{CreateAsync} request: {request}", nameof(CreateAsync), request.TryParseToString());
            var entityNew = new TEntity();
            Mapper.Map(request, entityNew);
            logger.LogTrace("{CreateAsync} entitiesNew: {entityNew}", nameof(CreateAsync),
                entityNew.TryParseToString());
            var effectedCount =
                await unitOfWork.Repository<TEntity, TKey>().CreateAsync(entityNew);
            logger.LogTrace("{CreateAsync} affectedCount: {effectedCount}", nameof(CreateAsync), effectedCount);
            if (effectedCount <= 0) throw new CreateFailedException();
            var result = Mapper.Map<TViewModel>(entityNew);
            logger.LogTrace("{CreateAsync} result: {result}", nameof(CreateAsync), result.TryParseToString());
            await trans.CommitAsync();
            return result;
        }
        catch
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    ///     Creates multiple new entities asynchronously. (EN)<br />
    ///     Tạo nhiều thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="request">The list of create requests containing the data for the new entities.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the collection of created view
    ///     models, or an empty collection if the creation failed.
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    public virtual async Task<IEnumerable<TViewModel>?> CreateAsync(
        List<TCreateRequest> request)
    {
        if (!request.Any())
        {
            logger.LogInformation("Request Is Empty");
            return new List<TViewModel>();
        }

        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {
            var baseCreateRequests = request as TCreateRequest[] ?? request.ToArray();
            logger.LogTrace("{CreateAsync} request: {baseCreateRequests}", nameof(CreateAsync),
                baseCreateRequests.TryParseToString());

            var entitiesNew = new List<TEntity>();
            Mapper.Map(baseCreateRequests, entitiesNew);
            logger.LogTrace("{CreateAsync} entitiesNew: {entitiesNew}", nameof(CreateAsync),
                entitiesNew.TryParseToString());

            var effectedCount =
                await unitOfWork.Repository<TEntity, TKey>().CreateAsync(entitiesNew);
            logger.LogTrace("{CreateAsync} affectedCount: {effectedCount}", nameof(CreateAsync), effectedCount);

            if (effectedCount <= 0) throw new CreateFailedException();

            var result = Mapper.Map<IEnumerable<TViewModel>>(entitiesNew);
            var baseViewModels = result as TViewModel[] ?? result.ToArray();
            logger.LogTrace("{CreateAsync} result: {baseViewModels}", nameof(CreateAsync),
                baseViewModels.TryParseToString());
            await trans.CommitAsync();

            return baseViewModels;
        }
        catch
        {
            await trans.CommitAsync();
            throw;
        }
    }
}