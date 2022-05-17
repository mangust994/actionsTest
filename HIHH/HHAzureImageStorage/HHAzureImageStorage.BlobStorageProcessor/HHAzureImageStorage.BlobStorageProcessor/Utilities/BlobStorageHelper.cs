using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using System;
using System.Threading.Tasks;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.BlobStorageProcessor.Utilities
{
    public class BlobStorageHelper : IStorageHelper
    {
        private static BlobStorageSettings _blobStorageSettings;

        private static BlobServiceClient _testBlobServiceClient;
        private static BlobServiceClient _mainBlobServiceClient;
        private static BlobServiceClient _thumbnailBlobServiceClient;

        private static StorageSharedKeyCredential _uploadCredential;
        private static StorageSharedKeyCredential _storageCredential;

        public BlobStorageHelper(BlobStorageSettings blobStorageSettings)
        {
            _blobStorageSettings = blobStorageSettings;
        }

        private StorageSharedKeyCredential UploadCredential
        {
            get
            {
                if (_uploadCredential == null)
                {
                    _uploadCredential = new StorageSharedKeyCredential(_blobStorageSettings.AccountNameMain,
                        _blobStorageSettings.AccountKeyMain);
                }

                return _uploadCredential;
            }
        }

        private StorageSharedKeyCredential StorageCredential
        {
            get
            {
                if (_storageCredential == null)
                {
                    _storageCredential = new StorageSharedKeyCredential(_blobStorageSettings.AccountNameTemp,
                        _blobStorageSettings.AccountKeyTemp);
                }

                return _storageCredential;
            }
        }

        private BlobServiceClient TestBlobServiceClient 
        { 
            get 
            {
                if (_testBlobServiceClient == null)
                {
                    _testBlobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionStringTemp);
                }

                return _testBlobServiceClient; 
            }
        }

        private BlobServiceClient MainBlobServiceClient
        {
            get
            {
                if (_mainBlobServiceClient == null)
                {
                    _mainBlobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionStringMain);
                }

                return _mainBlobServiceClient;
            }
        }

        private BlobServiceClient ThumbnailBlobServiceClient
        {
            get
            {
                if (_thumbnailBlobServiceClient == null)
                {
                    _thumbnailBlobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionStringThumbnail);
                }

                return _thumbnailBlobServiceClient;
            }
        }
        
        public string GetStorageAccountName(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                    return _blobStorageSettings.AccountNameTemp;
                case ImageVariant.Main:
                    return _blobStorageSettings.AccountNameMain;
                case ImageVariant.SmallThumbnail:
                case ImageVariant.SmallThumbnailWithWatermark:
                case ImageVariant.MediumThumbnail:
                case ImageVariant.MediumThumbnailWithWatermark:
                case ImageVariant.LargeThumbnail:
                case ImageVariant.LargeThumbnailWithWatermark:
                case ImageVariant.Service:
                    return _blobStorageSettings.AccountNameThumbnail;
                default:
                    return _blobStorageSettings.AccountNameTemp;
            }
        }

        public string GetContainerName(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                    return _blobStorageSettings.ContainerNameTemp;
                case ImageVariant.Main:
                    return _blobStorageSettings.ContainerNameMain;
                case ImageVariant.SmallThumbnail:
                case ImageVariant.SmallThumbnailWithWatermark:
                case ImageVariant.MediumThumbnail:
                case ImageVariant.MediumThumbnailWithWatermark:
                case ImageVariant.LargeThumbnail:
                case ImageVariant.LargeThumbnailWithWatermark:
                case ImageVariant.Service:
                    return _blobStorageSettings.ContainerNameThumbnail;
                default:
                    return _blobStorageSettings.ContainerNameTemp;
            }
        }

        public async Task<BlobClient> GetBlobClientAsync(string fileName, ImageVariant imageVariant)
        {
            string bloabContainerName = GetContainerName(imageVariant);

            var containerClient = GetBlobServiceClient(imageVariant)
                .GetBlobContainerClient(bloabContainerName);

            await containerClient.CreateIfNotExistsAsync();

            return containerClient.GetBlobClient(fileName);
        }

        public string GetBlobUri(string fileName, DateTime urlExpireDateTime,
            BlobContainerSasPermissions permissions, ImageVariant imageVariant)
        {
            try
            {
                string bloabContainerName = GetContainerName(imageVariant);

                var container = GetBlobServiceClient(imageVariant)
                        .GetBlobContainerClient(bloabContainerName);

                var credential = GetCredential(imageVariant);

                var sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container.Name,
                    BlobName = fileName,
                    Resource = "b",//Value b is for generating token for a Blob and c is for container
                    StartsOn = DateTime.UtcNow.AddMinutes(-2),
                    ExpiresOn = urlExpireDateTime.ToUniversalTime(),
                };

                sasBuilder.SetPermissions(permissions); //multiple permissions can be added by using | symbol

                UriBuilder sasUri = new UriBuilder($"{container.Uri}/{fileName}");
                sasUri.Query = sasBuilder.ToSasQueryParameters(credential).ToString();

                return sasUri.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UploadFileGetName(Uri uri)
        {
            var credential = GetCredential(ImageVariant.Temp);

            return new BlobClient(uri, credential).Name;
        }

        private BlobServiceClient GetBlobServiceClient(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                    return TestBlobServiceClient;
                case ImageVariant.Main:
                    return MainBlobServiceClient;
                case ImageVariant.SmallThumbnail:
                case ImageVariant.SmallThumbnailWithWatermark:
                case ImageVariant.MediumThumbnail:
                case ImageVariant.MediumThumbnailWithWatermark:
                case ImageVariant.LargeThumbnail:
                case ImageVariant.LargeThumbnailWithWatermark:
                case ImageVariant.Service:
                    return ThumbnailBlobServiceClient;
                default:
                    return TestBlobServiceClient;
            }
        }

        private StorageSharedKeyCredential GetCredential(ImageVariant variant)
        {
            switch (variant)
            {
                case ImageVariant.Temp:
                    return UploadCredential;
                case ImageVariant.Main:
                    return StorageCredential;
                //case ImageVariant.SmallThumbnail:
                //case ImageVariant.SmallThumbnailWithWatermark:
                //case ImageVariant.MediumThumbnail:
                //case ImageVariant.MediumThumbnailWithWatermark:
                //case ImageVariant.LargeThumbnail:
                //case ImageVariant.LargeThumbnailWithWatermark:
                //    return _blobStorageSettings.ContainerNameThumbnail;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
