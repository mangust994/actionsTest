using HttpMultipartParser;

namespace HHAzureImageStorage.FunctionApp.Helpers
{
    public interface IUploadFileHelper
    {
        string GetValidPhotoName(string photoName);
        string Sanitize(string fileName);
        string ValidateFile(MultipartFormDataParser formData);
    }
}