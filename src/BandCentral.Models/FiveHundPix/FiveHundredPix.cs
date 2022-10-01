using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Popups;
using BandCentral.Models.Common;
using Newtonsoft.Json;

namespace BandCentral.Models.FiveHundPix
{
    public class FiveHundredPix
    {
        private readonly string baseUrl = @"https://api.500px.com/v1/";
        private readonly string consumerKey;

        public string Excludes { get; set; }

        public IncrementalLoadingCollection<PixPhoto> PixPhotos { get; set; }

        public FiveHundredPix(string consumerKey, string excludes = "nude, people")
        {
            this.Excludes = excludes;
            this.consumerKey = consumerKey;
        }

        /// <summary>
        /// 500px API endpoint
        /// </summary>
        /// <param name="term">A keyword to search for.</param>
        /// <returns>A FiveHundredPixSearchResultRoot object </returns>
        public async Task<FiveHundredPixSearchResultRoot> SearchPhotosAsync(string term)
        {
            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            try
            {
                var handler = new HttpClientHandler();
                if(handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var url = $"{baseUrl}/photos/search?consumer_key={consumerKey}&feature=popular&exclude={Excludes}&term={term}";
                    var jsonResult = await client.GetStringAsync(url);

                    if (jsonResult == null)
                        return null;
                    
                    return JsonConvert.DeserializeObject<FiveHundredPixSearchResultRoot>(jsonResult);
                }
            }
            catch (HttpRequestException ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. Please check your internet connection and try again.\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. \r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }

        /// <summary>
        /// 500px API endpoint, searches tags. NOTE: Only 20 photos returned!
        /// </summary>
        /// <param name="tag"> A complete tag string to search for.</param>
        /// <returns>A FiveHundredPixSearchResultRoot object </returns>
        public async Task<FiveHundredPixSearchResultRoot> SearchTagAsync(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            try
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var url = $"{baseUrl}photos/search?consumer_key={consumerKey}&feature=popular&exclude={Excludes}&tag={tag}";
                    var jsonResult = await client.GetStringAsync(url);

                    if (jsonResult == null)
                        return null;

                    return JsonConvert.DeserializeObject<FiveHundredPixSearchResultRoot>(jsonResult);
                }
            }
            catch (HttpRequestException ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. Please check your internet connection and try again.\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. \r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }

        /// <summary>
        /// Get a particular photo
        /// </summary>
        /// <param name="id">photo id</param>
        /// <param name="size">
        /// 1	70px x 70px,
        /// 2	140px x 140px,
        /// 3	280px x 280px,
        /// 100	100px x 100px,
        /// 200	200px x 200px,
        /// 440	440px x 440px,
        /// 600	600px x 600px,
        /// 4	900px on the longest edge,
        /// 5	1170px on the longest edge,
        /// 6	1080px high,
        /// 20	300px high,
        /// 21	600px high,
        /// 30	256px on the longest edge,
        /// 31	450px high,
        /// 1080	1080px on the longest edge,
        /// 1600	1600px on the longest edge,
        /// 2048	2048px on the longest edge,
        /// </param>
        /// <returns></returns>
        public async Task<GetPixPhotoRoot> GetPhotoAsync(string id, int size = 440)
        {
            try
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var url = $"https://api.500px.com/v1/photos/{id}?consumer_key={consumerKey}&image_size={size}";

                    Debug.WriteLine($"GetPhoto url: {url}");
                    var jsonResult = await client.GetStringAsync(url);

                    if (jsonResult == null)
                        return null;

                    var result = JsonConvert.DeserializeObject<GetPixPhotoRoot>(jsonResult);

                    Debug.WriteLine($"GetPhoto Deserialized url: {result?.photo?.image_url}");

                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. Please check your internet connection and try again.\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. \r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }
        
        /// <summary>
        /// Search image database by category, use one of the predefined category ID numbers. 0 is default.
        /// </summary>
        /// <param name="category">
        /// 0	Uncategorized,
        /// 10	Abstract,
        /// 11	Animals,
        /// 5	Black and White,
        /// 1	Celebrities,
        /// 9	City and Architecture,
        /// 15	Commercial,
        /// 16	Concert,
        /// 20	Family,
        /// 14	Fashion,
        /// 2	Film,
        /// 24	Fine Art,
        /// 23	Food,
        /// 3	Journalism,
        /// 8	Landscapes,
        /// 12	Macro,
        /// 18	Nature,
        /// 4	Nude,
        /// 7	People,
        /// 19	Performing Arts,
        /// 17	Sport,
        /// 6	Still Life,
        /// 21	Street,
        /// 26	Transportation New!,
        /// 13	Travel,
        /// 22	Underwater,
        /// 27	Urban Exploration New!,
        /// 25	Wedding New!,
        /// </param>
        /// <returns></returns>
        public async Task<FiveHundredPixSearchResultRoot> SearchCategoryAsync(string category = "0")
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            try
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var jsonResult = await client.GetStringAsync($"{baseUrl}photos/search?feature=popular&consumer_key={consumerKey}?category={category}");

                    if (jsonResult == null)
                        return null;

                    return JsonConvert.DeserializeObject<FiveHundredPixSearchResultRoot>(jsonResult);
                }
            }
            catch (HttpRequestException ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. Please check your internet connection and try again.\r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong searching photos on 500px. \r\n\nError: {ex.Message}").ShowAsync();
                return null;
            }
        }

        /// <summary>
        /// Searches photos using page number (i.e. use with IncrementalLoadingCollection)
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="searchTerm"></param>
        /// <returns>Root object with page information and next set of photos</returns>
        public async Task<FiveHundredPixSearchResultRoot> SearchPhotosAsync(int pageNumber, int itemsPerPage, string searchTerm)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                    using (var client = new HttpClient(handler))
                    {
                        var url = $"{baseUrl}/photos/search?consumer_key={consumerKey}&feature=popular&exclude={Excludes}&term={searchTerm}&page={pageNumber}&rpp={itemsPerPage}";
                        var jsonResult = await client.GetStringAsync(url);

                        if (jsonResult == null)
                            return null;

                        return JsonConvert.DeserializeObject<FiveHundredPixSearchResultRoot>(jsonResult);

                    }
                }
                catch (HttpRequestException ex)
                {
                    await new MessageDialog($"Something went wrong searching photos on 500px. Please check your internet connection and try again.\r\n\nError: {ex.Message}").ShowAsync();
                    return null;
                }
                catch (Exception ex)
                {
                    await new MessageDialog($"Something went wrong searching photos on 500px. \r\n\nError: {ex.Message}").ShowAsync();
                    return null;
                }
            });
        }
    }
}
