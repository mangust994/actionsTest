using System.Collections.Immutable;
using System.IO;

namespace HHAzureImageStorage.BL.Validators
{
    public class UploadFileValidator : IUploadFileValidator
    {
        static readonly ImmutableArray<string> allowedContentTypes = ImmutableArray
            .Create("image/jpeg", "image/png", "image/svg+xml");

        public bool IsValidContentType(string contentType)
        {
            return allowedContentTypes.Contains(contentType);
        }

        public bool IsFileNotEmpty(Stream file)
        {
            if (file == null || file.Length == 0L)
                return false;

            return true;
        }
    }
}
