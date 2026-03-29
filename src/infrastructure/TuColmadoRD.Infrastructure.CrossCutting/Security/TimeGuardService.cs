using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Security;

public class TimeGuardService : ITimeGuard
{
    private readonly ISystemConfigRepository _configRepo;

    public TimeGuardService(ISystemConfigRepository configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task<OperationResult<DateTime, SubscriptionError>> GetLastKnownTimeAsync()
    {
        var configValue = await _configRepo.GetLastKnownTimeAsync();

        if (string.IsNullOrEmpty(configValue) || !DateTime.TryParse(configValue, out var lkt))
        {
            return OperationResult<DateTime, SubscriptionError>.Good(DateTime.MinValue);
        }

        return OperationResult<DateTime, SubscriptionError>.Good(lkt);
    }

    public async Task<OperationResult<Unit, SubscriptionError>> AdvanceTimeAsync(DateTime newTime)
    {
        var lktResult = await GetLastKnownTimeAsync();
        if (!lktResult.TryGetResult(out var lkt))
        {
            return OperationResult<Unit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected);
        }

        if (newTime < lkt)
        {
            return OperationResult<Unit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected);
        }

        try
        {
            await _configRepo.UpdateLastKnownTimeAsync(newTime.ToString("O"));
            return OperationResult<Unit, SubscriptionError>.Good(Unit.Value);
        }
        catch
        {
            return OperationResult<Unit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected);
        }
    }
}
