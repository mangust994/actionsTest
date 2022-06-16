using HHAzureImageStorage.BL.Validators;
using HHAzureImageStorage.FunctionApp.Helpers;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class UploadFileHelperTests
    {
        readonly IUploadFileHelper _uploadFileHelper;

        public UploadFileHelperTests()
        {
            _uploadFileHelper = new UploadFileHelper(new UploadFileValidator());
        }

        [Fact]
        public void BRACKETS_Sanitize_True()
        {
            string badFileName = "<Name>";

            string sanitizedFileName = _uploadFileHelper.Sanitize(badFileName);

            Assert.Equal("Name", sanitizedFileName);
        }

        [Fact]
        public void JPEG_GetValidPhotoName_True()
        {
            string badFileName = "Test&Name.jpeg";

            string sanitizedFileName = _uploadFileHelper.GetValidPhotoName(badFileName);

            Assert.Equal("Test-Name.jpg", sanitizedFileName);
        }
    }
}
