using HHAzureImageStorage.Core.Models.Responses;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Core.Interfaces.Processors
{
    public interface IImageResizer
    {
        public ImageResizeResponse Resize(byte[] sourceArray, int longestPixelSize);

        public ImageResizeResponse ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType);
    }
}
