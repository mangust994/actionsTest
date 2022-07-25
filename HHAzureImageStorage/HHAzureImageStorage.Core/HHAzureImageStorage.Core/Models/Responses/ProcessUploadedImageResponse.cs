namespace HHAzureImageStorage.Core.Models.Responses
{
    public class ProcessUploadedImageResponse
    {
        public int WidthPixels { get; set; }

        public int HeightPixels { get; set; }

        public long SizeInBytes { get; set; }

        public byte[] IgageBytes { get; set; }

        public MemoryStream ImageStream { get; set; }

        public MemoryStream ImageStreamForMagicImage { get; set; }
    }
}
