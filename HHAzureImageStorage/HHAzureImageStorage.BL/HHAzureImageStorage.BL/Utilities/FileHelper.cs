using HHAzureImageStorage.Domain.Enums;
using System.Collections.Generic;
using System.IO;

namespace HHAzureImageStorage.BL.Utilities
{
    public class FileHelper
    {
        public static string GetFileNamePrefix(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                case ImageVariant.Main:
                    return string.Empty;
                case ImageVariant.SmallThumbnail:
                    return "S";
                case ImageVariant.SmallThumbnailWithWatermark:
                    return "SW";
                case ImageVariant.MediumThumbnail:
                    return "M";
                case ImageVariant.MediumThumbnailWithWatermark:
                    return "MW";
                case ImageVariant.LargeThumbnail:
                    return "L";
                case ImageVariant.LargeThumbnailWithWatermark:
                    return "LW";
                case ImageVariant.Service:
                    return "SRV";
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
