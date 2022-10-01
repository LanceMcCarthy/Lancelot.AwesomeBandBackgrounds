using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BandCentral.Models.Bing
{
    public class BingImageHelper
    {
        private DateTime lastRefreshedOn;
        private BingImagesResult imagesCache;
        private readonly string apiEndpoint = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0";

        public BingImageHelper()
        {
            
        }

        public async Task<List<BingImage>> GetBingImagesAsync()
        {
            try
            {
                await UpdateLocalCache();
                return imagesCache?.images;
            }
            catch (HttpRequestException ex)
            {
                //await new MessageDialog($"There was a problem getting the Bing Images, please check your internet connection. \r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                //await new MessageDialog($"There was an unknown problem in GetBingImagesAsync.\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }

        public async Task<BingImage> GetTodaysBingImageAsync()
        {
            try
            {
                await UpdateLocalCache();
                return imagesCache?.images[0];

            }
            catch (HttpRequestException ex)
            {
                //await new MessageDialog($"There was a problem getting today's Bing Image, please check your internet connection. \r\n\nError: {ex.Message}").ShowAsync();
                return new BingImage() { copyright = "error" };
            }
            catch (Exception ex)
            {
                //await new MessageDialog($"There was an unknown problem in GetTodaysBingImageAsync.\r\n\nError: {ex.Message}").ShowAsync();
                return new BingImage() { copyright = "error" };
            }
        }

        private async Task UpdateLocalCache(int count = 8)
        {
            try
            {
                //in case we've already retrieved them and it is still the same day, no need to make another network call
                if (imagesCache?.images.Count > 0 && (lastRefreshedOn.DayOfYear < DateTime.Now.DayOfYear))
                {
                    return;
                }

                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var stringResponse = await client.GetStringAsync($"{apiEndpoint}&n={count}");

                    var result = JsonConvert.DeserializeObject<BingImagesResult>(stringResponse);

                    imagesCache = result;
                    lastRefreshedOn = DateTime.Now;
                    Debug.WriteLine($"UpdateLocalCache Complete: {imagesCache.images.Count}");
                }
            }
            catch (HttpRequestException ex)
            {
                //await new MessageDialog($"There was a problem refreshing the cache, please check your internet connection. \r\n\nError: {ex.Message}").ShowAsync();
            }
            catch (Exception ex)
            {
                //await new MessageDialog($"There was an unknown problem getting Bing Image of the Day.\r\n\nError: {ex.Message}").ShowAsync();
            }
        }
    }
}
