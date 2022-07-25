using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class ImageVariantHelperTests
    {
        [Fact]
        public void IsWithWatermark_ImageVariant_SmallThumbnailWithWatermark_True()
        {
            ImageVariant imageVariant = ImageVariant.SmallThumbnailWithWatermark;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.True(isWithWatermarkType);
        }

        [Fact]
        public void IsWithWatermark_ImageVariant_MediumThumbnailWithWatermark_True()
        {
            ImageVariant imageVariant = ImageVariant.MediumThumbnailWithWatermark;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.True(isWithWatermarkType);
        }

        [Fact]
        public void IsWithWatermark_ImageVariant_LargeThumbnailWithWatermark_True()
        {
            ImageVariant imageVariant = ImageVariant.LargeThumbnailWithWatermark;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.True(isWithWatermarkType);
        }

        [Fact]
        public void IsWithWatermark_ImageVariant_SmallThumbnail_False()
        {
            ImageVariant imageVariant = ImageVariant.SmallThumbnail;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.False(isWithWatermarkType);
        }

        [Fact]
        public void IsWithWatermark_ImageVariant_MediumThumbnail_False()
        {
            ImageVariant imageVariant = ImageVariant.MediumThumbnail;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.False(isWithWatermarkType);
        }

        [Fact]
        public void IsWithWatermark_ImageVariant_LargeThumbnail_False()
        {
            ImageVariant imageVariant = ImageVariant.LargeThumbnail;

            bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

            Assert.False(isWithWatermarkType);
        }
    }
}
