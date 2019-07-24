using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Modules;
using MajorInteractiveBot.Services.CommandHelp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace MajorInteractiveBot
{
    public class Program
    {
        public static Task Main() => new Program().MainAsync();
        public async Task MainAsync()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    Console.WriteLine(ctx.HostingEnvironment.EnvironmentName);
                    builder.AddJsonFile("appsettings.json");
                    builder.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true);
                    builder.AddEnvironmentVariables("MAJOR_");

                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets("5B9EACD8-A154-40EB-B406-6D62DF2C6AB1");
                    }
                })
                .ConfigureLogging(builder =>
                {
                    var seriLogger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.RollingFile(@"logs\{date}", restrictedToMinimumLevel: LogEventLevel.Debug)
                    .CreateLogger();

                    builder.AddSerilog(seriLogger);
                })
                .ConfigureServices((context, services) =>
                {
                    var serviceCollection = new ServiceCollection()
                        .AddSingleton<DiscordSocketClient>()
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<ICommandHelpService, CommandHelpService>()

                        .AddHostedService<MajorBot>();
                });

            var built = hostBuilder.Build();

            await built.RunAsync();
        }
    }
}
