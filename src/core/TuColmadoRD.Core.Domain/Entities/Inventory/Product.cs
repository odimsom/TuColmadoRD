using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class Product : ITenantEntity
    {
    private Product() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
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

        private Product(TenantIdentifier tenantId, string name, Guid categoryId, UnitType unitType, Money cost, Money sale, TaxRate itbis)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CategoryId = categoryId;
            Name = name;
            ShortName = name.Length > 20 ? name[..20] : name;
            UnitType = unitType;
            CostPrice = cost;
            SalePrice = sale;
            ItbisRate = itbis;

            string label = unitType switch
            {
                UnitType.Mass => "Lb",
                UnitType.Volume => "Lt",
                _ => "Ud"
            };

            var stockQuantityResult = Quantity.Create(0, label, unitType);
            if (!stockQuantityResult.IsGood)
            {
                throw new InvalidOperationException(stockQuantityResult.Error ?? "Error al crear la cantidad inicial de stock.");
            }
            StockQuantity = stockQuantityResult.Result!;

            var minStockResult = Quantity.Create(5, label, unitType);
            if (!minStockResult.IsGood)
            {
                throw new InvalidOperationException(minStockResult.Error ?? "Error al crear la cantidad m�nima de stock.");
            }
            MinStock = minStockResult.Result!;
        }

        public static OperationResult<Product, string> Create(
            TenantIdentifier tenantId, string name, Guid categoryId, UnitType unitType,
            Money cost, Money sale, TaxRate itbis, string? barcode = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return OperationResult<Product, string>.Bad("Nombre requerido.");
            if (sale.Amount <= cost.Amount) return OperationResult<Product, string>.Bad("El precio de venta debe dejar margen de ganancia.");

            var product = new Product(tenantId, name, categoryId, unitType, cost, sale, itbis)
            {
                Barcode = barcode
            };
            return OperationResult<Product, string>.Good(product);
        }

        public static OperationResult<Product, string> RehydrateForCatalogSync(
            Guid productId,
            TenantIdentifier tenantId,
            Guid categoryId,
            string name,
            Money cost,
            Money sale,
            TaxRate itbis)
        {
            var createResult = Create(tenantId, name, categoryId, UnitType.Unitary, cost, sale, itbis);
            if (!createResult.TryGetResult(out var product) || product is null)
            {
                createResult.TryGetError(out var error);
                return OperationResult<Product, string>.Bad(error ?? "No se pudo crear el producto para sync.");
            }

            product.Id = productId;
            product.ShortName = name.Length > 20 ? name[..20] : name;

            return OperationResult<Product, string>.Good(product);
        }

        public OperationResult<Unit, string> UpdateFromCatalogSync(
            Guid categoryId,
            string name,
            Money cost,
            Money sale)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult<Unit, string>.Bad("Nombre requerido.");

            if (sale.Amount <= cost.Amount)
                return OperationResult<Unit, string>.Bad("El precio de venta debe dejar margen de ganancia.");

            CategoryId = categoryId;
            Name = name;
            ShortName = name.Length > 20 ? name[..20] : name;
            CostPrice = cost;
            SalePrice = sale;

            return OperationResult<Unit, string>.Good(Unit.Value);
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
