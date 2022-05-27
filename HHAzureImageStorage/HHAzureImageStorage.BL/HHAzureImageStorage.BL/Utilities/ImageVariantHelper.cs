using HHAzureImageStorage.Domain.Enums;
using System;

namespace HHAzureImageStorage.BL.Utilities
{
    public static class ImageVariantHelper
    {
        private const int largeThumbSizeInPixels = 1024;

        private const int middleThumbSizeInPixels = 512;

        private const int smallThumbSizeInPixels = 230;

        /* static */

        public static bool IsWithWatermark(ImageVariant thumb)
        {
            return thumb.ToString().EndsWith("WithWatermark");
        }

        public static int GetThumbSize(ImageVariant thumb)
        {
            switch (thumb)
            {
                default:
                    var msg = string.Format("Not found size for \"{0}\".", thumb);
                    throw new ArgumentException(msg);
                //case ImageVariant.HiRes:
                //    return 0;
                case ImageVariant.LargeThumbnail:
                case ImageVariant.LargeThumbnailWithWatermark:
                    return largeThumbSizeInPixels;
                case ImageVariant.MediumThumbnail:
                case ImageVariant.MediumThumbnailWithWatermark:
                    return middleThumbSizeInPixels;
                case ImageVariant.SmallThumbnail:
                case ImageVariant.SmallThumbnailWithWatermark:
                    return smallThumbSizeInPixels;
            }
        }

        public static string GetPhotoPrefix(ImageVariant thumb)
        {
            switch (thumb)
            {
                default: return thumb.ToString();
                //case ImageVariant.HiRes: return string.Empty;
                case ImageVariant.LargeThumbnail: return "L";
                case ImageVariant.MediumThumbnail: return "M";
                case ImageVariant.SmallThumbnail: return "S";
                case ImageVariant.LargeThumbnailWithWatermark: return "LW";
                case ImageVariant.MediumThumbnailWithWatermark: return "MW";
                case ImageVariant.SmallThumbnailWithWatermark: return "SW";
            }
        }

        public static ImageVariant GetTypeFromString(string stringWaterMarkType)
        {
            switch (stringWaterMarkType)
            {
                case "Temp":
                    return ImageVariant.Temp;
                case "Main":
                    return ImageVariant.Main;
                case "SmallThumbnail":
                    return ImageVariant.SmallThumbnail;
                case "SmallThumbnailWithWatermark":
                    return ImageVariant.SmallThumbnailWithWatermark;
                case "MediumThumbnail":
                    return ImageVariant.MediumThumbnail;
                case "MediumThumbnailWithWatermark":
                    return ImageVariant.MediumThumbnailWithWatermark;
                case "LargeThumbnail":
                    return ImageVariant.LargeThumbnail;
                case "LargeThumbnailWithWatermark":
                    return ImageVariant.LargeThumbnailWithWatermark;
                case "Watermark":
                case "Service":
                    return ImageVariant.Service;
                default:
                    return ImageVariant.Main;
            }
        }

        public static ImageVariant GetImageVariantOppositeWatermarkVariant(ImageVariant thumb)
        {
            switch (thumb)
            {
                default: 
                    return ImageVariant.SmallThumbnailWithWatermark;                
                case ImageVariant.LargeThumbnailWithWatermark: 
                    return ImageVariant.LargeThumbnail;
                case ImageVariant.MediumThumbnailWithWatermark: 
                    return ImageVariant.MediumThumbnail;
                case ImageVariant.SmallThumbnailWithWatermark: 
                    return ImageVariant.SmallThumbnail;
            }
        }
    }
}
