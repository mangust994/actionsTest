using HHAzureImageStorage.BL.Validators;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class UploadFileValidatorTests
    {

        readonly IUploadFileValidator _uploadFileValidator;

        public UploadFileValidatorTests()
        {
            _uploadFileValidator = new UploadFileValidator();
        }

        [Fact]
        public void JPEG_IsValidContentType_True()
        {
            string contentType = "image/jpeg";
            
            bool isValid = _uploadFileValidator.IsValidContentType(contentType);

            Assert.True(isValid);
        }

        [Fact]
        public void SVG_IsValidContentType_True()
        {
            string contentType = "image/svg+xml";

            bool isValid = _uploadFileValidator.IsValidContentType(contentType);

            Assert.True(isValid);
        }

        [Fact]
        public void PNG_IsValidContentType_True()
        {
            string contentType = "image/png";

            bool isValid = _uploadFileValidator.IsValidContentType(contentType);

            Assert.True(isValid);
        }

        [Fact]
        public void JSON_IsValidContentType_False()
        {
            string contentType = "application/json";

            bool isValid = _uploadFileValidator.IsValidContentType(contentType);

            Assert.False(isValid);
        }

        [Fact]
        public void NULL_IsFileNotEmpty_False()
        {
            bool isNotEmpty = _uploadFileValidator.IsFileNotEmpty(null);

            Assert.False(isNotEmpty);
        }
    }
}
