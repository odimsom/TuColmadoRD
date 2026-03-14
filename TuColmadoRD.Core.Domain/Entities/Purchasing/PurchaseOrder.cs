using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.InventoryPurchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Domain.Core.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing
{
    public class PurchaseOrder : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid SupplierId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public PurchaseStatus Status { get; private set; }
        public List<PurchaseDetail> Details { get; private set; } = new();
        public Money TotalAmount { get; private set; }

        private PurchaseOrder(TenantIdentifier tenantId, Guid supplierId)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            SupplierId = supplierId;
            OrderDate = DateTime.UtcNow;
            Status = PurchaseStatus.Draft;
            TotalAmount = Money.Zero;
        }

        public static OperationResult<PurchaseOrder, string> Create(TenantIdentifier tenantId, Guid supplierId)
        {
            return OperationResult<PurchaseOrder, string>.Good(new PurchaseOrder(tenantId, supplierId));
        }

        public void AddItem(Guid productId, Quantity quantity, Money unitCost)
        {
            var detail = new PurchaseDetail(Id, productId, quantity, unitCost);
            Details.Add(detail);
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            decimal totalAcc = 0;
            foreach (var item in Details) totalAcc += item.SubTotal.Amount;
            TotalAmount = Money.FromDecimal(totalAcc).Result!;
        }

        public void MarkAsReceived() => Status = PurchaseStatus.Received;
    }
}