using MoneyManagement.Application.DTOs.Jar;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Interfaces;

/// <summary>
///     Interface for jar management service implementing 6 Jars method (EN)<br />
///     Giao diện cho dịch vụ quản lý lọ tiền triển khai phương pháp 6 lọ (VI)
/// </summary>
public interface IJarService
{
    /// <summary>
    ///     Gets all jars asynchronously (EN)<br />
    ///     Lấy tất cả lọ một cách bất đồng bộ (VI)
    /// </summary>
    /// <returns>List of jar response DTOs</returns>
    Task<IEnumerable<JarResponseDto>> GetAllJarsAsync();

    /// <summary>
    ///     Gets a jar by identifier asynchronously (EN)<br />
    ///     Lấy lọ theo định danh một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="jarId">Jar identifier</param>
    /// <returns>Jar response DTO or null if not found</returns>
    Task<JarResponseDto?> GetJarByIdAsync(Guid jarId);

    /// <summary>
    ///     Gets jar by type asynchronously (EN)<br />
    ///     Lấy lọ theo loại một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="jarType">Jar type</param>
    /// <returns>Jar response DTO or null if not found</returns>
    Task<JarResponseDto?> GetJarByTypeAsync(JarType jarType);

    /// <summary>
    ///     Creates a new jar asynchronously (EN)<br />
    ///     Tạo lọ mới một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="request">Create jar request DTO</param>
    /// <returns>Created jar response DTO</returns>
    Task<JarResponseDto> CreateJarAsync(CreateJarRequestDto request);

    /// <summary>
    ///     Updates an existing jar asynchronously (EN)<br />
    ///     Cập nhật lọ hiện có một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="jarId">Jar identifier</param>
    /// <param name="request">Update jar request DTO</param>
    /// <returns>Updated jar response DTO</returns>
    Task<JarResponseDto> UpdateJarAsync(Guid jarId, UpdateJarRequestDto request);

    /// <summary>
    ///     Deletes a jar asynchronously (EN)<br />
    ///     Xóa lọ một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="jarId">Jar identifier</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteJarAsync(Guid jarId);

    /// <summary>
    ///     Initializes default 6 jars for a user (EN)<br />
    ///     Khởi tạo 6 lọ mặc định cho người dùng (VI)
    /// </summary>
    /// <returns>List of created jar response DTOs</returns>
    Task<IEnumerable<JarResponseDto>> InitializeDefaultJarsAsync();

    /// <summary>
    ///     Adds money to a jar (EN)<br />
    ///     Thêm tiền vào lọ (VI)
    /// </summary>
    /// <param name="jarId">Jar identifier</param>
    /// <param name="request">Add money request DTO</param>
    /// <returns>Updated jar response DTO</returns>
    Task<JarResponseDto> AddMoneyToJarAsync(Guid jarId, AddMoneyToJarRequestDto request);

    /// <summary>
    ///     Withdraws money from a jar (EN)<br />
    ///     Rút tiền từ lọ (VI)
    /// </summary>
    /// <param name="jarId">Jar identifier</param>
    /// <param name="request">Withdrawal request DTO</param>
    /// <returns>Updated jar response DTO</returns>
    Task<JarResponseDto> WithdrawFromJarAsync(Guid jarId, WithdrawFromJarRequestDto request);

    /// <summary>
    ///     Transfers money between jars (EN)<br />
    ///     Chuyển tiền giữa các lọ (VI)
    /// </summary>
    /// <param name="request">Transfer request DTO</param>
    /// <returns>Transfer result DTO</returns>
    Task<TransferResultDto> TransferBetweenJarsAsync(TransferBetweenJarsRequestDto request);

    /// <summary>
    ///     Distributes income across jars using 6 Jars method (EN)<br />
    ///     Phân phối thu nhập vào các lọ sử dụng phương pháp 6 lọ (VI)
    /// </summary>
    /// <param name="request">Distribute income request DTO</param>
    /// <returns>Distribution result DTO</returns>
    Task<DistributionResultDto> DistributeIncomeAsync(DistributeIncomeRequestDto request);

    /// <summary>
    ///     Gets jar allocation summary (EN)<br />
    ///     Lấy tóm tắt phân bổ lọ (VI)
    /// </summary>
    /// <returns>Jar allocation summary DTO</returns>
    Task<JarAllocationSummaryDto> GetJarAllocationSummaryAsync();
}