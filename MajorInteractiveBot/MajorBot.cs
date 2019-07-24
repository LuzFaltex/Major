using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Configuration;
using MajorInteractiveBot.Extensions;
using MajorInteractiveBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajorInteractiveBot
{
    public class MajorBot : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        // private readonly IConfiguration _config;
        private readonly ApplicationConfiguration _appConfig;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly DiscordSerilogAdapter _serilogAdapter;
        private IServiceScope _scope;
        private readonly ILogger<MajorBot> Log;

        public MajorBot(IServiceProvider services, IApplicationLifetime applicationLifetime)
        {
            _provider = services;

            _client = _provider.GetRequiredServiceOrThrow<DiscordSocketClient>();
            _commands = _provider.GetRequiredServiceOrThrow<CommandService>();
            // _config = _provider.GetRequiredServiceOrThrow<IConfiguration>();
            _appConfig = _provider.GetRequiredServiceOrThrow<ApplicationConfiguration>();
            _applicationLifetime = applicationLifetime;
            _serilogAdapter = new DiscordSerilogAdapter(_provider.GetRequiredServiceOrThrow<ILogger<DiscordSerilogAdapter>>());
            Log = _provider.GetRequiredServiceOrThrow<ILogger<MajorBot>>();
        }

        /// <inheritdoc />
        public override Task StartAsync(CancellationToken stoppingToken)
            => ExecuteAsync(stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            Log.LogInformation("Starting bot...");

            try
            {
                // create a new scope
                _scope = _provider.CreateScope();

                Log.LogTrace("Registering listeners for Discord Client Events.");              

                _client.Disconnected += OnDisconnect;

                _client.Log += _serilogAdapter.HandleLog;
                _client.JoinedGuild += JoinedGuild;
                _client.UserJoined += UserJoined;
                _commands.Log += _serilogAdapter.HandleLog;

                // Register the cancellation token so we can stop listening to events if the server is
                // shutting down or being disposed.
                stoppingToken.Register(OnStopping);

                Log.LogInformation("Starting behaviors.");
                // We don't have any rn

                Log.LogInformation("Loading command modules.");

                await _commands.AddModulesAsync(typeof(MajorBot).Assembly, _scope.ServiceProvider);

                Log.LogInformation("{Modules} modules loaded, containing {Commands} commands", _commands.Modules.Count(), _commands.Modules.SelectMany(d => d.Commands).Count());

                Log.LogInformation("Logging into Discord and starting the client.");

                await StartClient(stoppingToken);

                Log.LogInformation("Discord client started successfully.");

                await Task.Delay(-1);

            }
            catch (Exception ex)
            {
                Log.LogError(ex, "An error occurred while attempting to start the background service.");

                try
                {
                    OnStopping();

                    Log.LogInformation("Logging out of Discord.");
                    await _client.LogoutAsync();
                }
                finally
                {
                    _scope?.Dispose();
                    _scope = null;
                }

                throw;
            }

            void OnStopping()
            {
                Log.LogInformation("Stopping bot service.");

                ConfigurationManager.WriteConfig(_appConfig);

                _client.Disconnected -= OnDisconnect;

                _client.Log -= _serilogAdapter.HandleLog;
                _commands.Log -= _serilogAdapter.HandleLog;
            }
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            var guild = _appConfig.GuildConfigurations[user.Guild.Id];
            var channelId = guild.GreetingChannel;

            if (!guild.GreetUser || channelId == ulong.MaxValue) return;

            var channel = user.Guild.GetTextChannel(channelId);

            await channel.SendMessageAsync(guild.GreetingMessage);            
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            var guildConfig = new GuildConfiguration(guild);
            // var appConfig = _provider.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;
            var appConfig = _provider.GetRequiredService<ApplicationConfiguration>();
            appConfig.GuildConfigurations.Add(guild.Id, guildConfig);

            await ConfigurationManager.WriteConfig(appConfig);
        }

        private Task OnDisconnect(Exception ex)
        {
            Log.LogInformation(ex, "The bot disconnected unexpectedly. Stopping the application.");
            _applicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            try
            {
                // IF the service is currently running, this will cancel the cancellation token that was passed into
                // our ExecuteAsync method, unregistering our event handlers for us.
                base.Dispose();
            }
            finally
            {
                _scope?.Dispose();
                _client.Dispose();
            }            
        }

        private async Task StartClient(CancellationToken cancellationToken)
        {
            try
            {
                _client.Ready += OnClientReady;

                cancellationToken.ThrowIfCancellationRequested();

                // await _client.LoginAsync(TokenType.Bot, _config["DiscordToken"]);
                await _client.LoginAsync(TokenType.Bot, _appConfig.BotToken);
                await _client.StartAsync();
            }
            catch (Exception)
            {
                _client.Ready -= OnClientReady;

                throw;
            }

            async Task OnClientReady()
            {
                Log.LogTrace("Discord client is ready. Setting up game status.");
                _client.Ready -= OnClientReady;
                await _client.SetGameAsync("anime", type: ActivityType.Watching);
            }
        }


    }
}
