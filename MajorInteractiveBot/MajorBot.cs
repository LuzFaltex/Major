using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Data.Models;
using MajorInteractiveBot.Extensions;
using Microsoft.EntityFrameworkCore;
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
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly DiscordSerilogAdapter _serilogAdapter;
        private IServiceScope _scope;
        private readonly ILogger<MajorBot> Log;
        private readonly MajorContext _context;
        private readonly MajorConfig _applicationConfig;

        public MajorBot(IServiceProvider services, MajorContext context)
        {
            _provider = services;
            _context = context;

            _applicationConfig = _provider.GetRequiredService<IOptions<MajorConfig>>().Value;
            _client = _provider.GetRequiredService<DiscordSocketClient>();
            _commands = _provider.GetRequiredService<CommandService>();
            _applicationLifetime = _provider.GetRequiredService<IApplicationLifetime>();
            _serilogAdapter = new DiscordSerilogAdapter(_provider.GetRequiredService<ILogger<DiscordSerilogAdapter>>());
            Log = _provider.GetRequiredService<ILogger<MajorBot>>();
        }

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
                _client.LeftGuild += LeftGuild;
                _client.UserJoined += UserJoined;
                _client.UserLeft += UserLeft;
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

                _client.Disconnected -= OnDisconnect;

                _client.Log -= _serilogAdapter.HandleLog;
                _commands.Log -= _serilogAdapter.HandleLog;

                _client.JoinedGuild -= JoinedGuild;
                _client.LeftGuild -= LeftGuild;
                _client.UserJoined -= UserJoined;
                _client.UserLeft -= UserLeft;
            }
        }
        
        private async Task UserJoined(SocketGuildUser user)
        {
            var guild = _context.Guilds.Find(user.Guild.Id);
            var channelId = guild.GreetingChannel;

            if (!guild.GreetUser || channelId == ulong.MaxValue) return;

            var channel = user.Guild.GetTextChannel(channelId);

            await channel.SendMessageAsync(guild.GreetingMessage);

            Log.LogInformation("User {User} joined {Guild}", user.UsernameAndDiscrim(), user.Guild.Name);
        }
        private Task UserLeft(SocketGuildUser user)
        {
            Log.LogInformation("User {User} left {Guild}", user.UsernameAndDiscrim(), user.Guild.Name);
            return Task.CompletedTask;
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            // Disable suggestion to simplify Data.Models.Guild to Guild
            // to avoid confusion between Data.Models.Guild and some flavor of Discord Guild.
            #pragma warning disable IDE0001
            var guildConfig = new Data.Models.Guild()
            {
                GuildId = guild.Id,
                CommandPrefix = "."
            };
            #pragma warning restore IDE0001
            HashSet<Module> modules = GetAllModules(guild.Id);
            await _context.Guilds.AddAsync(guildConfig);
            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();
        }

        private async Task LeftGuild(SocketGuild guild)
        {
            var guildConfig = await _context.Guilds.FindAsync(guild.Id);
            if (!(guildConfig is null))
            {
                _context.Guilds.Remove(guildConfig);
            }

            var cmdChannels = await _context.CommandChannels
                .Where(x => x.GuildId == guild.Id).ToListAsync();
            if (cmdChannels.Count > 0)
            {
                _context.CommandChannels.RemoveRange(cmdChannels);
            }

            var modules = await _context.Modules
                .Where(x => x.GuildId == guild.Id).ToListAsync();
            if (modules.Count > 0)
            {
                _context.Modules.RemoveRange(modules);
            }

            await _context.SaveChangesAsync();
        }

        private HashSet<Module> GetAllModules(ulong guildId)
        {
            var modules = _provider.GetRequiredService<CommandService>().Modules;

            var ret = new HashSet<Module>();

            foreach (var module in modules)
            {
                ret.Add(new Module()
                {
                    GuildId = guildId,
                    ModuleName = module.Name,
                    Enabled = true
                });
            }

            return ret;
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

                // await _client.LoginAsync(TokenType.Bot, _context["DiscordToken"]);
                await _client.LoginAsync(TokenType.Bot, _applicationConfig.DiscordToken);
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
