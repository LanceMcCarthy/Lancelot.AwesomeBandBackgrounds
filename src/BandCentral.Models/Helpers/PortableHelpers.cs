using BandCentral.Models.Secrets;
using FlickrNet;
using System;
using System.Threading.Tasks;

namespace BandCentral.Models.Helpers
{
    public static class PortableHelpers
    {
        /// <summary>
        /// Portable Search Action to retrieve photos
        /// </summary>
        /// <param name="pageToGet"></param>
        /// <param name="photosPerPage"></param>
        /// <param name="safetyLevel"></param>
        /// <param name="searchTerm"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static async Task<PhotoCollection> GetPhotosAsync(int pageToGet, int photosPerPage, SafetyLevel safetyLevel, string searchTerm, PhotoSearchSortOrder sortOrder = PhotoSearchSortOrder.Relevance)
        {
            FlickrNet.Flickr flickr = new FlickrNet.Flickr(FlickrConstants.WindowsDevFlickrApiKey, FlickrConstants.WindowsDevFlickrSharedSecret);
            Exception exception = null;
            PhotoCollection photos = null;

            try
            {
                var searchOptions = new PhotoSearchOptions
                {
                    Page = pageToGet,
                    PerPage = photosPerPage,
                    SafeSearch = safetyLevel,
                    PrivacyFilter = PrivacyFilter.PublicPhotos,
                    Tags = searchTerm,
                    SortOrder = sortOrder,
                    Extras = PhotoSearchExtras.All
                };

                photos = await flickr.PhotosSearchAsync(searchOptions);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            if(exception != null)
            {
                return null;
            }

            return photos;
        }
    }
}
