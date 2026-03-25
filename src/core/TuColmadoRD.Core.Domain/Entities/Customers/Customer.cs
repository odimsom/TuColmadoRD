using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class Customer : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string FullName { get; private set; }
        public Cedula DocumentId { get; private set; }
        public Phone? ContactPhone { get; private set; }
        public Address? HomeAddress { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Customer() { }
        public CustomerAccount Account { get; private set; }

        private Customer(TenantIdentifier tenantId, string fullName, Cedula documentId, Phone? phone, Address? address)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            FullName = fullName;
            DocumentId = documentId;
            ContactPhone = phone;
            HomeAddress = address;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;

            Account = CustomerAccount.CreateNew(tenantId, Id);
        }

        public static OperationResult<Customer, string> Create(
            TenantIdentifier tenantId,
            string fullName,
            Cedula documentId,
            Phone? phone = null,
            Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return OperationResult<Customer, string>.Bad("El nombre completo es obligatorio.");
            if (fullName.Length > 100)
                return OperationResult<Customer, string>.Bad("El nombre completo no puede exceder los 100 caracteres.");

            return OperationResult<Customer, string>.Good(new Customer(tenantId, fullName, documentId, phone, address));
        }

        public void Deactivate() => IsActive = false;
    }
}
