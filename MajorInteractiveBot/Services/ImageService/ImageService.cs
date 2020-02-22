using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MajorInteractiveBot.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MajorInteractiveBot.Services.ImageService
{
    public class ImageService
    {
        private readonly HttpClient _httpClient;

        public ImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves a json document from the specified URL and returns the deserialized form as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the json document into.</typeparam>
        /// <param name="url">The web address from which to fetch the json document.</param>
        /// <returns>An <see cref="ImageServiceResult{TResult}"/> containing the deserialized object.</returns>
        public async Task<ImageServiceResult<T>> FromJsonAsync<T>(string url)
        {
            var result = await GetResponseOrEmptyAsync(url);

            return result.IsSuccess
                ? ImageServiceResult<T>.FromSuccess(JsonConvert.DeserializeObject<T>(result.Result))
                : ImageServiceResult<T>.FromError(result.ErrorReason);
        }

        /// <summary>
        /// Retrieves a json document from the specified URL and returns a single value from the document as a string.
        /// </summary>
        /// <param name="url">The web address from which to fetch the json document.</param>
        /// <param name="key">The name of the key from which to get the value.</param>
        /// <param name="returnFormat">The format which is placed into the value of the <see cref="ImageServiceResult{string}"/>.</param>
        /// <returns>An <see cref="ImageServiceResult{string}"/> containing the requested value as a string.</returns>
        public Task<ImageServiceResult<string>> FromJsonAsync(string url, string key)
           => FromJsonAsync<string>(url, key);

        /// <summary>
        /// Retrieves a json document from the specified URL and returns a single value from the document as a the specified type.
        /// </summary>
        /// <typeparam name="T">The returned value will be converted into this specified type.</typeparam>
        /// <param name="url">The web address from which to fetch the json document.</param>
        /// <param name="key">The name of the key from which to get the value.</param>
        /// <param name="returnFormat">The format which is placed into the value of the <see cref="ImageServiceResult{TResult}"/>.</param>
        /// <returns>An <see cref="ImageServiceResult{TResult}"/> containing the requested value as a string.</returns>
        public async Task<ImageServiceResult<T>> FromJsonAsync<T>(string url, string key)
        {
            var result = await GetResponseOrEmptyAsync(url);

            if (result.IsSuccess)
            {
                var jArray = JArray.Parse(result.Result);
                var jToken = jArray.First;
                return ImageServiceResult<T>.FromSuccess(jToken.Value<T>(key));
            }
            else
            {
                return ImageServiceResult<T>.FromError(result.ErrorReason);
            }
        }

        public async Task<ImageUrlResult> FromBodyAsync(string url, string baseUrl = "{0}")
        {
            var result = await GetResponseOrEmptyAsync(url);

            return result.IsSuccess
                ? ImageUrlResult.FromSuccess(string.Format(baseUrl, result.Result))
                : result;
        }

        private async Task<ImageServiceResult<string>> GetResponseOrEmptyAsync(string url)
        {
            var result = await _httpClient.GetAsync(url);
            return result.IsSuccessStatusCode
                ? ImageServiceResult<string>.FromSuccess(await result.Content.ReadAsStringAsync())
                : ImageServiceResult<string>.FromError(result.ReasonPhrase);
        }
    }

    public struct ImageUrlResult : IResult<string>
    {
        public bool IsSuccess { get; }
        public string Result { get; }
        public string ErrorReason { get; }
        public Exception Exception => null;

        private ImageUrlResult(string result, string errorMessage, bool isSuccess)
        {
            Result = result;
            ErrorReason = errorMessage;
            IsSuccess = isSuccess;
        }

        internal static ImageUrlResult FromSuccess(string url)
            => new ImageUrlResult(url, string.Empty, true);
        internal static ImageUrlResult FromError(string errorMessage)
            => new ImageUrlResult(string.Empty, errorMessage, false);

        public override bool Equals(object obj)
            => obj is ImageUrlResult result
                ? Equals(result)
                : false;

        public bool Equals(ImageUrlResult imageUrlResult)
            => IsSuccess == imageUrlResult.IsSuccess &&
                Result.Equals(imageUrlResult.Result) &&
                ErrorReason.Equals(imageUrlResult.ErrorReason);

        public override int GetHashCode()
            => HashCode.Combine(IsSuccess, Result, ErrorReason);

        public static bool operator ==(ImageUrlResult left, ImageUrlResult right) 
            => left.Equals(right);

        public static bool operator !=(ImageUrlResult left, ImageUrlResult right)
            => !(left == right);

        public static implicit operator ImageUrlResult(ImageServiceResult<string> imageServiceResult)
        {
            if (imageServiceResult.IsSuccess)
            {
                if (Uri.IsWellFormedUriString(imageServiceResult.Result, UriKind.Absolute))
                    return FromSuccess(imageServiceResult.Result);
                else
                    ThrowHelper.ThrowInvalidOperationException($"{nameof(imageServiceResult.Result)} is malformed or is not a URL.");
                return default;
            }
            else
            {
                ThrowHelper.ThrowInvalidOperationException($"{nameof(imageServiceResult.Result)} is malformed or is not a URL.");
                return default;
            }
        }
    }

    public struct ImageServiceResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess { get; }
        public TResult Result { get; }
        public string ErrorReason { get; }
        public Exception Exception { get; }

        private ImageServiceResult(TResult result, string errorMessage, bool isSuccess, Exception exception)
        {
            Result = result;
            ErrorReason = errorMessage;
            IsSuccess = isSuccess;
            Exception = exception;
        }

        internal static ImageServiceResult<TResult> FromSuccess(TResult result)
            => new ImageServiceResult<TResult>(result, string.Empty, true, null);
        internal static ImageServiceResult<TResult> FromError(string errorMessage, Exception exception = null)
            => new ImageServiceResult<TResult>(default, errorMessage, false, null);

        public override bool Equals(object obj) 
            => obj is ImageServiceResult<TResult> result
                ? Equals(result)
                : false;

        public bool Equals(ImageServiceResult<TResult> result)
            => IsSuccess == result.IsSuccess &&
                Result.Equals(result.Result) &&
                ErrorReason.Equals(result.ErrorReason);

        public override int GetHashCode()
            => HashCode.Combine(IsSuccess, Result, ErrorReason, Exception);

        public static bool operator ==(ImageServiceResult<TResult> left, ImageServiceResult<TResult> right) 
            => left.Equals(right);

        public static bool operator !=(ImageServiceResult<TResult> left, ImageServiceResult<TResult> right) 
            => !(left == right);
    }
}
