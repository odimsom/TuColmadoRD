using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record Quantity
    {
        public decimal Value { get; init; }
        public string UnitLabel { get; init; }
        public UnitType Type { get; init; }

        private Quantity(decimal value, string unitLabel, UnitType type)
        {
            Value = value;
            UnitLabel = unitLabel;
            Type = type;
        }

        public static OperationResult<Quantity, string> Create(decimal value, string unitLabel, UnitType type)
        {
            if (value <= 0)
                return OperationResult<Quantity, string>.Bad("La cantidad debe ser mayor a cero.");

            if (type == UnitType.Unitary && value % 1 != 0)
            {
                return OperationResult<Quantity, string>.Bad("Los productos por unidad no pueden venderse en fracciones.");
            }

            if (string.IsNullOrWhiteSpace(unitLabel))
                return OperationResult<Quantity, string>.Bad("Debe especificar una etiqueta de unidad (Lb, Ud, etc).");

            return OperationResult<Quantity, string>.Good(new Quantity(value, unitLabel, type));
        }

        public override string ToString() => $"{Value} {UnitLabel}";
    }
}
