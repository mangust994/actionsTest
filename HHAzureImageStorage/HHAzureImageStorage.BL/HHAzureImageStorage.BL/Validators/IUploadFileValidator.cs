using System.IO;

namespace HHAzureImageStorage.BL.Validators
{
    public interface IUploadFileValidator
    {
        bool IsFileNotEmpty(Stream file);

        bool IsValidContentType(string contentType);
    }
}