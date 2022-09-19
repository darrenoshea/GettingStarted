using System;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GettingStarted
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<RegistrationDbContext>(x =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("Default");

                        x.UseSqlServer(connectionString, options =>
                        {
                            options.MinBatchSize(1);
                        });
                    });

                    services.AddMassTransit(x =>
                    {

                        x.AddEntityFrameworkOutbox<RegistrationDbContext>(o =>
                        {
                            o.QueryDelay = TimeSpan.FromSeconds(1);

                            o.UseSqlServer();

                            o.UseBusOutbox();
                        });

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly);
                        x.AddSagaStateMachines(entryAssembly);
                        x.AddSagas(entryAssembly);
                        x.AddActivities(entryAssembly);

                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            var connStr = hostContext.Configuration.GetConnectionString("AzureSb");
                            cfg.Host(connStr);

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
