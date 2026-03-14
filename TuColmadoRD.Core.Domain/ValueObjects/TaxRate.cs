using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record TaxRate
    {
        public decimal Percentage { get; init; }
        public string Name { get; init; }

        private TaxRate(decimal percentage, string name)
        {
            Percentage = percentage;
            Name = name;
        }

        public static OperationResult<TaxRate, string> Create(decimal percentage, string name = "ITBIS")
        {
            if (percentage < 0 || percentage > 100)
                return OperationResult<TaxRate, string>.Bad("El porcentaje de impuesto debe estar entre 0 y 100.");

            return OperationResult<TaxRate, string>.Good(new TaxRate(percentage, name));
        }

        public OperationResult<Money, string> CalculateTax(Money baseAmount)
        {
            decimal taxAmount = baseAmount.Amount * (Percentage / 100m);
            return Money.FromDecimal(taxAmount);
        }
    }
}
