namespace TuColmadoRD.Core.Domain.ValueObjects.Base
{
    public abstract record DomainError(string Code, string Message)
    {
        public override string ToString() => $"[{Code}] {Message}";
    }

    public sealed record ValidationError(string Field, string Message)
        : DomainError("VAL_001", Message);

    public sealed record NotFoundError(string Entity, Guid Id)
        : DomainError("NF_001", $"{Entity} con id '{Id}' no fue encontrado.");
}
