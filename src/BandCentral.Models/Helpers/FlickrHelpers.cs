using BandCentral.Models.Common;
using FlickrNet.Drawing;

namespace BandCentral.Models.Helpers
{
    public static class FlickrHelpers
    {
        public static string GetPhotoUrl(FlickrNet.Photo photo)
        {
            if (photo == null) return null;

            if (!string.IsNullOrEmpty(photo.Small320Url) && photo.Small320Width >= 310)
                return photo.Small320Url;
            if (!string.IsNullOrEmpty(photo.Medium640Url) && photo.Medium640Width > 310)
                return photo.Medium640Url;
            if (!string.IsNullOrEmpty(photo.Medium800Url) && photo.Medium800Width > 310)
                return photo.Medium800Url;
            if (!string.IsNullOrEmpty(photo.Large1600Url) && photo.Large1600Width > 310)
                return photo.Large1600Url;
            if (!string.IsNullOrEmpty(photo.Large2048Url) && photo.Large2048Width > 310)
                return photo.Large2048Url;
            if (!string.IsNullOrEmpty(photo.OriginalUrl) && photo.OriginalWidth > 310)
                return photo.OriginalUrl;

            return null;
        }

        public static ConsolidatedImageInfo GetPhotoInfo(FlickrNet.Photo photo, int currentScreenWidth)
        {
            if (photo == null) return null;

            if (!string.IsNullOrEmpty(photo.Small320Url) && photo.Small320Height != null
                && photo.Small320Width >= 310 && photo.Small320Width != null)
            {
                return new ConsolidatedImageInfo(photo.Small320Url,
                    new Size((int) photo.Small320Width, (int) photo.Small320Height),
                    currentScreenWidth);
            }

            if (!string.IsNullOrEmpty(photo.Medium640Url) && photo.Medium640Height != null
                && photo.Medium640Width > 310 && photo.Medium640Width != null)
            {
                return new ConsolidatedImageInfo(photo.Medium640Url,
                    new Size((int) photo.Medium640Width, (int) photo.Medium640Height),
                    currentScreenWidth);
            }

            if (!string.IsNullOrEmpty(photo.Medium800Url) && photo.Medium800Width != null
                && photo.Medium800Width > 310 && photo.Medium800Height != null)
            {
                return new ConsolidatedImageInfo(photo.Medium800Url,
                    new Size((int) photo.Medium800Width, (int) photo.Medium800Height),
                    currentScreenWidth);
            }

            if (!string.IsNullOrEmpty(photo.Large1600Url) && photo.Large1600Width != null
                && photo.Large1600Width > 310 && photo.Large1600Height != null)
            {
                return new ConsolidatedImageInfo(photo.Large1600Url,
                    new Size((int) photo.Large1600Width, (int) photo.Large1600Height),
                    currentScreenWidth);
            }

            if (!string.IsNullOrEmpty(photo.Large2048Url) && photo.Large2048Width != null
                && photo.Large2048Width > 310 & photo.Large2048Height != null)
            {
                return new ConsolidatedImageInfo(photo.Large2048Url,
                    new Size((int) photo.Large2048Width, (int) photo.Large2048Height),
                    currentScreenWidth);
            }

            if (!string.IsNullOrEmpty(photo.OriginalUrl) && photo.OriginalWidth != null
                && photo.OriginalWidth > 310 && photo.OriginalHeight != null)
            {
                return new ConsolidatedImageInfo(photo.OriginalUrl,
                    new Size((int) photo.OriginalWidth, (int) photo.OriginalHeight),
                    currentScreenWidth);
            }

            return null;
        }
        
    }
}

/*typical json response
 * 
 * 
 * 
"id":"16343521434",
 * "owner":"55576947@N00",
 * "secret":"e21650264e",
 * "server":"8708","farm":9,
 * ,
 * "ispublic":1,
 * "isfriend":0,
 * "isfamily":0,
 * "description":{"_content":""},
title="DSC_0139"
dateupload="1427634155"
datetaken="2015-03-08 13:21:51"
ownername="Rick & Kim"
views="0"
 url_sq="https://farm9.staticflickr.com/8708/16343521434_e21650264e_s.jpg" 
 height_sq="75"
 width_sq=75"
 url_t="https://farm9.staticflickr.com/8708/16343521434_e21650264e_t.jpg"
 height_t="66"
 width_t="100"
 url_s="https://farm9.staticflickr.com/8708/16343521434_e21650264e_m.jpg"
 height_s="159"
 width_s="240"
 url_q="https://farm9.staticflickr.com/8708/16343521434_e21650264e_q.jpg"
 height_q="150"
 width_q="150"
 url_m="https://farm9.staticflickr.com/8708/16343521434_e21650264e.jpg"
 height_m="332"
 width_m="500"
 url_n":"https://farm9.staticflickr.com/8708/16343521434_e21650264e_n.jpg"
 height_n="213"
 width_n="320"
 url_z="https://farm9.staticflickr.com/8708/16343521434_e21650264e_z.jpg"
 height_z=425"
 width_z="640"
 url_c="https://farm9.staticflickr.com/8708/16343521434_e21650264e_c.jpg"
 height_c="531"
 width_c="800"
 url_l="https://farm9.staticflickr.com/8708/16343521434_e21650264e_b.jpg"
 height_l="680"
 width_l="1024"
 url_o="https://farm9.staticflickr.com/8708/16343521434_749f2bba2f_o.jpg"
 height_o="680"
 width_o="1024"
 * 
 * 
 title="IMG_20150329_090033"
 url_sq="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_s.jpg"
 height_sq="75"
 width_sq="75"
 url_t="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_t.jpg"
 height_t="100"
 width_t="67"
 url_s="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_m.jpg"
 height_s="240"
 width_s="160"
 url_q="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_q.jpg"
 height_q="150"
 width_q="150"
 url_m="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5.jpg"
 height_m="500"
 width_m="334"
 url_n="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_n.jpg"
 height_n="320"
 width_n="214"
 url_z="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_z.jpg"
 height_z="640"
 width_z="428"
 url_c="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_c.jpg"
 height_c="800"
 width_c="534"
 url_l="https://farm9.staticflickr.com/8684/16343521504_c5f768cab5_b.jpg"
 height_l="1024"
 width_l="684"
 url_o="https://farm9.staticflickr.com/8684/16343521504_87391dba22_o.jpg"
 height_o="1616"
 width_o="1080"

{"id":"16345786623","owner":"94046760@N00","secret":"120049fc0c","server":"7631","farm":8,"title":"Untitled photo","ispublic":1,"isfriend":0,"isfamily":0,"description":{"_content":""},"dateupload":"1427634150","datetaken":"2015-03-29 06:02:30","datetakengranularity":0,"datetakenunknown":"1","ownername":"tony-28","views":"0","url_sq":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_s.jpg","height_sq":75,"width_sq":75,"url_t":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_t.jpg","height_t":"67","width_t":"100","url_s":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_m.jpg","height_s":"160","width_s":"240","url_q":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_q.jpg","height_q":"150","width_q":"150","url_m":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c.jpg","height_m":"333","width_m":"500","url_n":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_n.jpg","height_n":213,"width_n":"320","url_z":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_z.jpg","height_z":"427","width_z":"640","url_c":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_c.jpg","height_c":534,"width_c":"800","url_l":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_120049fc0c_b.jpg","height_l":"683","width_l":"1024","url_o":"https:\/\/farm8.staticflickr.com\/7631\/16345786623_6212272ec3_o.jpg","height_o":"3456","width_o":"5184"}
 * 
 * 
 * 
{"id":"16343521114",
"owner":"69859194@N06",
"secret":"2837ebaa63",
"server":"7624",
"farm":8,
"title":"_DSC1434.jpg",
"ispublic":1,
"isfriend":0,
"isfamily":0,
"description":{"_content":""},
"dateupload":"1427634153",
"datetaken":"2014-05-25 20:32:16",
"datetakengranularity":"0",
"datetakenunknown":"0",
"ownername":"cdao11",
"views":"0",
-----------------------------------------------------------------------------------------
 * URLS
"url_sq":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_s.jpg",
"url_t":"https://farm8.staticflickr.com/7624\/16343521114_2837ebaa63_t.jpg",
"url_s":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_m.jpg",
"url_q":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_q.jpg",
"url_m":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63.jpg",
"url_n":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_n.jpg",
"url_z":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_z.jpg",
"url_c":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_c.jpg",
"url_l":"https://farm8.staticflickr.com/7624/16343521114_2837ebaa63_b.jpg",
"url_o":"https://farm8.staticflickr.com/7624/16343521114_62430aee15_o.jpg",
-----------------------------------------------------------------------------------------
 * DIMENSIONS
 height_sq":75,
"width_sq":75,
"height_t":"67","
width_t":"100",
"height_s":"161",
"width_s":"240",
"height_q":"150",
"width_q":"150",
"height_m":"334",
"width_m":"500",
"height_n":214,
"width_n":"320",
"height_z":"428",
"width_z":"640",
"height_c":535,
"width_c":"800",
"height_l":"685",
"width_l":"1024",
"height_o":"2592",
"width_o":"3872"}
 * 
 */
