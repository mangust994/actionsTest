using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.Core.Interfaces.Processors;

namespace HHAzureImageStorage.BlobStorageProcessor
{
    public class AzureBlobStorageProcessor : IStorageProcessor
    {       
        readonly IStorageHelper _storageHelper;
        readonly ILogger _logger;

        public AzureBlobStorageProcessor(IStorageHelper storageHelper,
            ILoggerFactory loggerFactory
            )
        {
            _storageHelper = storageHelper;
            _logger = loggerFactory.CreateLogger<AzureBlobStorageProcessor>();
        }        

        public async Task<bool> StorageFileUploadAsync(Stream content,
          string contentType,
          ImageVariant imageVariant,
          string fileName,
          IDictionary<string, string> metadata = null)
        {
            try
            {
                BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                await blobClient.UploadAsync(content, blobHttpHeaders);

                if (metadata != null)
                {
                    await blobClient.SetMetadataAsync(metadata);
                }                

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileUploadAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }
        }

        public async Task<string> StorageFileGetReadAccessUrl(ImageVariant imageVariant, string fileName, DateTimeOffset urlExpireDateTime)
        {
            try
            {
                BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                Uri sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, urlExpireDateTime);

                return sasUri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileGetReadAccessUrl: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            return string.Empty;
        }

        public async Task<IDictionary<string, string>> StorageFileGetMetadataAsync(string fileName, ImageVariant imageVariant)
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>();

            try
            {
                BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                BlobProperties properties = await blobClient.GetPropertiesAsync();

                metadata = properties.Metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileGetMetadataAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            return metadata;
        }

        public async Task<bool> StorageFileDeleteAsync(string fileName, ImageVariant imageVariant)
        {
            try
            {
                BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                var result = await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);

                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileDeleteAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            return false;
        }

        public string UploadFileGetCreateAccessUrl(string fileName, DateTime urlExpireDateTime)
        {
            var premissions = BlobSasPermissions.Write | BlobSasPermissions.Create;

            return _storageHelper.GetBlobUri(fileName, urlExpireDateTime, premissions, ImageVariant.Temp);
        }

        public async Task<Stream> StorageFileGetAsync(string fileName, ImageVariant imageVariant)
        {
            try
            {
                BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                var result = await blobClient.DownloadContentAsync();

                return result.Value.Content.ToStream();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileGetAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            return null;
        }

        public async Task<byte[]> StorageFileBytesGetAsync(string fileName, ImageVariant imageVariant)
        {
            try
            {
                using (var sourceImage = new MemoryStream())
                {
                    BlobClient blobClient = await _storageHelper.GetBlobClientAsync(fileName, imageVariant);

                    if (blobClient == null)
                    {
                        return null;
                    }

                    var result = await blobClient.DownloadContentAsync();

                    //return result.Value.Content.ToArray();

                    await blobClient.DownloadToAsync(sourceImage);

                    return sourceImage.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AzureBlobStorageProcessor|StorageFileBytesGetAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            return null;
        }

        public string UploadFileGetName(Uri uri)
        {
            return _storageHelper.UploadFileGetName(uri);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
