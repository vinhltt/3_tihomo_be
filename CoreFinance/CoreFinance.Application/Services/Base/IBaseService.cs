using Shared.EntityFramework.DTOs;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Services.Base;

// ReSharper disable once UnusedTypeParameter
public interface IBaseService<TEntity, TCreateRequest, in TUpdateRequest, TViewModel,
    in TKey>
    where TEntity : UserOwnedEntity<TKey>?, new()
    where TCreateRequest : BaseCreateRequest, new()
    where TUpdateRequest : BaseUpdateRequest<TKey>, new()
    where TViewModel : BaseViewModel<TKey>, new()
{
    /// <summary>
    ///     Gets all entities as view models asynchronously.<br />
    ///     (EN) Gets all entities as view models asynchronously.<br />
    ///     (VI) Lấy tất cả các entity dưới dạng view model một cách bất đồng bộ.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of view models.</returns>
    Task<IEnumerable<TViewModel>?> GetAllDtoAsync();

    /// <summary>
    ///     Gets an entity by its identifier as a view model asynchronously.<br />
    ///     (EN) Gets an entity by its identifier as a view model asynchronously.<br />
    ///     (VI) Lấy một entity theo định danh của nó dưới dạng view model một cách bất đồng bộ.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the view model, or null if the
    ///     entity is not found.
    /// </returns>
    Task<TViewModel?> GetByIdAsync(TKey id);

    /// <summary>
    ///     (EN) Deletes an entity permanently by its identifier asynchronously.<br />
    ///     (VI) Xóa vĩnh viễn một entity theo định danh của nó một cách bất đồng bộ.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    Task<int?> DeleteHardAsync(TKey id);

    /// <summary>
    ///     (EN) Soft deletes an entity by its identifier asynchronously.<br />
    ///     (VI) Xóa mềm một entity theo định danh của nó một cách bất đồng bộ.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    Task<int?> DeleteSoftAsync(TKey id);

    /// <summary>
    ///     (EN) Updates an entity asynchronously.<br />
    ///     (VI) Cập nhật một entity một cách bất đồng bộ.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="request">The update request containing the new data.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the updated view model, or null if
    ///     the update failed.
    /// </returns>
    Task<TViewModel?> UpdateAsync(TKey id, TUpdateRequest request);

    /// <summary>
    ///     (EN) Creates a new entity asynchronously.<br />
    ///     (VI) Tạo một entity mới một cách bất đồng bộ.
    /// </summary>
    /// <param name="request">The create request containing the data for the new entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the created view model, or null if
    ///     the creation failed.
    /// </returns>
    Task<TViewModel?> CreateAsync(TCreateRequest request);

    /// <summary>
    ///     (EN) Creates multiple new entities asynchronously.<br />
    ///     (VI) Tạo nhiều entity mới một cách bất đồng bộ.
    /// </summary>
    /// <param name="request">The list of create requests containing the data for the new entities.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the collection of created view
    ///     models, or an empty collection if the creation failed.
    /// </returns>
    Task<IEnumerable<TViewModel>?> CreateAsync(List<TCreateRequest> request);
}