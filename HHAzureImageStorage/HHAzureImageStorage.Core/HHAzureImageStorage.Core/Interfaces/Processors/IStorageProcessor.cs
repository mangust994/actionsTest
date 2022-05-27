using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Core.Interfaces.Processors
{
    public interface IStorageProcessor
    {
        #region STORAGE CONTAINER
        Task<bool> StorageFileUploadAsync(Stream content,
          string contentType,
          ImageVariant imageVariant,
          string fileName,
          IDictionary<string, string> metadata);

        public Task<string> StorageFileGetReadAccessUrl(ImageVariant imageVariant, string fileName, DateTimeOffset urlExpireDateTime);

        Task<IDictionary<string, string>> StorageFileGetMetadataAsync(string fileName, ImageVariant imageVariant);

        public Task<bool> StorageFileDeleteAsync(string fileName, ImageVariant imageVariant);

        public Task<Stream> StorageFileGetAsync(string fileName, ImageVariant imageVariant);

        public Task<byte[]> StorageFileBytesGetAsync(string fileName, ImageVariant imageVariant);

        #endregion STORAGE CONTAINER

        #region UPLOAD CONTAINER
        public string UploadFileGetCreateAccessUrl(string fileName, DateTime urlExpireDateTime);
        string UploadFileGetName(Uri uri);

        //public string UploadFileGetName(Uri uri);

        //public bool UploadFileDelete(Uri uri);

        //public byte[] UploadFileGetBytes(Uri uri);
        #endregion UPLOAD CONTAINER
    }
}
