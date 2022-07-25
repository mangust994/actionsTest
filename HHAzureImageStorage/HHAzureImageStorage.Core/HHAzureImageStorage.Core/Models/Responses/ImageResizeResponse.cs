using System.IO;

namespace HHAzureImageStorage.Core.Models.Responses
{
    public class ImageResizeResponse
    {
        public Stream ResizedStream { get; set; }

        public int WidthPixels { get; set; }

        public int HeightPixels { get; set; }

        public int SizeInBytes { get; set; }
    }
}
