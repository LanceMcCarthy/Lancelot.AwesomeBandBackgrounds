using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlickrNet;

namespace BandCentralBase.Common
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
            Flickr flickr = new Flickr(Constants.WindowsDevFlickrApiKey, Constants.WindowsDevFlickrSharedSecret);
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
