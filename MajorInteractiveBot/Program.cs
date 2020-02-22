﻿using Discord.Addons.Interactive;
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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace MajorInteractiveBot
{
    public class Program
    {
        public static async Task<int> Main()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.AddEnvironmentVariables("MAJOR_");
                    builder.AddJsonFile("appsettings.json", true);
                    builder.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true);

                    Debug.WriteLine(ctx.HostingEnvironment.EnvironmentName);

                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<MajorBot>();
                    }
                })
                .ConfigureLogging((ctx, builder) =>
                {
                    var logMinimum = new Serilog.Core.LoggingLevelSwitch(ctx.HostingEnvironment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information);
                    var seriLogger = new LoggerConfiguration()
                        .WriteTo.Console(levelSwitch: logMinimum)
                        .WriteTo.RollingFile(Path.Combine("logs", "{Date}"), restrictedToMinimumLevel: LogEventLevel.Debug)
                        .CreateLogger();

                    builder.AddSerilog(seriLogger);
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .Configure<MajorConfig>(context.Configuration)
                        .AddDbContext<MajorContext>(options =>
                        {
                            // options.UseSqlite(context.Configuration.GetValue<string>(nameof(MajorConfig.DbConnection)));
                            options.UseNpgsql(context.Configuration.GetConnectionString("MajorDb"));
                        })
                        .AddMemoryCache()
                        .AddSingleton<DiscordSocketClient>()
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<InteractiveService>()
                        .AddSingleton<ICommandHelpService, CommandHelpService>()
                        .AddSingleton<HttpClient>()

                        .AddHostedService<MajorBot>();
                });

            // Ensure disposal at the end of the Main() block
            using IHost built = hostBuilder.Build();

            try
            {
                await built.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.ForContext<Program>()
                    .Fatal(ex, "Host terminated unexpectedly.");

                if (Debugger.IsAttached && Environment.UserInteractive)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to exit...");
                    Console.ReadKey(true);
                }

                return ex.HResult;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
