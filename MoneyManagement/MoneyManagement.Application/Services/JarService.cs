using AutoMapper;
using Microsoft.Extensions.Logging;
using MoneyManagement.Application.DTOs.Jar;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Domain.UnitOfWorks;

namespace MoneyManagement.Application.Services;

public class JarService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<JarService> logger)
    : IJarService
{
    // Default 6 Jars allocation percentages
    private readonly Dictionary<JarType, decimal> _defaultAllocationPercentages = new()
    {
        { JarType.Necessities, 55m }, // 55%
        { JarType.LongTermSavings, 10m }, // 10%
        { JarType.Education, 10m }, // 10%
        { JarType.Play, 10m }, // 10%
        { JarType.FinancialFreedom, 10m }, // 10%
        { JarType.Give, 5m } // 5%
    };

    public async Task<IEnumerable<JarViewModel>> GetUserJarsAsync(Guid userId)
    {
        try
        {
            // User filtering is automatically applied in BaseRepository
            var jars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            return mapper.Map<IEnumerable<JarViewModel>>(jars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving jars for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to retrieve jars for user {userId}", ex);
        }
    }

    public async Task<JarViewModel?> GetJarByIdAsync(Guid jarId, Guid userId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId);
            if (jar == null)
                return null;

            return mapper.Map<JarViewModel>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving jar {JarId} for user {UserId}", jarId, userId);
            throw new InvalidOperationException($"Failed to retrieve jar {jarId} for user {userId}", ex);
        }
    }

    public async Task<JarViewModel> CreateJarAsync(CreateJarRequest request, Guid userId)
    {
        try
        {
            // Check if jar with same type already exists for user
            var existingJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var existingJarsList = existingJars.ToList();

            if (existingJarsList.Any(j => j.JarType == request.JarType))
                throw new InvalidOperationException($"A jar of type {request.JarType} already exists for this user.");
            var jar = new Jar
            {
                Id = Guid.CreateVersion7(),
                Name = request.Name,
                JarType = request.JarType,
                TargetAmount = request.TargetAmount, AllocationPercentage = request.AllocationPercentage ??
                                                                            _defaultAllocationPercentages
                                                                                .GetValueOrDefault(request.JarType,
                                                                                    10m),
                Balance = 0,
                IsActive = true
            };

            await unitOfWork.Repository<Jar, Guid>().CreateAsync(jar);

            logger.LogInformation("Created jar {JarId} of type {JarType} for user {UserId}",
                jar.Id, jar.JarType, userId);

            return mapper.Map<JarViewModel>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating jar for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to create jar for user {userId}", ex);
        }
    }

    public async Task<JarViewModel> UpdateJarAsync(UpdateJarRequest request, Guid userId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.Id) ?? throw new InvalidOperationException("Jar not found or access denied.");
            jar.Name = request.Name;
            jar.TargetAmount = request.TargetAmount;
            jar.AllocationPercentage = request.AllocationPercentage;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Updated jar {JarId} for user {UserId}", request.Id, userId);

            return mapper.Map<JarViewModel>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating jar {JarId} for user {UserId}", request.Id, userId);
            throw new InvalidOperationException($"Failed to update jar {request.Id} for user {userId}", ex);
        }
    }

    public async Task<bool> DeleteJarAsync(Guid jarId, Guid userId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId);
            if (jar == null) return false;

            if (jar.Balance > 0)
                throw new InvalidOperationException("Cannot delete jar with balance. Please transfer funds first.");

            await unitOfWork.Repository<Jar, Guid>().DeleteSoftAsync(jarId);

            logger.LogInformation("Deleted jar {JarId} for user {UserId}", jarId, userId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting jar {JarId} for user {UserId}", jarId, userId);
            throw new InvalidOperationException($"Failed to delete jar {jarId} for user {userId}", ex);
        }
    }

    public async Task<IncomeAllocationResult> AllocateIncomeAsync(AllocateIncomeRequest request, Guid userId)
    {
        try
        {
            var userJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var activeJars = userJars.Where(j => j.IsActive).ToList();
            if (activeJars.Count == 0) throw new InvalidOperationException("No active jars found for allocation.");

            var result = new IncomeAllocationResult
            {
                TotalAllocated = request.IncomeAmount,
                AllocationTime = DateTime.UtcNow,
                JarAllocations = []
            };

            decimal totalAllocated = 0;
            var jarsToUpdate = new List<Jar>();
            foreach (var jar in activeJars)
            {
                var allocationPercentage = request.CustomRatios?.GetValueOrDefault(jar.JarType.ToString())
                                           ?? jar.AllocationPercentage ?? 0;

                var allocationAmount = request.IncomeAmount * (allocationPercentage / 100);

                var previousBalance = jar.Balance;
                jar.Balance += allocationAmount;
                jar.UpdatedAt = DateTime.UtcNow;

                totalAllocated += allocationAmount;
                jarsToUpdate.Add(jar);

                result.JarAllocations.Add(new JarAllocationDetail
                {
                    Jar = mapper.Map<JarViewModel>(jar),
                    AllocationPercentage = allocationPercentage,
                    AllocatedAmount = allocationAmount,
                    BalanceBefore = previousBalance,
                    BalanceAfter = jar.Balance
                });
            }

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jarsToUpdate);
            result.TotalAllocated = totalAllocated;
            result.IsSuccess = true;
            result.Message = $"Successfully allocated {totalAllocated:C} across {activeJars.Count} jars.";

            logger.LogInformation("Allocated income {Amount} for user {UserId} across {JarCount} jars",
                request.IncomeAmount, userId, activeJars.Count);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error allocating income for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to allocate income for user {userId}", ex);
        }
    }

    public async Task<JarTransferResult> TransferBetweenJarsAsync(JarTransferRequest request, Guid userId)
    {
        try
        {
            var fromJar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.FromJarId);
            var toJar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.ToJarId);

            if (fromJar == null)
                throw new InvalidOperationException("Source jar not found or access denied.");

            if (toJar == null)
                throw new InvalidOperationException("Destination jar not found or access denied.");
            if (fromJar.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance in source jar.");

            var result = new JarTransferResult
            {
                IsSuccess = true,
                Message = $"Transfer of {request.Amount:C} completed successfully.",
                TransferAmount = request.Amount,
                TransferTime = DateTime.UtcNow,
                FromJar = mapper.Map<JarViewModel>(fromJar),
                ToJar = mapper.Map<JarViewModel>(toJar)
            };

            // Perform transfer
            fromJar.Balance -= request.Amount;
            toJar.Balance += request.Amount;

            fromJar.UpdatedAt = DateTime.UtcNow;
            toJar.UpdatedAt = DateTime.UtcNow;

            // Update the result with new jar states
            result.FromJar = mapper.Map<JarViewModel>(fromJar);
            result.ToJar = mapper.Map<JarViewModel>(toJar);

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync([fromJar, toJar]);

            logger.LogInformation("Transferred {Amount} from jar {FromJarId} to jar {ToJarId} for user {UserId}",
                request.Amount, request.FromJarId, request.ToJarId, userId);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error transferring between jars for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to transfer between jars for user {userId}", ex);
        }
    }

    public async Task<JarViewModel> WithdrawFromJarAsync(JarWithdrawRequest request, Guid userId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.JarId) ?? throw new InvalidOperationException("Jar not found or access denied.");
            if (jar.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance in jar.");

            jar.Balance -= request.Amount;
            jar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Withdrew {Amount} from jar {JarId} for user {UserId}",
                request.Amount, request.JarId, userId);

            return mapper.Map<JarViewModel>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error withdrawing from jar {JarId} for user {UserId}", request.JarId, userId);
            throw new InvalidOperationException($"Failed to withdraw from jar {request.JarId} for user {userId}", ex);
        }
    }

    public async Task<JarBalanceSummary> GetJarBalanceSummaryAsync(Guid userId)
    {
        try
        {
            var jars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var jarsList = jars.ToList();
            var summary = new JarBalanceSummary
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow,
                TotalBalance = jarsList.Sum(j => j.Balance),
                TotalTarget = jarsList.Where(j => j.TargetAmount.HasValue).Sum(j => j.TargetAmount!.Value),
                JarBalances = [.. jarsList.Select(jar => new JarBalanceDetail
                {
                    JarId = jar.Id,
                    Name = jar.Name,
                    JarType = jar.JarType.ToString(),
                    CurrentBalance = jar.Balance, TargetAmount = jar.TargetAmount ?? 0,
                    AllocationPercentage = jar.AllocationPercentage ?? 0,
                    Progress = jar.TargetAmount.HasValue && jar.TargetAmount > 0
                        ? Math.Min(jar.Balance / jar.TargetAmount.Value * 100, 100)
                        : 0,
                    Status = GetJarStatus(jar),
                    LastUpdated = jar.UpdatedAt ?? DateTime.UtcNow,
                    MonthlyContribution = 0 // This would be calculated based on historical data
                })]
            };

            // Calculate statistics
            summary.Statistics = CalculateStatistics(summary.JarBalances);
            summary.OverallProgress = summary.TotalTarget > 0
                ? summary.TotalBalance / summary.TotalTarget * 100
                : 0;

            return summary;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating jar balance summary for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to generate jar balance summary for user {userId}", ex);
        }
    }

    public async Task<IEnumerable<JarViewModel>> InitializeDefaultJarsAsync(Guid userId)
    {
        try
        {
            var existingJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            if (existingJars.Count != 0)
            {
                logger.LogWarning("User {UserId} already has jars, returning existing jars", userId);
                return mapper.Map<IEnumerable<JarViewModel>>(existingJars);
            }

            var defaultJars = new List<Jar>();

            foreach (var jarType in Enum.GetValues<JarType>())
            {
                var jar = new Jar
                {
                    Id = Guid.CreateVersion7(),
                    Name = GetDefaultJarName(jarType),
                    JarType = jarType,
                    AllocationPercentage = _defaultAllocationPercentages[jarType],
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                defaultJars.Add(jar);
            }

            await unitOfWork.Repository<Jar, Guid>().CreateAsync(defaultJars);

            logger.LogInformation("Initialized {Count} default jars for user {UserId}", defaultJars.Count, userId);

            return mapper.Map<IEnumerable<JarViewModel>>(defaultJars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing default jars for user {UserId}", userId);
            throw new InvalidOperationException($"Failed to initialize default jars for user {userId}", ex);
        }
    }

    public async Task<JarViewModel> ToggleJarStatusAsync(Guid jarId, bool isActive, Guid userId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId) ?? throw new InvalidOperationException("Jar not found or access denied.");
            jar.IsActive = isActive;
            jar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Set jar {JarId} status to {Status} for user {UserId}",
                jarId, isActive ? "Active" : "Inactive", userId);

            return mapper.Map<JarViewModel>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting jar status for jar {JarId} and user {UserId}", jarId, userId);
            throw new InvalidOperationException($"Failed to set jar {jarId} status for user {userId}", ex);
        }
    }

    public async Task<IEnumerable<JarViewModel>> GetJarsByTypeAsync(Guid userId, JarType jarType)
    {
        try
        {
            var allJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var filteredJars = allJars.Where(j => j.JarType == jarType);
            return mapper.Map<IEnumerable<JarViewModel>>(filteredJars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving jars of type {JarType} for user {UserId}", jarType, userId);
            throw new InvalidOperationException($"Failed to retrieve jars of type {jarType} for user {userId}", ex);
        }
    }

    private static string GetJarStatus(Jar jar)
    {
        if (!jar.IsActive)
            return "Inactive";

        if (!jar.TargetAmount.HasValue)
            return "No Target";

        var progress = jar.Balance / jar.TargetAmount.Value * 100;

        return progress switch
        {
            >= 100 => "Target Reached",
            >= 80 => "Near Target",
            >= 50 => "On Track",
            >= 25 => "Building",
            _ => "Getting Started"
        };
    }

    private static JarBalanceStatistics CalculateStatistics(List<JarBalanceDetail> jarBalances)
    {
        var activeJars = jarBalances.Where(j => j.Status != "Inactive").ToList();

        return new JarBalanceStatistics
        {
            TotalJars = jarBalances.Count,
            ActiveJars = activeJars.Count,
            JarsAtTarget = activeJars.Count(j => j.Status == "Target Reached"),
            JarsNearTarget = activeJars.Count(j => j.Status == "Near Target"),
            AverageProgress = activeJars.Count != 0 ? activeJars.Average(j => j.Progress) : 0,
            TopPerformingJar = activeJars.OrderByDescending(j => j.Progress).FirstOrDefault()?.Name ?? "None",
            LeastProgressJar = activeJars.OrderBy(j => j.Progress).FirstOrDefault()?.Name ?? "None",
            TotalMonthlyAllocation = activeJars.Sum(j => j.MonthlyContribution)
        };
    }

    private static string GetDefaultJarName(JarType jarType)
    {
        return jarType switch
        {
            JarType.Necessities => "Necessities (55%)",
            JarType.LongTermSavings => "Long Term Savings (10%)",
            JarType.Education => "Education (10%)",
            JarType.Play => "Play (10%)",
            JarType.FinancialFreedom => "Financial Freedom (10%)",
            JarType.Give => "Give (5%)",
            _ => jarType.ToString()
        };
    }

    #region Interface Implementation Methods

    // Implementation of IJarService interface methods that return DTOs instead of ViewModels

    public async Task<IEnumerable<JarResponseDto>> GetAllJarsAsync()
    {
        try
        {
            var jars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            return mapper.Map<IEnumerable<JarResponseDto>>(jars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all jars");
            throw new InvalidOperationException("Failed to retrieve jars", ex);
        }
    }

    public async Task<JarResponseDto?> GetJarByIdAsync(Guid jarId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId);
            return jar == null ? null : mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving jar {JarId}", jarId);
            throw new InvalidOperationException($"Failed to retrieve jar {jarId}", ex);
        }
    }

    public async Task<JarResponseDto?> GetJarByTypeAsync(JarType jarType)
    {
        try
        {
            var jars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var jar = jars.FirstOrDefault(j => j.JarType == jarType);
            return jar == null ? null : mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving jar by type {JarType}", jarType);
            throw new InvalidOperationException($"Failed to retrieve jar of type {jarType}", ex);
        }
    }

    public async Task<JarResponseDto> CreateJarAsync(CreateJarRequestDto request)
    {
        try
        {
            // Check if jar with same type already exists
            var existingJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var existingJarsList = existingJars.ToList();

            if (existingJarsList.Any(j => j.JarType == request.JarType))
                throw new InvalidOperationException($"A jar of type {request.JarType} already exists.");

            var jar = new Jar
            {
                Id = Guid.CreateVersion7(),
                Name = request.Name,
                JarType = request.JarType,
                TargetAmount = request.TargetAmount,
                AllocationPercentage = request.AllocationPercentage ??
                                       _defaultAllocationPercentages.GetValueOrDefault(request.JarType, 10m),
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.Repository<Jar, Guid>().CreateAsync(jar);

            logger.LogInformation("Created jar {JarId} of type {JarType}", jar.Id, jar.JarType);

            return mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating jar");
            throw new InvalidOperationException("Failed to create jar", ex);
        }
    }

    public async Task<JarResponseDto> UpdateJarAsync(Guid jarId, UpdateJarRequestDto request)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId) ?? throw new InvalidOperationException("Jar not found.");
            jar.Name = request.Name;
            jar.TargetAmount = request.TargetAmount;
            jar.AllocationPercentage = request.AllocationPercentage;
            jar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Updated jar {JarId}", jarId);

            return mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating jar {JarId}", jarId);
            throw new InvalidOperationException($"Failed to update jar {jarId}", ex);
        }
    }

    public async Task<bool> DeleteJarAsync(Guid jarId)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId);
            if (jar == null) return false;

            if (jar.Balance > 0)
                throw new InvalidOperationException("Cannot delete jar with balance. Please transfer funds first.");

            await unitOfWork.Repository<Jar, Guid>().DeleteSoftAsync(jarId);

            logger.LogInformation("Deleted jar {JarId}", jarId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting jar {JarId}", jarId);
            throw new InvalidOperationException($"Failed to delete jar {jarId}", ex);
        }
    }

    public async Task<IEnumerable<JarResponseDto>> InitializeDefaultJarsAsync()
    {
        try
        {
            var existingJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            if (existingJars.Count != 0)
            {
                logger.LogWarning("Jars already exist, returning existing jars");
                return mapper.Map<IEnumerable<JarResponseDto>>(existingJars);
            }

            var defaultJars = new List<Jar>();

            foreach (var jarType in Enum.GetValues<JarType>())
            {
                var jar = new Jar
                {
                    Id = Guid.CreateVersion7(),
                    Name = GetDefaultJarName(jarType),
                    JarType = jarType,
                    AllocationPercentage = _defaultAllocationPercentages[jarType],
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                defaultJars.Add(jar);
            }

            await unitOfWork.Repository<Jar, Guid>().CreateAsync(defaultJars);

            logger.LogInformation("Initialized {Count} default jars", defaultJars.Count);

            return mapper.Map<IEnumerable<JarResponseDto>>(defaultJars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing default jars");
            throw new InvalidOperationException("Failed to initialize default jars", ex);
        }
    }

    public async Task<JarResponseDto> AddMoneyToJarAsync(Guid jarId, AddMoneyToJarRequestDto request)
    {
        try
        {
            Jar jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId) ?? throw new InvalidOperationException("Jar not found.");
            jar.Balance += request.Amount;
            jar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Added {Amount} to jar {JarId}", request.Amount, jarId);

            return mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding money to jar {JarId}", jarId);
            throw new InvalidOperationException($"Failed to add money to jar {jarId}", ex);
        }
    }

    public async Task<JarResponseDto> WithdrawFromJarAsync(Guid jarId, WithdrawFromJarRequestDto request)
    {
        try
        {
            var jar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(jarId) ?? throw new InvalidOperationException("Jar not found.");
            if (jar.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance in jar.");

            jar.Balance -= request.Amount;
            jar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jar);

            logger.LogInformation("Withdrew {Amount} from jar {JarId}", request.Amount, jarId);

            return mapper.Map<JarResponseDto>(jar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error withdrawing from jar {JarId}", jarId);
            throw new InvalidOperationException($"Failed to withdraw from jar {jarId}", ex);
        }
    }

    public async Task<TransferResultDto> TransferBetweenJarsAsync(TransferBetweenJarsRequestDto request)
    {
        try
        {
            var fromJar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.FromJarId);
            var toJar = await unitOfWork.Repository<Jar, Guid>().GetByIdAsync(request.ToJarId);

            if (fromJar == null)
                throw new InvalidOperationException("Source jar not found.");

            if (toJar == null)
                throw new InvalidOperationException("Destination jar not found.");

            if (fromJar.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance in source jar.");

            // Perform transfer
            fromJar.Balance -= request.Amount;
            toJar.Balance += request.Amount;

            fromJar.UpdatedAt = DateTime.UtcNow;
            toJar.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync([fromJar, toJar]);

            logger.LogInformation("Transferred {Amount} from jar {FromJarId} to jar {ToJarId}",
                request.Amount, request.FromJarId, request.ToJarId);
            return new TransferResultDto
            {
                IsSuccessful = true,
                TransactionId = Guid.CreateVersion7(),
                AmountTransferred = request.Amount,
                TransferredAt = DateTime.UtcNow,
                FromJar = mapper.Map<JarResponseDto>(fromJar),
                ToJar = mapper.Map<JarResponseDto>(toJar),
                Description = request.Description
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error transferring between jars");
            throw new InvalidOperationException("Failed to transfer between jars", ex);
        }
    }

    public async Task<DistributionResultDto> DistributeIncomeAsync(DistributeIncomeRequestDto request)
    {
        try
        {
            var userJars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var activeJars = userJars.Where(j => j.IsActive).ToList();

            if (activeJars.Count == 0) throw new InvalidOperationException("No active jars found for allocation.");
            var jarsToUpdate = new List<Jar>();
            var jarDistributions = new List<JarDistributionItemDto>();
            decimal totalAllocated = 0;
            foreach (var jar in activeJars)
            {
                decimal allocationPercentage = 0;
                if (request.CustomRatios != null && request.CustomRatios.TryGetValue(jar.JarType, out decimal value))
                    allocationPercentage = value;
                else
                    allocationPercentage = jar.AllocationPercentage ?? 0;

                var allocationAmount = request.IncomeAmount * (allocationPercentage / 100);

                var previousBalance = jar.Balance;
                jar.Balance += allocationAmount;
                jar.UpdatedAt = DateTime.UtcNow;

                totalAllocated += allocationAmount;
                jarsToUpdate.Add(jar);

                jarDistributions.Add(new JarDistributionItemDto
                {
                    Jar = mapper.Map<JarResponseDto>(jar),
                    AllocatedAmount = allocationAmount,
                    AllocationPercentage = allocationPercentage,
                    PreviousBalance = previousBalance,
                    NewBalance = jar.Balance
                });
            }

            await unitOfWork.Repository<Jar, Guid>().UpdateAsync(jarsToUpdate);

            logger.LogInformation("Distributed income {Amount} across {JarCount} jars",
                request.IncomeAmount, activeJars.Count);
            return new DistributionResultDto
            {
                IsSuccessful = true,
                TotalIncomeDistributed = totalAllocated,
                DistributedAt = DateTime.UtcNow,
                JarDistributions = jarDistributions,
                RemainingAmount = request.IncomeAmount - totalAllocated,
                Description = request.Description
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error distributing income");
            throw new InvalidOperationException("Failed to distribute income", ex);
        }
    }

    public async Task<JarAllocationSummaryDto> GetJarAllocationSummaryAsync()
    {
        try
        {
            var jars = await unitOfWork.Repository<Jar, Guid>().GetAllAsync();
            var jarsList = jars.ToList();
            return new JarAllocationSummaryDto
            {
                UserId = Guid.Empty, // This would need to be passed from the calling method
                LastUpdatedAt = DateTime.UtcNow,
                TotalBalance = jarsList.Sum(j => j.Balance),
                TotalTargetAmount = jarsList.Where(j => j.TargetAmount.HasValue).Sum(j => j.TargetAmount!.Value),
                TotalAllocationPercentage = jarsList.Sum(j => j.AllocationPercentage ?? 0),
                AvailableAllocationPercentage = 100 - jarsList.Sum(j => j.AllocationPercentage ?? 0),
                OverallProgressPercentage = jarsList.Where(j => j.TargetAmount.HasValue && j.TargetAmount > 0)
                    .Average(j => Math.Min(j.Balance / j.TargetAmount!.Value * 100, 100)),
                JarAllocations = [.. jarsList.Select(j => new JarAllocationItemDto
                {
                    JarId = j.Id,
                    JarName = j.Name,
                    JarType = j.JarType,
                    CurrentBalance = j.Balance,
                    TargetAmount = j.TargetAmount,
                    AllocationPercentage = j.AllocationPercentage,
                    ProgressPercentage = j.TargetAmount.HasValue && j.TargetAmount > 0
                        ? Math.Min(j.Balance / j.TargetAmount.Value * 100, 100)
                        : 0,
                    IsActive = j.IsActive
                })],
                DefaultAllocations = _defaultAllocationPercentages
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting jar allocation summary");
            throw new InvalidOperationException("Failed to get jar allocation summary", ex);
        }
    }

    #endregion
}