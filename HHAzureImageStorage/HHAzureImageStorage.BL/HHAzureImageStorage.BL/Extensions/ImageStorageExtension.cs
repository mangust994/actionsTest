using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using System;

namespace HHAzureImageStorage.BL.Extensions
{
    public class ImageStorageExtension
    {
        public static ImageStorage CreateImageStorageEntity(
          Guid imageId,
          ImageVariant variant,
          string storageAccountName,
          string bloabContainerName,
          string blobName)
        {
            return new ImageStorage()
            {
                id = Guid.NewGuid(),
                imageId = imageId,
                imageVariantId = variant,
                Status = ImageStatus.InProgress,
                StorageAccount = storageAccountName,
                Container = bloabContainerName,
                CreatedDate = DateTime.Now,
                BlobName = blobName
            };
        }
    }
}
