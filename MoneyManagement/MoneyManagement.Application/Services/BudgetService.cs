using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyManagement.Application.DTOs.Budget;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Domain.UnitOfWorks;

namespace MoneyManagement.Application.Services;

/// <summary>
///     Budget management service implementation (EN)<br />
///     Triển khai dịch vụ quản lý ngân sách (VI)
/// </summary>
public class BudgetService(IMapper mapper, IUnitOfWork unitOfWork, ILogger<BudgetService> logger)
    : IBudgetService
{
    public async Task<BudgetViewModel> CreateBudgetAsync(CreateBudgetRequest request, Guid userId)
    {
        try
        {
            logger.LogInformation("Creating new budget for user {UserId}", userId);

            var budget = mapper.Map<Budget>(request);
            budget.UserId = userId;
            budget.SetDefaultValue(userId.ToString());

            await unitOfWork.Repository<Budget, Guid>().CreateAsync(budget);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Budget created successfully with ID {BudgetId}", budget.Id);
            return mapper.Map<BudgetViewModel>(budget);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating budget for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BudgetViewModel> UpdateBudgetAsync(UpdateBudgetRequest request, Guid userId)
    {
        try
        {
            var existingBudget = await unitOfWork.Repository<Budget, Guid>().GetByIdAsync(request.Id);
            if (existingBudget == null || existingBudget.UserId != userId)
                throw new InvalidOperationException($"Budget with ID {request.Id} not found or access denied");

            var originalSpentAmount = existingBudget.SpentAmount;
            mapper.Map(request, existingBudget);
            existingBudget.SpentAmount = originalSpentAmount;
            existingBudget.SetValueUpdate(userId.ToString());

            await unitOfWork.Repository<Budget, Guid>().UpdateAsync(existingBudget);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<BudgetViewModel>(existingBudget);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating budget {BudgetId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteBudgetAsync(Guid budgetId, Guid userId)
    {
        try
        {
            var budget = await unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
            if (budget == null || budget.UserId != userId) return false;

            await unitOfWork.Repository<Budget, Guid>().DeleteSoftAsync(budget);
            await unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting budget {BudgetId}", budgetId);
            throw;
        }
    }

    public async Task<BudgetViewModel?> GetBudgetByIdAsync(Guid budgetId, Guid userId)
    {
        var budget = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.Id == budgetId && b.UserId == userId && string.IsNullOrEmpty(b.IsDeleted))
            .FirstOrDefaultAsync();

        return budget == null ? null : mapper.Map<BudgetViewModel>(budget);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByUserIdAsync(Guid userId)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && string.IsNullOrEmpty(b.IsDeleted))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetActiveBudgetsByUserIdAsync(Guid userId)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active && string.IsNullOrEmpty(b.IsDeleted))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByCategoryAsync(Guid userId, string category)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Category == category && string.IsNullOrEmpty(b.IsDeleted))
            .ToListAsync();

        return mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByPeriodAsync(Guid userId, BudgetPeriod period)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Period == period && string.IsNullOrEmpty(b.IsDeleted))
            .ToListAsync();

        return mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<BudgetViewModel> UpdateBudgetSpentAmountAsync(Guid budgetId, decimal amount, Guid userId)
    {
        var budget = await unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
        if (budget == null || budget.UserId != userId)
            throw new InvalidOperationException($"Budget with ID {budgetId} not found or access denied");

        budget.SpentAmount += amount;
        budget.SetValueUpdate(userId.ToString());

        await unitOfWork.Repository<Budget, Guid>().UpdateAsync(budget);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<BudgetViewModel>(budget);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsReachedAlertThresholdAsync(Guid userId)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active &&
                        b.AlertThreshold.HasValue && b.EnableNotifications && string.IsNullOrEmpty(b.IsDeleted))
            .ToListAsync();

        var alertBudgets = budgets.Where(b => b.IsAlertThresholdReached).ToList();
        return mapper.Map<List<BudgetViewModel>>(alertBudgets);
    }

    public async Task<List<BudgetViewModel>> GetOverBudgetBudgetsAsync(Guid userId)
    {
        var budgets = await unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active && string.IsNullOrEmpty(b.IsDeleted))
            .ToListAsync();

        var overBudgets = budgets.Where(b => b.IsOverBudget).ToList();
        return mapper.Map<List<BudgetViewModel>>(overBudgets);
    }

    public async Task<BudgetViewModel> ChangeBudgetStatusAsync(Guid budgetId, BudgetStatus status, Guid userId)
    {
        var budget = await unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
        if (budget == null || budget.UserId != userId)
            throw new InvalidOperationException($"Budget with ID {budgetId} not found or access denied");

        budget.Status = status;
        budget.SetValueUpdate(userId.ToString());

        await unitOfWork.Repository<Budget, Guid>().UpdateAsync(budget);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<BudgetViewModel>(budget);
    }
}