using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Sales
{
    public class Sale : ITenantEntity
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Guid? CustomerId { get; private set; }
        public List<SaleDetail> Details { get; private set; } = new();

        public Money SubTotal { get; private set; }
        public Money TaxTotal { get; private set; }
        public Money Total { get; private set; }

        public SaleStatus Status { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }

        private Sale(Guid tenantId, PaymentMethod paymentMethod, Guid? customerId)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CreatedAt = DateTime.UtcNow;
            PaymentMethod = paymentMethod;
            CustomerId = customerId;
            Status = SaleStatus.Pending;

            SubTotal = Money.Zero;
            TaxTotal = Money.Zero;
            Total = Money.Zero;
        }

        public static OperationResult<Sale, string> Create(
            Guid tenantId,
            PaymentMethod paymentMethod,
            Guid? customerId = null)
        {
            if (tenantId == Guid.Empty)
                return OperationResult<Sale, string>.Bad("El TenantId es requerido.");

            if (paymentMethod == PaymentMethod.Credit && customerId == null)
                return OperationResult<Sale, string>.Bad("No se puede realizar una venta a crédito (Fiado) sin un cliente.");

            return OperationResult<Sale, string>.Good(new Sale(tenantId, paymentMethod, customerId));
        }

        public OperationResult<bool, string> AddItem(Guid productId, Quantity quantity, Money unitPrice, TaxRate taxRate)
        {
            var detailResult = SaleDetail.Create(this.Id, productId, quantity, unitPrice, taxRate);

            if (!detailResult.IsGood)
                return OperationResult<bool, string>.Bad(detailResult.Error!);

            Details.Add(detailResult.Result!);
            RecalculateTotals();

            return OperationResult<bool, string>.Good(true);
        }

        private void RecalculateTotals()
        {
            decimal subtotalAcc = 0;
            decimal taxAcc = 0;

            foreach (var detail in Details)
            {
                subtotalAcc += detail.SubTotal.Amount;
                taxAcc += detail.TaxAmount.Amount;
            }

            SubTotal = Money.FromDecimal(subtotalAcc).Result!;
            TaxTotal = Money.FromDecimal(taxAcc).Result!;
            Total = SubTotal + TaxTotal;
        }

        public void Complete() => Status = SaleStatus.Completed;
    }
}
