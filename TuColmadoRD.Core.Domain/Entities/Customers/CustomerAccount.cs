using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class CustomerAccount : ITenantEntity
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Money Balance { get; private set; }
        public Money CreditLimit { get; private set; }
        public DateTime LastActivity { get; private set; }

        public CreditLimitStatus Status
        {
            get
            {
                if (Balance.Amount > CreditLimit.Amount)
                    return CreditLimitStatus.Exceeded;

                if (Balance.Amount >= (CreditLimit.Amount * 0.8m))
                    return CreditLimitStatus.NearLimit;

                return CreditLimitStatus.Healthy;
            }
        }

        private CustomerAccount(Guid tenantId, Guid customerId)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CustomerId = customerId;
            Balance = Money.Zero;
            CreditLimit = Money.FromDecimal(5000).Result!;
            LastActivity = DateTime.UtcNow;
        }

        internal static CustomerAccount CreateNew(Guid tenantId, Guid customerId)
        {
            return new CustomerAccount(tenantId, customerId);
        }

        /// <summary>
        /// Incrementa la deuda (Fiao).
        /// </summary>
        public OperationResult<bool, string> RecordDebt(Money amount)
        {
            var projectedBalance = Balance + amount;

            if (projectedBalance.Amount > CreditLimit.Amount)
            {
                return OperationResult<bool, string>.Bad(
                    $"Operación rechazada: El límite de crédito es {CreditLimit} y el nuevo balance sería {projectedBalance}.");
            }

            Balance = projectedBalance;
            LastActivity = DateTime.UtcNow;
            return OperationResult<bool, string>.Good(true);
        }

        /// <summary>
        /// Registra un abono (Pagar la libreta).
        /// </summary>
        public OperationResult<bool, string> RecordPayment(Money payment)
        {
            var result = Balance - payment;

            if (!result.IsGood)
            {
                return OperationResult<bool, string>.Bad($"Error en abono: {result.Error}");
            }

            Balance = result.Result!;
            LastActivity = DateTime.UtcNow;
            return OperationResult<bool, string>.Good(true);
        }

        public void UpdateCreditLimit(Money newLimit)
        {
            CreditLimit = newLimit;
            LastActivity = DateTime.UtcNow;
        }
    }
}
