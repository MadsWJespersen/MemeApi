using MemeApi.library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MemeApi.library;

public class ConsumeScopedServiceHostedService : BackgroundService
{

    private readonly CrontabSchedule _crontabSchedule;
    private DateTime _nextRun;
    private const string Schedule = "30 12 * * *"; // run day at 1 am

    public IServiceProvider Services { get; }
    public ConsumeScopedServiceHostedService(IServiceProvider services)
    {
        _crontabSchedule = CrontabSchedule.Parse(Schedule);
        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
        Services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(UntilNextExecution(), stoppingToken); // wait until next time

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IMemeOfTheDayService>();

                await scopedProcessingService.ExecuteAsync(stoppingToken);
            }

            _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
        }
    }
    private int UntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);


    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}
