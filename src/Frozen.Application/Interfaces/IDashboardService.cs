using Frozen.Application.DTOs.Dashboard;

namespace Frozen.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
