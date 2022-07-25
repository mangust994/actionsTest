using HHAzureImageStorage.Core.Models.Responses;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Core.Interfaces.Processors
{
    public interface IImageResizer
    {
        ProcessUploadedImageResponse ProcessUploadedImageAndGetData(byte[] inputImageBytes, int shortestPixelSize, string contentType);
        ImageResizeResponse Resize(byte[] sourceArray, int longestPixelSize, string contentType);

        ImageResizeResponse ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType, string contentType);
    }
}
