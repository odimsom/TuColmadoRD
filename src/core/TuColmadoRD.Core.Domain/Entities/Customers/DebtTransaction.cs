using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class DebtTransaction : ITenantEntity
    {
    private DebtTransaction() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid CustomerAccountId { get; private set; }
        public Money Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public string Concept { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private DebtTransaction(TenantIdentifier tenantId, Guid accountId, Money amount, TransactionType type, string concept)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CustomerAccountId = accountId;
            Amount = amount;
            Type = type;
            Concept = concept;
            CreatedAt = DateTime.UtcNow;
        }

        public static OperationResult<DebtTransaction, string> Create(
            TenantIdentifier tenantId,
            Guid accountId,
            Money amount,
            TransactionType type,
            string concept)
        {
            if (string.IsNullOrWhiteSpace(concept))
                return OperationResult<DebtTransaction, string>.Bad("El concepto es obligatorio.");

            return OperationResult<DebtTransaction, string>.Good(
                new DebtTransaction(tenantId, accountId, amount, type, concept.Trim())
            );
        }
    }
}
