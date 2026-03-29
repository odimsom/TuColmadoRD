namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;

public interface ISystemConfigRepository
{
    Task<string?> GetLastKnownTimeAsync();
    Task UpdateLastKnownTimeAsync(string newTime);
}
