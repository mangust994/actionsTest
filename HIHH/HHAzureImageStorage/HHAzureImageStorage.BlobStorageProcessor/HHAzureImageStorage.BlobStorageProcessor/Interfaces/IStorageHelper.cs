using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.Threading.Tasks;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.BlobStorageProcessor.Interfaces
{
    public interface IStorageHelper
    {
        string GetStorageAccountName(ImageVariant variant);

        string GetContainerName(ImageVariant variant);

        Task<BlobClient> GetBlobClientAsync(string fileName, ImageVariant imageVariant);

        string GetBlobUri(string fileName, DateTime urlExpireDateTime,
            BlobContainerSasPermissions permissions, ImageVariant imageVariant);
        string UploadFileGetName(Uri uri);
    }
}
