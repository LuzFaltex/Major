using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MajorInteractiveBot.Attributes;
using MajorInteractiveBot.Services.ImageService;
using MajorInteractiveBot.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MajorInteractiveBot.Modules
{
    [Name("Fun")]
    [Summary("Provides miscelaneous fun commands")]
    [Module]
    public sealed class FunModule : ModuleBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly ImageService _imageService;


        private const string xkcdPath = "https://xkcd.com/info.0.json";
        private const string xkcdSpecificPath = "https://xkcd.com/{0}/info.0.json";
        private const string xkcdMemoryKey = "kxcdLatest";
        private const int defaultXkcdMax = 2100;
        public const string Empty = "";

        public FunModule(IMemoryCache memoryCache, ILogger<FunModule> logger, ImageService imageService)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _imageService = imageService;
        }

        [Command("8ball")]
        [Summary("Answers a yes or no question")]
        public async Task EightBall([Remainder] string question = "")
        {
            // get random responses and reply with them
            // ThrowHelper.ThrowNotImplementedException();

        }

        [Command("trapcard")]
        [Summary("Turn the tables with your ultimate Yu-Gi-Oh trap card!")]
        public async Task TrapCard()
        {
            ThrowHelper.ThrowNotImplementedException();
        }

        [Command("xkcd"), Summary("Posts a random xkcd article")]
        public async Task PostRandomXkcd()
        {
            var latestComicId = await _memoryCache.GetOrCreateAsync(xkcdMemoryKey, GetLatestComicId);

            var random = new Random();

            await PostXkcd(random.Next(1, latestComicId));
        }

        [Command("xkcd"), Summary("Posts the specified xkcd article")]
        public async Task PostXkcd([Summary("The zero-based Id of the article you're trying to get.")] int id)
        {
            string jsonUrl = string.Format(xkcdSpecificPath, id);
            ImageUrlResult result = await _imageService.FromJsonAsync(jsonUrl, "img");

            if (result.IsSuccess)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithImageUrl(result.Result)
                    .WithAuthor(Context.User)
                    .WithCurrentTimestamp();

                await ReplyAsync(embed: embedBuilder.Build());
            }
            else
            {
                await ReplyAsync("Could not find the image specified.");
            }
        }

        /// <summary>
        /// Retrieves the latest XKCD comic id
        /// </summary>
        private async Task<int> GetLatestComicId(ICacheEntry cache)
        {
            IResult<string> latestResult = await _imageService.FromJsonAsync(xkcdPath, "num");

            if (!(latestResult.IsSuccess && int.TryParse(latestResult.Result, out int latest)))
            {
                latest = defaultXkcdMax;
            }

            cache.SetValue(latest);
            return latest;
        }
    }
}
