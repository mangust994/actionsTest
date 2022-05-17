using HHAzureImageStorage.Domain.Enums;
using System.IO;

namespace HHAzureImageStorage.Core.Interfaces.Processors
{
    public interface IImageResizer
    {
        //void Resize(Stream input, Stream output, int width, int height);

        //void ResizeToLargeThumbnail(Stream input, Stream output);

        public Stream Resize(byte[] sourceArray, int longestPixelSize);

        public Stream ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType);
    }
}
