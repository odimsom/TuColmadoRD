namespace TuColmadoRD.Core.Domain.Base
{
    /// <summary>
    /// Define que una entidad pertenece a un suscriptor (Colmado) específico.
    /// </summary>
    public interface ITenantEntity
    {
        Guid TenantId { get; }
    }
}
