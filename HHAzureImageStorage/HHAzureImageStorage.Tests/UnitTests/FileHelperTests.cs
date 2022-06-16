using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class FileHelperTests
    {
        [Fact]
        public void GetFileNamePrefix_ImageVariant_Temp_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.Temp;            

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(string.Empty, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_Main_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.Main;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(string.Empty, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_Service_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.Service;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.ServicePrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_SmallThumbnail_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.SmallThumbnail;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.SmallThumbnailPrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_SmallThumbnailWithWatermark_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.SmallThumbnailWithWatermark;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.SmallThumbnailWithWatermarkPrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_MediumThumbnail_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.MediumThumbnail;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.MediumThumbnailPrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_MediumThumbnailWithWatermark_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.MediumThumbnailWithWatermark;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.MediumThumbnailWithWatermarkPrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_LargeThumbnail_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.LargeThumbnail;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.LargeThumbnailPrefix, filePrefix);
        }

        [Fact]
        public void GetFileNamePrefix_ImageVariant_LargeThumbnailWithWatermark_IsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.LargeThumbnailWithWatermark;

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);

            Assert.Equal(FileHelper.LargeThumbnailWithWatermarkPrefix, filePrefix);
        }

        [Fact]
        public void GetFileName_FileNameIsCorrect()
        {
            ImageVariant imageVariant = ImageVariant.LargeThumbnailWithWatermark;            
            string imageId = "TestImageName";
            string originalFileName = "TestFileName.jpg";

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
            string fileName = FileHelper.GetFileName(imageId, filePrefix, originalFileName);

            Assert.Equal($"{filePrefix}_{imageId}.jpg", fileName);
        }
    }
}
