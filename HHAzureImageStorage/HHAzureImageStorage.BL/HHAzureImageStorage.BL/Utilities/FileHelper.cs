using HHAzureImageStorage.Domain.Enums;
using System.Collections.Generic;
using System.IO;

namespace HHAzureImageStorage.BL.Utilities
{
    public class FileHelper
    {
        public const string SmallThumbnailPrefix = "S";
        public const string SmallThumbnailWithWatermarkPrefix = "SW";
        public const string MediumThumbnailPrefix = "M";
        public const string MediumThumbnailWithWatermarkPrefix = "MW";
        public const string LargeThumbnailPrefix = "L";
        public const string LargeThumbnailWithWatermarkPrefix = "LW";
        public const string ServicePrefix = "SRV";

        public static string GetFileNamePrefix(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                case ImageVariant.Main:
                    return string.Empty;
                case ImageVariant.SmallThumbnail:
                    return SmallThumbnailPrefix;
                case ImageVariant.SmallThumbnailWithWatermark:
                    return SmallThumbnailWithWatermarkPrefix;
                case ImageVariant.MediumThumbnail:
                    return MediumThumbnailPrefix;
                case ImageVariant.MediumThumbnailWithWatermark:
                    return MediumThumbnailWithWatermarkPrefix;
                case ImageVariant.LargeThumbnail:
                    return LargeThumbnailPrefix;
                case ImageVariant.LargeThumbnailWithWatermark:
                    return LargeThumbnailWithWatermarkPrefix;
                case ImageVariant.Service:
                    return ServicePrefix;
                default:
                    return string.Empty;
            }
        }

        public static string GetFileName(string imageId, string prefix, string fileName)
        {
            //string randomFileName = Path.GetRandomFileName();
            //string str = Path.GetFileNameWithoutExtension(fileName).TruncateLongString(10);
            string extension = Path.GetExtension(fileName);

            return $"{prefix}_{imageId}{extension}";
        }

        public static IDictionary<string, string> CreateMetaData(string imageId, string contentType)
        {
            return new Dictionary<string, string>()
                  {
                    {
                      "ImageId",
                      imageId
                    },
                    {
                      "ContentType",
                      contentType
                    }
                  };
        }
    }
}
