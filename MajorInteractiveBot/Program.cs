using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Data.Models;
using MajorInteractiveBot.Modules;
using MajorInteractiveBot.Services.CommandHelp;
using Microsoft.EntityFrameworkCore;
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
                        builder.AddUserSecrets<MajorBot>();
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
                        .Configure<MajorConfig>(context.Configuration)
                        .AddDbContext<MajorContext>(options =>
                        {
                            // options.UseSqlite(context.Configuration.GetValue<string>(nameof(MajorConfig.DbConnection)));
                            options.UseNpgsql(context.Configuration.GetConnectionString("MajorDb"));
                        })
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
