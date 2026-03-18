using AutoMapper;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Services
{
    public interface ITransactionService
    {
        Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<TransactionDto>>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, TransactionType? type = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<TransactionDto>>> GetByDuelIdAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<TransactionDto>>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> CreateDepositAsync(Guid userId, decimal amount, PaymentProvider paymentProvider, Guid? duelId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> CreateEntryFeeAsync(Guid userId, Guid duelId, decimal amount, CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> CreatePrizeAsync(Guid userId, Guid duelId, decimal amount, CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> CreateRefundAsync(Guid userId, Guid? duelId, decimal amount, string reason, CancellationToken cancellationToken = default);
        Task<ApiResponse<TransactionDto>> CreateWithdrawalAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CompleteTransactionAsync(Guid transactionId, string? externalTransactionId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> FailTransactionAsync(Guid transactionId, string errorMessage, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CancelTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
        Task<ApiResponse<decimal>> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessWebhookAsync(string provider, object webhookData, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateTransactionInfoAsync(
            Guid transactionId,
            string? externalTransactionId,
            string? paymentUrl,
            DateTime? expiresAt = null,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<List<TransactionDto>>> RefundDuelEntryFeesAsync(Guid duelId, string reason, CancellationToken cancellationToken = default);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDuelRepository _duelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IDuelRepository duelRepository,
            IMapper mapper,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _duelRepository = duelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
                if (transaction == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    };
                }

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction by ID: {TransactionId}", id);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByExternalIdAsync(externalTransactionId, cancellationToken);
                if (transaction == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    };
                }

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction by external ID: {ExternalId}", externalTransactionId);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<TransactionDto>>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, TransactionType? type = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var (transactions, totalCount) = await _transactionRepository.GetPagedByUserAsync(userId, pageNumber, pageSize, type, cancellationToken);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return new ApiResponse<PagedResult<TransactionDto>>
                {
                    Success = true,
                    Data = new PagedResult<TransactionDto>
                    {
                        Items = transactionDtos,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for user: {UserId}", userId);
                return new ApiResponse<PagedResult<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transactions",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<TransactionDto>>> GetByDuelIdAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _transactionRepository.GetByDuelIdAsync(duelId, cancellationToken);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return new ApiResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for duel: {DuelId}", duelId);
                return new ApiResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transactions",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<TransactionDto>>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _transactionRepository.GetPendingTransactionsAsync(cancellationToken);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return new ApiResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending transactions");
                return new ApiResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving pending transactions",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> CreateDepositAsync(Guid userId, decimal amount, PaymentProvider paymentProvider, Guid? duelId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                if (amount <= 0)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Amount must be greater than zero"
                    };
                }

                var transaction = new Transaction
                {
                    UserId = userId,
                    DuelId = duelId,
                    Type = TransactionType.Deposit,
                    Amount = amount,
                    Status = TransactionStatus.Pending,
                    PaymentProvider = paymentProvider,
                    Description = $"Deposit of €{amount:F2}"
                };

                transaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
                _logger.LogInformation("Created deposit transaction: {TransactionId} for user: {UserId}, amount: {Amount}",
                    transaction.Id, userId, amount);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction),
                    Message = "Deposit transaction created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deposit for user: {UserId}, amount: {Amount}", userId, amount);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while creating deposit",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> CreateEntryFeeAsync(Guid userId, Guid duelId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                var duel = await _duelRepository.GetByIdAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Duel not found"
                    };
                }

                if (user.Balance < amount)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Insufficient balance"
                    };
                }

                // Use repository method for transaction + balance update
                var transaction = await _transactionRepository.CreateEntryFeeTransactionAsync(userId, duelId, amount, duel.Title, cancellationToken);

                _logger.LogInformation("Created entry fee transaction: {TransactionId} for user: {UserId}, duel: {DuelId}, amount: {Amount}",
                    transaction.Id, userId, duelId, amount);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction),
                    Message = "Entry fee processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entry fee for user: {UserId}, duel: {DuelId}, amount: {Amount}", userId, duelId, amount);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while processing entry fee",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> CreatePrizeAsync(Guid userId, Guid duelId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                var duel = await _duelRepository.GetByIdAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Duel not found"
                    };
                }

             
                var transaction = await _transactionRepository.CreatePrizeTransactionAsync(userId, duelId, amount, duel.Title, cancellationToken);

                _logger.LogInformation("Created prize transaction: {TransactionId} for user: {UserId}, duel: {DuelId}, amount: {Amount}",
                    transaction.Id, userId, duelId, amount);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction),
                    Message = "Prize awarded successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prize for user: {UserId}, duel: {DuelId}, amount: {Amount}", userId, duelId, amount);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while awarding prize",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> CreateRefundAsync(Guid userId, Guid? duelId, decimal amount, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                // Use repository method for transaction + balance update
                var transaction = await _transactionRepository.CreateRefundTransactionAsync(userId, duelId, amount, reason, cancellationToken);

                _logger.LogInformation("Created refund transaction: {TransactionId} for user: {UserId}, amount: {Amount}, reason: {Reason}",
                    transaction.Id, userId, amount, reason);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction),
                    Message = "Refund processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund for user: {UserId}, amount: {Amount}", userId, amount);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while processing refund",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TransactionDto>> CreateWithdrawalAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                if (user.Balance < amount)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Insufficient balance"
                    };
                }

                if (amount < 10)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Minimum withdrawal amount is €10"
                    };
                }

                var transaction = new Transaction
                {
                    UserId = userId,
                    Type = TransactionType.Withdrawal,
                    Amount = amount,
                    Status = TransactionStatus.Pending,
                    Description = $"Withdrawal of €{amount:F2}"
                };

                transaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
                _logger.LogInformation("Created withdrawal transaction: {TransactionId} for user: {UserId}, amount: {Amount}",
                    transaction.Id, userId, amount);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = _mapper.Map<TransactionDto>(transaction),
                    Message = "Withdrawal request created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal for user: {UserId}, amount: {Amount}", userId, amount);
                return new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = "An error occurred while creating withdrawal",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetByIdAsync(request.DuelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Duel not found"
                    };
                }

                if (request.Amount != duel.EntryFee)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Invalid payment amount"
                    };
                }

                _logger.LogInformation("Processing payment for duel: {DuelId}, amount: {Amount}, provider: {Provider}",
                    request.DuelId, request.Amount, request.PaymentProvider);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Payment processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for duel: {DuelId}", request.DuelId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while processing payment",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CompleteTransactionAsync(Guid transactionId, string? externalTransactionId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
                if (transaction == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Transaction not found"
                    };
                }

                if (transaction.Status != TransactionStatus.Pending)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Transaction is not in pending status"
                    };
                }

                // Use repository method for completing transaction with balance update
                await _transactionRepository.CompleteTransactionAsync(transactionId, externalTransactionId, cancellationToken);

                _logger.LogInformation("Completed transaction: {TransactionId}", transactionId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Transaction completed successfully"
                };
            }
            catch (Exception ex)
            {
                throw;
                _logger.LogError(ex, "Error completing transaction: {TransactionId}", transactionId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while completing transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> FailTransactionAsync(Guid transactionId, string errorMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                await _transactionRepository.UpdateStatusAsync(transactionId, TransactionStatus.Failed, errorMessage, cancellationToken);
                _logger.LogWarning("Failed transaction: {TransactionId}, error: {Error}", transactionId, errorMessage);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Transaction marked as failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error failing transaction: {TransactionId}", transactionId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while failing transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CancelTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _transactionRepository.UpdateStatusAsync(transactionId, TransactionStatus.Cancelled, cancellationToken: cancellationToken);
                _logger.LogInformation("Cancelled transaction: {TransactionId}", transactionId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Transaction cancelled successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transaction: {TransactionId}", transactionId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while cancelling transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateTransactionInfoAsync(
            Guid transactionId,
            string? externalTransactionId,
            string? paymentUrl,
            DateTime? expiresAt = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
                if (transaction == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Transaction not found"
                    };
                }

                if (!string.IsNullOrEmpty(externalTransactionId))
                    transaction.ExternalTransactionId = externalTransactionId;

                if (!string.IsNullOrEmpty(paymentUrl))
                    transaction.PaymentUrl = paymentUrl;

                if (expiresAt.HasValue)
                    transaction.ExpiresAt = expiresAt.Value;

                await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Transaction updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction info: {TransactionId}", transactionId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while updating transaction",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<decimal>> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var balance = await _transactionRepository.GetUserBalanceAsync(userId, cancellationToken);
                return new ApiResponse<decimal>
                {
                    Success = true,
                    Data = balance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user balance: {UserId}", userId);
                return new ApiResponse<decimal>
                {
                    Success = false,
                    Message = "An error occurred while retrieving balance",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessWebhookAsync(string provider, object webhookData, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing webhook from provider: {Provider}", provider);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Webhook processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook from provider: {Provider}", provider);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while processing webhook",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<TransactionDto>>> RefundDuelEntryFeesAsync(Guid duelId, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Refunding entry fees for duel {DuelId}, reason: {Reason}", duelId, reason);

                var refunds = new List<Transaction>();

               
                var entryFees = await _transactionRepository.GetByDuelIdAsync(duelId, cancellationToken);
                var entryFeeTransactions = entryFees.Where(t => t.Type == TransactionType.EntryFee).ToList();

                foreach (var entryFee in entryFeeTransactions)
                {
                
                    var refund = new Transaction
                    {
                        UserId = entryFee.UserId,
                        DuelId = duelId,
                        Type = TransactionType.Refund,
                        Amount = entryFee.Amount,
                        Status = TransactionStatus.Completed,
                        Description = reason,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _transactionRepository.AddAsync(refund, cancellationToken);

                 
                    var user = await _userRepository.GetByIdAsync(entryFee.UserId, cancellationToken);
                    if (user != null)
                    {
                        user.Balance += entryFee.Amount;
                        await _userRepository.UpdateAsync(user, cancellationToken);
                    }

                    refunds.Add(refund);
                }

                _logger.LogInformation("Refunded {Count} entry fees for duel {DuelId}", refunds.Count, duelId);

                return new ApiResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = _mapper.Map<List<TransactionDto>>(refunds)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding entry fees for duel {DuelId}", duelId);
                return new ApiResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while refunding entry fees",
                    Errors = { ex.Message }
                };
            }
        }
    }
}
