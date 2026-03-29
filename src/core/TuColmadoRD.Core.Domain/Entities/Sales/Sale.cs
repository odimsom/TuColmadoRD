using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales
{
    public class Sale : ITenantEntity
    {
        private Sale()
        {
            TenantId = TenantIdentifier.Empty;
            Total = Money.Zero;
        }

        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid ShiftId { get; private set; }
        public Guid TerminalId { get; private set; }
        public SaleStatus Status { get; private set; }
        public Money Total { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public static OperationResult<Sale, DomainError> Create(
            Guid tenantId,
            Guid terminalId,
            Guid shiftId,
            Money total)
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Business("sale.not_implemented"));
        }

        public static OperationResult<Sale, string> Create(
            TenantIdentifier tenantId,
            PaymentMethod paymentMethod,
            Guid? customerId = null)
        {
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ShiftId = Guid.Empty,
                TerminalId = Guid.Empty,
                Status = SaleStatus.Completed,
                Total = Money.Zero,
                CreatedAt = DateTime.UtcNow
            };

            return OperationResult<Sale, string>.Good(sale);
        }

        internal static Sale Rehydrate(
            Guid id,
            Guid tenantId,
            Guid terminalId,
            Guid shiftId,
            Money total,
            SaleStatus status,
            DateTime createdAt)
        {
            return new Sale
            {
                Id = id,
                TenantId = TenantIdentifier.Validate(tenantId).Result,
                TerminalId = terminalId,
                ShiftId = shiftId,
                Total = total,
                Status = status,
                CreatedAt = createdAt
            };
        }
    }
}
