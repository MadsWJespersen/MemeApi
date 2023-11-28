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

    public IServiceProvider Services { get; }
    public ConsumeScopedServiceHostedService(IServiceProvider services)
    {
        Services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedule = CrontabSchedule.Parse("30 12 * * *"); // Run once a day at 12:30
        var nextRun = schedule.GetNextOccurrence(DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            if (now > nextRun)
            {
                using (var scope = Services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IMemeOfTheDayService>();

                    await scopedProcessingService.ExecuteAsync(stoppingToken);
                }

                nextRun = schedule.GetNextOccurrence(DateTime.Now);
            }
            
            // Wait for 24 hours before the next execution
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}
