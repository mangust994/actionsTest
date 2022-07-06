using HHAzureImageStorage.Domain.Entities;
using System;
using System.IO;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class GetImageUploadSasUrlDto
    {
        public Guid ImageId { get; set; }

        public int PhotographerKey { get; set; }

        public int AlbumKey { get; set; }

        public int EventKey { get; set; }

        public string OriginalFileName { get; set; }

        public string FileName { get; set; }

        public bool ColorCorrectLevel { get; set; } = false;

        public bool AutoThumbnails { get; set; } = true;

        public string WatermarkImageId { get; set; }

        public string WatermarkMethod { get; set; }

        public DateTime? ExpirationDate { get; set; } = null;

        public bool HiResDownload { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

        public static GetImageUploadSasUrlDto CreateInstance(string originalFileName)
        {
            //Build the GUID file name for storage
            var guid = Guid.NewGuid();
            var ext = Path.GetExtension(originalFileName);
            var guidFileName = String.Format("{0}{1}", guid, ext);

            return new GetImageUploadSasUrlDto()
            {
                ImageId = guid,
                FileName = guidFileName,
                OriginalFileName = originalFileName
            };
        }

        public ImageUpload CreateImageUploadEntity() => new()
        {
            id = ImageId,
            ColorCorrectLevel = ColorCorrectLevel,
            OriginalImageName = OriginalFileName,
            FileName = FileName,
            AutoThumbnails = AutoThumbnails,
            WatermarkImageId = WatermarkImageId,
            WatermarkMethod = WatermarkMethod,
            hhihPhotographerKey = PhotographerKey,
            hhihEventKey = EventKey,
            AlbumKey = AlbumKey,
            HiResDownload = HiResDownload,
            HasTransparentAlphaLayer = HasTransparentAlphaLayer,
            ExpirationDate = ExpirationDate
        };
    }
}
