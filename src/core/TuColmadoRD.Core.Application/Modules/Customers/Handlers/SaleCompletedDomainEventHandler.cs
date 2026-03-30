using MediatR;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.Enums.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class SaleCompletedDomainEventHandler : INotificationHandler<SaleCompletedDomainEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDebtTransactionRepository _debtTransactionRepository;

    public SaleCompletedDomainEventHandler(
        ICustomerRepository customerRepository,
        IDebtTransactionRepository debtTransactionRepository)
    {
        _customerRepository = customerRepository;
        _debtTransactionRepository = debtTransactionRepository;
    }

    public async Task Handle(SaleCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var creditByCustomer = notification.Payments
            .Where(p => p.PaymentMethodId == PaymentMethod.Credit.Id && p.CustomerId.HasValue)
            .GroupBy(p => p.CustomerId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        foreach (var (customerId, amountValue) in creditByCustomer)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, [c => c.Account], cancellationToken);
            if (customer is null || !customer.IsActive)
            {
                throw new InvalidOperationException($"No se puede registrar fiado: cliente invalido ({customerId}).");
            }

            var amountResult = Money.FromDecimal(amountValue);
            if (!amountResult.TryGetResult(out var amount) || amount is null)
            {
                throw new InvalidOperationException($"Monto de credito invalido para cliente {customerId}.");
            }

            var debtResult = customer.Account.RecordDebt(amount);
            if (!debtResult.IsGood)
            {
                throw new InvalidOperationException(debtResult.Error);
            }

            var transactionResult = DebtTransaction.Create(
                customer.TenantId,
                customer.Account.Id,
                amount,
                TransactionType.Charge,
                $"Fiado por venta {notification.ReceiptNumber}");

            if (!transactionResult.TryGetResult(out var debtTx) || debtTx is null)
            {
                throw new InvalidOperationException(transactionResult.Error);
            }

            await _debtTransactionRepository.AddAsync(debtTx, cancellationToken);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
        }
    }
}
