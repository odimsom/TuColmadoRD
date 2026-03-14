using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Sales
{
    public class Shift
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid CashierId { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }

        public Money InitialCash { get; private set; }
        public Money? ActualCashAtClose { get; private set; }
        public ShiftStatus Status { get; private set; }

        public Money TotalCashSales { get; private set; } = Money.Zero;
        public Money TotalCreditSales { get; private set; } = Money.Zero;

        private Shift(Guid tenantId, Guid cashierId, Money initialCash)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CashierId = cashierId;
            InitialCash = initialCash;
            StartTime = DateTime.UtcNow;
            Status = ShiftStatus.Open;
        }

        public static OperationResult<Shift, string> Open(Guid tenantId, Guid cashierId, Money initialCash)
        {
            if (tenantId == Guid.Empty) return OperationResult<Shift, string>.Bad("TenantId requerido.");

            return OperationResult<Shift, string>.Good(new Shift(tenantId, cashierId, initialCash));
        }

        public void RegisterSale(PaymentMethod method, Money amount)
        {
            if (method == PaymentMethod.Cash)
                TotalCashSales = TotalCashSales + amount;
            else if (method == PaymentMethod.Credit)
                TotalCreditSales = TotalCreditSales + amount;
        }

        public OperationResult<bool, string> Close(Money actualCash)
        {
            if (Status != ShiftStatus.Open)
                return OperationResult<bool, string>.Bad("El turno ya está cerrado o suspendido.");

            ActualCashAtClose = actualCash;
            EndTime = DateTime.UtcNow;
            Status = ShiftStatus.Closed;

            return OperationResult<bool, string>.Good(true);
        }

        public Money ExpectedCash => InitialCash + TotalCashSales;
    }
}
