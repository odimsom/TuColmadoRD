using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class Product : ITenantEntity
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid CategoryId { get; private set; }

        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string? Barcode { get; private set; }
        public string? ImageUrl { get; private set; }

        public UnitType UnitType { get; private set; }
        public Quantity StockQuantity { get; private set; }
        public Quantity MinStock { get; private set; }

        public Money CostPrice { get; private set; }
        public Money SalePrice { get; private set; }

        public TaxRate ItbisRate { get; private set; }
        public TaxRate? IscRate { get; private set; }

        private Product(Guid tenantId, string name, Guid categoryId, UnitType unitType, Money cost, Money sale, TaxRate itbis)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CategoryId = categoryId;
            Name = name;
            ShortName = name.Length > 20 ? name.Substring(0, 20) : name;
            UnitType = unitType;
            CostPrice = cost;
            SalePrice = sale;
            ItbisRate = itbis;

            string label = unitType == UnitType.Mass ? "Lb" : "Ud";
            StockQuantity = Quantity.Create(0, label, unitType).Result!;
            MinStock = Quantity.Create(5, label, unitType).Result!;
        }

        public static OperationResult<Product, string> Create(
            Guid tenantId, string name, Guid categoryId, UnitType unitType,
            Money cost, Money sale, TaxRate itbis, string? barcode = null)
        {
            if (tenantId == Guid.Empty) return OperationResult<Product, string>.Bad("TenantId requerido.");
            if (string.IsNullOrWhiteSpace(name)) return OperationResult<Product, string>.Bad("Nombre requerido.");
            if (sale.Amount <= cost.Amount) return OperationResult<Product, string>.Bad("El precio de venta debe dejar margen de ganancia.");

            var product = new Product(tenantId, name, categoryId, unitType, cost, sale, itbis)
            {
                Barcode = barcode
            };
            return OperationResult<Product, string>.Good(product);
        }

        public void SetIsc(TaxRate isc) => IscRate = isc;

        public void SetImageUrl(string url) => ImageUrl = url;

        public Money GetTotalTaxedPrice()
        {
            var itbis = ItbisRate.CalculateTax(SalePrice).Result!;
            var isc = IscRate?.CalculateTax(SalePrice).Result! ?? Money.Zero;
            return SalePrice + itbis + isc;
        }

        public OperationResult<bool, string> SubtractStock(Quantity amount)
        {
            if (amount.Type != this.UnitType)
                return OperationResult<bool, string>.Bad("Tipo de unidad incompatible.");

            if (amount.Value > StockQuantity.Value)
                return OperationResult<bool, string>.Bad($"Stock insuficiente de {Name}.");

            StockQuantity = Quantity.Create(StockQuantity.Value - amount.Value, StockQuantity.UnitLabel, this.UnitType).Result!;
            return OperationResult<bool, string>.Good(true);
        }

        public void AddStock(Quantity amount)
        {
            if (amount.Type == this.UnitType)
            {
                StockQuantity = Quantity.Create(StockQuantity.Value + amount.Value, StockQuantity.UnitLabel, this.UnitType).Result!;
            }
        }
    }
}
