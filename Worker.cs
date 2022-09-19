namespace GettingStarted;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Worker : BackgroundService
{
    private IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;

    public Worker(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var _publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await _publishEndpoint.Publish(new GettingStarted { Value = $"The time is {DateTimeOffset.Now}" }, stoppingToken);

            await dbContext.SaveChangesAsync();

            await Task.Delay(5000, stoppingToken);
        }
    }
}