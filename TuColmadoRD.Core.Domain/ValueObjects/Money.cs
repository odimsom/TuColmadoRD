using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; init; }
        public string Currency { get; } = "DOP";

        private Money(decimal amount)
        {
            Amount = amount;
        }

        public static OperationResult<Money, string> FromDecimal(decimal amount)
        {
            return amount < 0 
                ? OperationResult<Money, string>.Bad("Cantidad No Puede Ser Negativa") 
                : OperationResult<Money, string>.Good(new Money(amount));
        }
        public static Money Zero
            => new(0);
        public static Money operator +(Money a, Money b)
            => new(a.Amount + b.Amount);
        public static OperationResult<Money, string> operator -(Money a, Money b)
            => a.Amount < b.Amount 
               ? OperationResult<Money, string>.Bad("Resultado De La Cantidad No Puede Ser Negativa") 
               : OperationResult<Money, string>.Good(new Money(a.Amount - b.Amount));
        override public string ToString()
            => $"RD$ {Amount}";
    }
}
