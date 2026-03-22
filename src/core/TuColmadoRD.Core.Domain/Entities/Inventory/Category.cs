using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class Category : ITenantEntity
    {
    private Category() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }

        public string? IconPath { get; private set; }
        public string? ColorHex { get; private set; }

        public bool IsActive { get; private set; }

        private Category(TenantIdentifier tenantId, string name, string? description, string? icon, string? color)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            Description = description;
            IconPath = icon;
            ColorHex = color;
            IsActive = true;
        }

        public static OperationResult<Category, string> Create(
            TenantIdentifier tenantId,
            string name,
            string? description = null,
            string? icon = null,
            string? color = "#3498db")
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult<Category, string>.Bad("El nombre de la categoría es obligatorio.");

            return OperationResult<Category, string>.Good(
                new Category(tenantId, name.Trim(), description?.Trim(), icon, color)
            );
        }

        public void UpdateMetadata(string icon, string color)
        {
            IconPath = icon;
            ColorHex = color;
        }

        public void Deactivate() => IsActive = false;
    }
}
