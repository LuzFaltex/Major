using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Configuration;
using MajorInteractiveBot.Models;
using MajorInteractiveBot.Modules;
using MajorInteractiveBot.Services.CommandHelp;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace MajorInteractiveBot
{
    public class Program
    {

        // private IConfigurationRoot _appConfig;
        private ApplicationConfiguration _appConfig;
        private IApplicationLifetime applicationLifetime;

        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            /*
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables("MAJOR_");


            _appConfig = builder.Build();
            */

            _appConfig = (await ConfigurationManager.TryGetConfig<ApplicationConfiguration>()).GetValueOrDefault(ApplicationConfiguration.DefaultConfiguration);

            using (var services = ConfigureServices())
            {
                applicationLifetime = new ApplicationLifetime(services.GetRequiredService<ILogger<ApplicationLifetime>>());

                var majorBot = new MajorBot(services, applicationLifetime);
                await majorBot.StartAsync(applicationLifetime.ApplicationStopping);
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var seriLogger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            var commandService = new CommandService();

            var services = new ServiceCollection()
                .AddSingleton(_appConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(commandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<ICommandHelpService>(new CommandHelpService(commandService))
                .AddLogging(configure => configure.AddSerilog(seriLogger));

            /*
            services.Configure<LoggerFilterOptions>(options 
                => options.MinLevel = _appConfig.GetValue<LogLevel>("LogLevel"));
            */
            services.Configure<LoggerFilterOptions>(options
                => options.MinLevel = _appConfig.LogLevel);

            // services.Configure<ApplicationConfiguration>(_appConfig);

            return services.BuildServiceProvider();
        }
    }
}
