using System.Threading.Tasks;
using System.Threading;

namespace MemeApi.library.Services;

internal interface IMemeOfTheDayService
{
    public Task ExecuteAsync(CancellationToken stoppingToken);
}