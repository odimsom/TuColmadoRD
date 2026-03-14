using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class UnitConversion : ITenantEntity
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid ProductId { get; private set; }

        public UnitOfMeasureEntity FromUnit { get; private set; }
        public UnitOfMeasureEntity ToUnit { get; private set; }
        public decimal Factor { get; private set; }

        private UnitConversion(Guid tenantId, Guid productId, UnitOfMeasureEntity fromUnit, UnitOfMeasureEntity toUnit, decimal factor)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            ProductId = productId;
            FromUnit = fromUnit;
            ToUnit = toUnit;
            Factor = factor;
        }

        public static OperationResult<UnitConversion, string> Create(
            Guid tenantId,
            Guid productId,
            UnitOfMeasureEntity fromUnit,
            UnitOfMeasureEntity toUnit,
            decimal factor)
        {
            if (tenantId == Guid.Empty) return OperationResult<UnitConversion, string>.Bad("TenantId requerido.");
            if (factor <= 0) return OperationResult<UnitConversion, string>.Bad("El factor debe ser mayor a cero.");
            if (fromUnit == toUnit) return OperationResult<UnitConversion, string>.Bad("No se requiere conversión para la misma unidad.");

            return OperationResult<UnitConversion, string>.Good(
                new UnitConversion(tenantId, productId, fromUnit, toUnit, factor)
            );
        }
    }
}
