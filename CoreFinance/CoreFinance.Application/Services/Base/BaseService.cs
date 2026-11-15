using AutoMapper;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.UnitOfWorks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Exceptions;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Application.Services.Base;

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
    ILogger logger,
    IValidator<TCreateRequest> createValidator,
    IValidator<TUpdateRequest> updateValidator
)
    : IBaseService<TEntity, TCreateRequest, TUpdateRequest, TViewModel, TKey>
    where TEntity : UserOwnedEntity<TKey>, new()
    where TCreateRequest : BaseCreateRequest, new()
    where TUpdateRequest : BaseUpdateRequest<TKey>, new()
    where TViewModel : BaseViewModel<TKey>, new()
{
    protected readonly IMapper Mapper = mapper;

    /// <summary>
    ///     Validates a create request and throws when any rule fails. (EN)<br />
    ///     Kiểm tra request tạo mới và ném lỗi khi có rule không đạt. (VI)
    /// </summary>
    /// <param name="request">The create request to validate.</param>
    /// <exception cref="ValidationException">Thrown when the request violates a validation rule.</exception>
    protected void ValidateCreate(TCreateRequest request) => createValidator.ValidateAndThrow(request);

    private InlineValidator<List<TCreateRequest>>? _createRangeValidator;

    /// <summary>
    ///     Validates a batch of create requests and throws one exception carrying the errors of every invalid item. (EN)<br />
    ///     Kiểm tra lô request tạo mới và ném một lỗi duy nhất chứa lỗi của mọi phần tử sai. (VI)
    /// </summary>
    /// <param name="requests">The create requests to validate.</param>
    /// <exception cref="ValidationException">
    ///     Thrown when the batch exceeds <see cref="CoreFinanceMessages.MaxBatchSize" /> items or any item violates a
    ///     validation rule.
    /// </exception>
    protected void ValidateCreateRange(List<TCreateRequest> requests)
    {
        _createRangeValidator ??= BuildCreateRangeValidator();
        _createRangeValidator.ValidateAndThrow(requests);
    }

    private InlineValidator<List<TCreateRequest>> BuildCreateRangeValidator()
    {
        var validator = new InlineValidator<List<TCreateRequest>>();
        validator.RuleFor(x => x.Count).LessThanOrEqualTo(CoreFinanceMessages.MaxBatchSize)
            .WithMessage(CoreFinanceMessages.BatchSizeExceeded);
        // The item error key prefix comes from this lambda parameter name, so keys read "x[i].<Field>".
        validator.RuleForEach(x => x).SetValidator(createValidator)
            .When(x => x.Count <= CoreFinanceMessages.MaxBatchSize);
        return validator;
    }

    /// <summary>
    ///     Validates an update request and throws when any rule fails. (EN)<br />
    ///     Kiểm tra request cập nhật và ném lỗi khi có rule không đạt. (VI)
    /// </summary>
    /// <param name="request">The update request to validate.</param>
    /// <exception cref="ValidationException">Thrown when the request violates a validation rule.</exception>
    protected void ValidateUpdate(TUpdateRequest request) => updateValidator.ValidateAndThrow(request);

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
        logger.LogTrace("{DeleteHardAsync} request: {id}", nameof(DeleteHardAsync), id.TryParseToString());
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
        logger.LogTrace("{DeleteSoftAsync} request: {id}", nameof(DeleteSoftAsync), id.TryParseToString());
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
        logger.LogTrace("{UpdateAsync} request: {id}, {request}", nameof(UpdateAsync), id,
            request.TryParseToString());
        if (id is null || !id.Equals(request.Id))
            throw new KeyNotFoundException();
        ValidateUpdate(request);

        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {
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
            var countAffect = await unitOfWork.Repository<TEntity, TKey>().UpdateAsync(entity);
            logger.LogTrace("{UpdateAsync} updated entity count: {countAffect}", nameof(UpdateAsync),
                countAffect.TryParseToString());
            if (countAffect == 0)
            {
                throw new UpdateFailedException("Failed to update the entity");
            }
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
        ValidateCreate(request);

        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {            
            logger.LogError("AUTOMAPPER DEBUG: Request data JSON: {RequestJson}", System.Text.Json.JsonSerializer.Serialize(request));
            logger.LogTrace("{CreateAsync} request: {request}", nameof(CreateAsync), request.TryParseToString());
            
            var entityNew = new TEntity();
            logger.LogError("AUTOMAPPER DEBUG: Before mapping - entity Name property: {NameValue}", 
                entityNew.GetType().GetProperty("Name")?.GetValue(entityNew));
            
            Mapper.Map(request, entityNew);
            
            logger.LogError("AUTOMAPPER DEBUG: After mapping - entity Name property: {NameValue}", 
                entityNew.GetType().GetProperty("Name")?.GetValue(entityNew));
            
            logger.LogTrace("{CreateAsync} entitiesNew: {entityNew}", nameof(CreateAsync),
                entityNew.TryParseToString());
                
            logger.LogWarning("AUTOMAPPER DEBUG: After mapping - entity Name: '{Name}', Type: {Type}, Currency: '{Currency}', UserId: {UserId}", 
                entityNew.GetType().GetProperty("Name")?.GetValue(entityNew), 
                entityNew.GetType().GetProperty("Type")?.GetValue(entityNew),
                entityNew.GetType().GetProperty("Currency")?.GetValue(entityNew),
                entityNew.GetType().GetProperty("UserId")?.GetValue(entityNew));
            
            logger.LogTrace("{CreateAsync} entitiesNew: {entityNew}", nameof(CreateAsync),
                entityNew.TryParseToString());
            var countAffect =
                await unitOfWork.Repository<TEntity, TKey>().CreateAsync(entityNew);
            logger.LogTrace("{CreateAsync} created entity count: {countAffect}", nameof(CreateAsync),
                countAffect.TryParseToString());

            // Kiểm tra xem có tạo thành công hay không
            if (countAffect == 0)
            {
                logger.LogError("{CreateAsync} No entities were created", nameof(CreateAsync));
                throw new CreateFailedException("Failed to create the entity");
            }

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

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TViewModel>?> CreateAsync(
        List<TCreateRequest> request)
    {
        if (!request.Any())
        {
            logger.LogInformation("Request Is Empty");
            return new List<TViewModel>();
        }

        ValidateCreateRange(request);

        await using var trans = await unitOfWork.BeginTransactionAsync();
        try
        {            var baseCreateRequests = request as TCreateRequest[] ?? request.ToArray();
            logger.LogTrace("{CreateAsync} request: {baseCreateRequests}", nameof(CreateAsync),
                baseCreateRequests.TryParseToString());

            var entitiesNew = Mapper.Map<List<TEntity>>(baseCreateRequests);
            logger.LogTrace("{CreateAsync} entitiesNew: {entitiesNew}", nameof(CreateAsync),
                entitiesNew.TryParseToString());
            var effectedCount =
                await unitOfWork.Repository<TEntity, TKey>().CreateAsync(entitiesNew);
            logger.LogTrace("{CreateAsync} effected count: {effectedCount}", nameof(CreateAsync),
                effectedCount.TryParseToString());

            // Kiểm tra xem có tạo thành công hay không
            if (effectedCount == 0)
            {
                logger.LogError("{CreateAsync} No entities were created", nameof(CreateAsync));
                throw new CreateFailedException("Failed to create entities");
            }

            // Map các entities đã được tạo thành ViewModels
            var result = Mapper.Map<IEnumerable<TViewModel>>(entitiesNew);
            var baseViewModels = result as TViewModel[] ?? result.ToArray();
            logger.LogTrace("{CreateAsync} result: {baseViewModels}", nameof(CreateAsync),
                baseViewModels.TryParseToString());
            await trans.CommitAsync();

            return baseViewModels;
        }
        catch
        {
            await trans.RollbackAsync();
            throw;
        }
    }
}