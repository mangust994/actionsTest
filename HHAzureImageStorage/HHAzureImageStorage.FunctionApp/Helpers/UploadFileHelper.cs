using HHAzureImageStorage.BL.Extensions;
using HHAzureImageStorage.BL.Validators;
using HttpMultipartParser;
using System.IO;

namespace HHAzureImageStorage.FunctionApp.Helpers
{
    public class UploadFileHelper : IUploadFileHelper
    {
        private const string PHOTO_REQUIRED = "Photo is required.";
        private const string SELECT_VALID_FILE = "Please, select a valid file!";

        private readonly IUploadFileValidator _uploadFileValidator;

        public UploadFileHelper(IUploadFileValidator uploadFileValidator)
        {
            _uploadFileValidator = uploadFileValidator;
        }

        public string ValidateFile(MultipartFormDataParser formData)
        {
            if (formData.Files.Count == 0)
            {
                return PHOTO_REQUIRED;
            }

            var file = formData.Files[0];

            if (IsFileNotValid(file))
            {
                return SELECT_VALID_FILE;
            }

            return string.Empty;
        }

        public string Sanitize(string fileName)
        {
            foreach (var character in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(character.ToString(), "");
            }

            return fileName;
        }
        
        public string GetValidPhotoName(string photoName)
        {
            if (string.IsNullOrWhiteSpace(photoName))
            {
                return string.Empty;
            }

            var name = photoName.Trim();

            if (IsExtension("jpeg", photoName))
            {
                name = ChangeExtension(photoName, ".jpg");
            }

            name = name.Replace("&", "-");

            return name;
        }

        private static bool IsExtension(string ex, string path)
        {
            var left = ex.Default();
            var right = Path.GetExtension(path).Default();
            if (!left.StartsWith("."))
            {
                left = "." + left;
            }
            return left.Is(right, true);
        }

        private static string ChangeExtension(string path, string ex)
        {
            return Path.ChangeExtension(path, ex);
        }

        private bool IsFileNotValid(FilePart file)
        {
            return !_uploadFileValidator.IsFileNotEmpty(file.Data);
        }
    }
}
