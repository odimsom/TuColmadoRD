using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing
{
    public class PurchaseDetail
    {
        public Guid Id { get; private set; }
        public Guid PurchaseOrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public Quantity Quantity { get; private set; }
        public Money UnitCost { get; private set; }
        public Money SubTotal { get; private set; }

        internal PurchaseDetail(Guid purchaseId, Guid productId, Quantity quantity, Money unitCost)
        {
            Id = Guid.NewGuid();
            PurchaseOrderId = purchaseId;
            ProductId = productId;
            Quantity = quantity;
            UnitCost = unitCost;
            SubTotal = Money.FromDecimal(quantity.Value * unitCost.Amount).Result!;
        }
    }
}