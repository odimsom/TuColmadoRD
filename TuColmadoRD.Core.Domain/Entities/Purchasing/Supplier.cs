using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing
{
    public class Supplier : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string BusinessName { get; private set; }
        public Rnc TaxId { get; private set; }
        public SupplierType Type { get; private set; }
        public Phone? ContactPhone { get; private set; }
        public bool IsActive { get; private set; }

        private Supplier(TenantIdentifier tenantId, string businessName, Rnc taxId, SupplierType type, Phone? phone)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            BusinessName = businessName;
            TaxId = taxId;
            Type = type;
            ContactPhone = phone;
            IsActive = true;
        }

        public static OperationResult<Supplier, string> Create(TenantIdentifier tenantId, string businessName, Rnc taxId, SupplierType type, Phone? phone = null)
        {
            if (string.IsNullOrWhiteSpace(businessName)) return OperationResult<Supplier, string>.Bad("Nombre comercial requerido.");

            return OperationResult<Supplier, string>.Good(new Supplier(tenantId, businessName.Trim(), taxId, type, phone));
        }

        public void Deactivate() => IsActive = false;
    }
}