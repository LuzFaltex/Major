using Discord;
using Discord.Commands;
using MajorInteractiveBot.Attributes;
using MajorInteractiveBot.Services.ImageService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("Animal")]
    [Summary("A collection of commands for displaying animals of the specified type.")]
    [Module]
    public sealed class AnimalModule : ModuleBase
    {
        private readonly ILogger Log;
        private readonly ImageService _imageService;

        public AnimalModule(ILogger<AnimalModule> logger, ImageService imageService)
        {
            Log = logger;
            _imageService = imageService;
        }

        private const string FindBird = "https://random.birb.pw/tweet/";
        private const string BirdPicture = "https://random.birb.pw/img/{0}";
        private const string CatPicture = "https://api.thecatapi.com/v1/images/search";

        [Command("bird")]
        [Alias("birb")]
        [Summary("Shows a random bird.")]
        [RequireCommandChannel]
        public async Task ShowBird()
        {
            var message = await ReplyAsync("Searching for a bird...");
            var result = await _imageService.FromBodyAsync(FindBird, BirdPicture);

            await ShowPictureAsync(message, result);
        }

        [Command("cat")]
        [Summary("Show a random cat")]
        [RequireCommandChannel]
        public async Task ShowCat()
        {
            // string location = await GetJsonAsync(CatPicture, "url");
            // await ShowPicture("cat", location);

            var message = await ReplyAsync("Searching for a cat...");
            var result = await _imageService.FromJsonAsync(CatPicture, "url");

            await ShowPictureAsync(message, result);
        }

        private async Task ShowPictureAsync(IUserMessage message, ImageUrlResult result)
        {
            if (result.IsSuccess)
            {
                var embed = new EmbedBuilder().WithUrl(result.Result);

                await message.ModifyAsync(msg =>
                {
                    msg.Content = "Found one!";
                    msg.Embed = embed.Build();
                });
            }
            else
            {
                await message.ModifyAsync(msg =>
                {
                    msg.Content = "Image lookup failed.";
                });
            }
        }
    }
}
