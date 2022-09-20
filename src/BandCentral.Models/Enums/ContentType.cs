using System.ComponentModel;
using BandCentral.Models.Attributes;

namespace BandCentral.Models.Enums
{
    public enum ContentType
    {
        /// <summary>
        /// No content type specified.
        /// </summary>
        [Display("None Specified")]
        None = 0,
        /// <summary>
        /// For normal photographs.
        /// </summary>
        [Display("Normal Photos")]
        PhotosOnly = 1,
        /// <summary>
        /// For screenshots.
        /// </summary>
        [Display("Screenshots Only")]
        ScreenshotsOnly = 2,
        /// <summary>
        /// For other uploads, such as artwork./// 
        /// </summary>
        [Display("Other (artwork, etc)")] 
        OtherOnly = 3,
        /// <summary>
        /// Search for photos and screenshots
        /// </summary>
        [Display("Photos and Screenshots")]
        PhotosAndScreenshots = 4,
        /// <summary>
        /// Search for screenshots and others
        /// </summary>
        [Display("Screenshots and Other Uploads")]
        ScreenshotsAndOthers = 5,
        /// <summary>
        /// Search for photos and other things
        /// </summary>
        [DefaultValue(true)]
        [Display("Photos and Other")]
        PhotosAndOthers = 6,
        /// <summary>
        /// Search for anything (default)
        /// </summary>
        [Display("All Types")]
        All = 7
    }
}
