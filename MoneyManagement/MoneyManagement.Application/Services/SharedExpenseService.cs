using MoneyManagement.Application.DTOs.SharedExpense;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Domain.UnitOfWorks;

namespace MoneyManagement.Application.Services;

/// <summary>
///     Service for managing shared expenses (EN)<br />
///     Dịch vụ quản lý chi tiêu chung (VI)
/// </summary>
public class SharedExpenseService(IUnitOfWork unitOfWork) : ISharedExpenseService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    #region SharedExpense Operations

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseResponseDto>> GetAllSharedExpensesAsync()
    {
        var sharedExpenses = await _unitOfWork.Repository<SharedExpense, Guid>().GetAllAsync();
        return sharedExpenses.Select(MapToResponseDto);
    }

    /// <inheritdoc />
    public async Task<SharedExpenseResponseDto?> GetSharedExpenseByIdAsync(Guid id)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(id);
        return sharedExpense != null ? MapToResponseDto(sharedExpense) : null;
    }

    /// <inheritdoc />
    public async Task<SharedExpenseResponseDto> CreateSharedExpenseAsync(CreateSharedExpenseRequestDto createDto)
    {
        var sharedExpense = new SharedExpense
        {
            Title = createDto.Title,
            Description = createDto.Description,
            TotalAmount = createDto.TotalAmount,
            ExpenseDate = createDto.ExpenseDate,
            Category = createDto.Category,
            GroupName = createDto.GroupName,
            CurrencyCode = createDto.CurrencyCode ?? "VND",
            ReceiptImageUrl = createDto.ReceiptImageUrl,
            Notes = createDto.Notes,
            Status = SharedExpenseStatus.Pending
        };
        await _unitOfWork.Repository<SharedExpense, Guid>().CreateAsync(sharedExpense);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponseDto(sharedExpense);
    }

    /// <inheritdoc />
    public async Task<SharedExpenseResponseDto> UpdateSharedExpenseAsync(Guid id,
        UpdateSharedExpenseRequestDto updateDto)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(id) ?? throw new KeyNotFoundException($"SharedExpense with ID {id} not found.");

        // Update properties
        sharedExpense.Title = updateDto.Title;
        sharedExpense.Description = updateDto.Description;
        sharedExpense.TotalAmount = updateDto.TotalAmount;
        sharedExpense.ExpenseDate = updateDto.ExpenseDate;
        sharedExpense.Category = updateDto.Category;
        sharedExpense.GroupName = updateDto.GroupName;
        sharedExpense.CurrencyCode = updateDto.CurrencyCode ?? sharedExpense.CurrencyCode;
        sharedExpense.ReceiptImageUrl = updateDto.ReceiptImageUrl;
        sharedExpense.Notes = updateDto.Notes;

        await _unitOfWork.Repository<SharedExpense, Guid>().UpdateAsync(sharedExpense);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponseDto(sharedExpense);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSharedExpenseAsync(Guid id)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(id);
        if (sharedExpense == null) return false;

        await _unitOfWork.Repository<SharedExpense, Guid>().DeleteSoftAsync(sharedExpense);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByStatusAsync(int status)
    {
        var sharedExpenses = await _unitOfWork.Repository<SharedExpense, Guid>().GetAllAsync();
        var filteredExpenses = sharedExpenses.Where(se => (int)se.Status == status);
        return filteredExpenses.Select(MapToResponseDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByDateRangeAsync(DateTime startDate,
        DateTime endDate)
    {
        var sharedExpenses = await _unitOfWork.Repository<SharedExpense, Guid>().GetAllAsync();
        var filteredExpenses = sharedExpenses.Where(se => se.ExpenseDate >= startDate && se.ExpenseDate <= endDate);
        return filteredExpenses.Select(MapToResponseDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByGroupAsync(string groupName)
    {
        var sharedExpenses = await _unitOfWork.Repository<SharedExpense, Guid>().GetAllAsync();
        var filteredExpenses = sharedExpenses.Where(se =>
            !string.IsNullOrEmpty(se.GroupName) &&
            se.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        return filteredExpenses.Select(MapToResponseDto);
    }

    /// <inheritdoc />
    public async Task<SharedExpenseResponseDto> MarkAsSettledAsync(Guid id)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(id) ?? throw new KeyNotFoundException($"SharedExpense with ID {id} not found.");
        sharedExpense.Status = SharedExpenseStatus.Settled;
        sharedExpense.SettledAmount = sharedExpense.TotalAmount;

        await _unitOfWork.Repository<SharedExpense, Guid>().UpdateAsync(sharedExpense);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponseDto(sharedExpense);
    }

    #endregion

    #region SharedExpenseParticipant Operations

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseParticipantResponseDto>> GetParticipantsBySharedExpenseIdAsync(
        Guid sharedExpenseId)
    {
        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var filteredParticipants = participants.Where(p => p.SharedExpenseId == sharedExpenseId);
        return filteredParticipants.Select(MapToParticipantResponseDto);
    }

    /// <inheritdoc />
    public async Task<SharedExpenseParticipantResponseDto> AddParticipantAsync(
        CreateSharedExpenseParticipantRequestDto createDto)
    {
        // Validate that either UserId or ParticipantName is provided
        if (createDto.UserId == null && string.IsNullOrEmpty(createDto.ParticipantName))
            throw new ArgumentException("Either UserId or ParticipantName must be provided.");

        var participant = new SharedExpenseParticipant
        {
            SharedExpenseId = createDto.SharedExpenseId,
            UserId = createDto.UserId,
            ParticipantName = createDto.ParticipantName,
            Email = createDto.Email,
            PhoneNumber = createDto.PhoneNumber,
            ShareAmount = createDto.ShareAmount,
            Notes = createDto.Notes
        };
        await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().CreateAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        return MapToParticipantResponseDto(participant);
    }

    /// <inheritdoc />
    public async Task<SharedExpenseParticipantResponseDto> UpdateParticipantAsync(Guid id,
        UpdateSharedExpenseParticipantRequestDto updateDto)
    {
        SharedExpenseParticipant participant = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetByIdAsync(id) ?? throw new KeyNotFoundException($"SharedExpenseParticipant with ID {id} not found.");

        // Update properties
        participant.ParticipantName = updateDto.ParticipantName;
        participant.Email = updateDto.Email;
        participant.PhoneNumber = updateDto.PhoneNumber;
        if (updateDto.ShareAmount.HasValue) participant.ShareAmount = updateDto.ShareAmount.Value;
        participant.PaymentMethod = updateDto.PaymentMethod;
        participant.Notes = updateDto.Notes;

        await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().UpdateAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        return MapToParticipantResponseDto(participant);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveParticipantAsync(Guid id)
    {
        var participant = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetByIdAsync(id);
        if (participant == null) return false;

        await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().DeleteSoftAsync(participant);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<SharedExpenseParticipantResponseDto> RecordPaymentAsync(Guid participantId, decimal amount,
        string? paymentMethod = null)
    {
        var participant = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetByIdAsync(participantId) ?? throw new KeyNotFoundException($"SharedExpenseParticipant with ID {participantId} not found.");

        // Update payment information
        participant.PaidAmount = Math.Min(participant.PaidAmount + amount, participant.ShareAmount);
        if (!string.IsNullOrEmpty(paymentMethod)) participant.PaymentMethod = paymentMethod;

        // Update settled date if fully paid
        if (participant.PaidAmount >= participant.ShareAmount) participant.SettledDate = DateTime.UtcNow;

        await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().UpdateAsync(participant);

        // Update shared expense settled amount
        await UpdateSharedExpenseSettledAmountAsync(participant.SharedExpenseId);

        await _unitOfWork.SaveChangesAsync();

        return MapToParticipantResponseDto(participant);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SharedExpenseParticipantResponseDto>> GetUnsettledParticipantsAsync(
        Guid sharedExpenseId)
    {
        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var unsettledParticipants = participants.Where(p =>
            p.SharedExpenseId == sharedExpenseId &&
            !p.IsSettled);
        return unsettledParticipants.Select(MapToParticipantResponseDto);
    }

    #endregion

    #region Calculation and Analysis

    /// <inheritdoc />
    public async Task<EqualSplitCalculationDto> CalculateEqualSplitAsync(Guid sharedExpenseId)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(sharedExpenseId) ?? throw new KeyNotFoundException($"SharedExpense with ID {sharedExpenseId} not found.");
        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var expenseParticipants = participants.Where(p => p.SharedExpenseId == sharedExpenseId).ToList();

        if (expenseParticipants.Count == 0)
            throw new InvalidOperationException("No participants found for this shared expense.");

        var splitAmount = sharedExpense.TotalAmount / expenseParticipants.Count;
        return new EqualSplitCalculationDto
        {
            SharedExpenseId = sharedExpenseId,
            TotalAmount = sharedExpense.TotalAmount,
            OwedAmount = sharedExpense.TotalAmount,
            ParticipantCount = expenseParticipants.Count,
            Participants = [.. expenseParticipants.Select(p => new ParticipantSplitDto
            {
                ParticipantId = p.Id,
                ParticipantName = p.ParticipantName ?? "Unknown",
                RecommendedShareAmount = splitAmount,
                CurrentShareAmount = p.ShareAmount,
                Difference = splitAmount - p.ShareAmount
            })]
        };
    }

    /// <inheritdoc />
    public async Task<SharedExpenseSummaryDto> GetExpenseSummaryAsync(Guid sharedExpenseId)
    {
        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(sharedExpenseId) ?? throw new KeyNotFoundException($"SharedExpense with ID {sharedExpenseId} not found.");
        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var expenseParticipants = participants.Where(p => p.SharedExpenseId == sharedExpenseId).ToList();

        var totalPaidAmount = expenseParticipants.Sum(p => p.PaidAmount);
        var settledParticipantsCount = expenseParticipants.Count(p => p.IsSettled);

        return new SharedExpenseSummaryDto
        {
            SharedExpenseId = sharedExpenseId,
            Title = sharedExpense.Title,
            TotalAmount = sharedExpense.TotalAmount,
            TotalPaidAmount = totalPaidAmount,
            RemainingAmount = sharedExpense.TotalAmount - totalPaidAmount,
            ParticipantCount = expenseParticipants.Count,
            SettledParticipantsCount = settledParticipantsCount,
            UnsettledParticipantsCount = expenseParticipants.Count - settledParticipantsCount,
            SettlementPercentage = sharedExpense.SettlementPercentage,
            Status = sharedExpense.Status,
            IsFullySettled = sharedExpense.IsFullySettled
        };
    }

    /// <inheritdoc />
    public async Task<UserSharedExpenseStatsDto> GetUserStatisticsAsync()
    {
        var sharedExpenses = await _unitOfWork.Repository<SharedExpense, Guid>().GetAllAsync();
        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var sharedExpensesList = sharedExpenses.ToList();
        var participantsList = participants.ToList();

        var totalExpenses = sharedExpensesList.Count;
        var totalAmount = sharedExpensesList.Sum(se => se.TotalAmount);
        var settledExpenses = sharedExpensesList.Count(se => se.Status == SharedExpenseStatus.Settled);
        var pendingExpenses = sharedExpensesList.Count(se => se.Status == SharedExpenseStatus.Pending);
        var partiallySettledExpenses =
            sharedExpensesList.Count(se => se.Status == SharedExpenseStatus.PartiallySettled);

        var userParticipations = participantsList.Count;
        var userTotalOwed = participantsList.Sum(p => p.ShareAmount);
        var userTotalPaid = participantsList.Sum(p => p.PaidAmount);
        var userTotalOutstanding = userTotalOwed - userTotalPaid;

        return new UserSharedExpenseStatsDto
        {
            TotalExpenses = totalExpenses,
            TotalAmount = totalAmount,
            SettledExpenses = settledExpenses,
            PendingExpenses = pendingExpenses,
            PartiallySettledExpenses = partiallySettledExpenses,
            UserParticipations = userParticipations,
            UserTotalOwed = userTotalOwed,
            UserTotalPaid = userTotalPaid,
            UserTotalOutstanding = userTotalOutstanding,
            SettlementRate = userTotalOwed > 0 ? userTotalPaid / userTotalOwed * 100 : 0
        };
    }

    #endregion

    #region Private Helper Methods

    private static SharedExpenseResponseDto MapToResponseDto(SharedExpense sharedExpense)
    {
        return new SharedExpenseResponseDto
        {
            Id = sharedExpense.Id,
            Title = sharedExpense.Title,
            Description = sharedExpense.Description,
            TotalAmount = sharedExpense.TotalAmount,
            SettledAmount = sharedExpense.SettledAmount,
            RemainingAmount = sharedExpense.RemainingAmount,
            ExpenseDate = sharedExpense.ExpenseDate,
            Category = sharedExpense.Category,
            Status = sharedExpense.Status,
            GroupName = sharedExpense.GroupName,
            CurrencyCode = sharedExpense.CurrencyCode,
            ReceiptImageUrl = sharedExpense.ReceiptImageUrl,
            Notes = sharedExpense.Notes,
            IsFullySettled = sharedExpense.IsFullySettled, SettlementPercentage = sharedExpense.SettlementPercentage,
            CreatedAt = sharedExpense.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = sharedExpense.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private static SharedExpenseParticipantResponseDto MapToParticipantResponseDto(SharedExpenseParticipant participant)
    {
        return new SharedExpenseParticipantResponseDto
        {
            Id = participant.Id,
            SharedExpenseId = participant.SharedExpenseId,
            UserId = participant.UserId,
            ParticipantName = participant.ParticipantName,
            Email = participant.Email,
            PhoneNumber = participant.PhoneNumber,
            ShareAmount = participant.ShareAmount,
            PaidAmount = participant.PaidAmount,
            RemainingAmount = participant.ShareAmount - participant.PaidAmount,
            IsSettled = participant.IsSettled,
            SettledDate = participant.SettledDate,
            PaymentMethod = participant.PaymentMethod,
            Notes = participant.Notes, PaymentPercentage = participant.PaymentPercentage,
            CreatedAt = participant.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = participant.UpdatedAt
        };
    }

    private async Task UpdateSharedExpenseSettledAmountAsync(Guid? sharedExpenseId)
    {
        if (!sharedExpenseId.HasValue) return;

        var sharedExpense = await _unitOfWork.Repository<SharedExpense, Guid>().GetByIdAsync(sharedExpenseId.Value);
        if (sharedExpense == null) return;

        var participants = await _unitOfWork.Repository<SharedExpenseParticipant, Guid>().GetAllAsync();
        var expenseParticipants = participants.Where(p => p.SharedExpenseId == sharedExpenseId);

        var totalPaid = expenseParticipants.Sum(p => p.PaidAmount);
        sharedExpense.SettledAmount = totalPaid;

        // Update status based on settlement
        if (totalPaid >= sharedExpense.TotalAmount)
            sharedExpense.Status = SharedExpenseStatus.Settled;
        else if (totalPaid > 0)
            sharedExpense.Status = SharedExpenseStatus.PartiallySettled;
        else
            sharedExpense.Status = SharedExpenseStatus.Pending;

        await _unitOfWork.Repository<SharedExpense, Guid>().UpdateAsync(sharedExpense);
    }

    #endregion
}