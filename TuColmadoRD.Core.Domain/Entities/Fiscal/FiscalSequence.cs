using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Fiscal;
using TuColmadoRD.Domain.Core.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal
{
    public class FiscalSequence : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }

        public FiscalReceiptType Type { get; private set; }
        public string Prefix { get; private set; }
        public long CurrentNumber { get; private set; }
        public long EndNumber { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public bool IsActive { get; private set; }

        private FiscalSequence(TenantIdentifier tenantId, FiscalReceiptType type, string prefix, long start, long end, DateTime expiration)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Type = type;
            Prefix = prefix;
            CurrentNumber = start;
            EndNumber = end;
            ExpirationDate = expiration;
            IsActive = true;
        }

        public static OperationResult<FiscalSequence, string> Create(
            TenantIdentifier tenantId, FiscalReceiptType type, string prefix, long start, long end, DateTime expiration)
        {
            if (end <= start) return OperationResult<FiscalSequence, string>.Bad("El número final debe ser mayor al inicial.");
            if (expiration <= DateTime.Now) return OperationResult<FiscalSequence, string>.Bad("La secuencia ya está vencida.");

            return OperationResult<FiscalSequence, string>.Good(new FiscalSequence(tenantId, type, prefix, start, end, expiration));
        }

        public OperationResult<string, string> GetNextFullNumber()
        {
            if (!IsActive) return OperationResult<string, string>.Bad("Secuencia inactiva.");
            if (CurrentNumber > EndNumber) return OperationResult<string, string>.Bad("Secuencia agotada.");
            if (DateTime.Now > ExpirationDate) return OperationResult<string, string>.Bad("Secuencia vencida.");

            string fullNumber = $"{Prefix}{(int)Type:D2}{CurrentNumber:D8}";
            CurrentNumber++;

            return OperationResult<string, string>.Good(fullNumber);
        }
    }
}