using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Domain.Enums;
using ImageMagick;
using System;
using System.IO;

namespace HHAzureImageStorage.BL.Utilities
{
    public class ImageResizerMagickImage : IImageResizer
    {
        public Stream Resize(byte[] sourceArray, int longestPixelSize)
        {
            using (IMagickImage thumbAsMagickImage = GetThumbAsMagickImage(sourceArray, longestPixelSize))
            {
                return new MemoryStream(thumbAsMagickImage.ToByteArray());
            }
        }

        public Stream ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType)
        {

            using (IMagickImage thumbAsMagickImage = GetThumbAsMagickImage(sourceArray, longestPixelSize))
            using (IMagickImage watermarkAsMagickImage = new MagickImage(watermarkArray))
            {
                var thumbWithWatermark = new MagickImage(drawWatermark(thumbAsMagickImage, watermarkAsMagickImage, watermarkType).ToByteArray());
                
                return new MemoryStream(thumbWithWatermark.ToByteArray());
            }
        }

        private IMagickImage GetThumbAsMagickImage(byte[] sourceArray, int longestPixelSize)
        {
            IMagickImage originalAsMagickImage = new MagickImage(sourceArray);


            if (longestPixelSize == 0)
            {
                longestPixelSize = Math.Max(originalAsMagickImage.Width, originalAsMagickImage.Height);
            }

            double scale = Math.Min(longestPixelSize / (double)originalAsMagickImage.Width, longestPixelSize / (double)originalAsMagickImage.Height);
            int w = (int)(originalAsMagickImage.Width * scale);
            int h = (int)(originalAsMagickImage.Height * scale);

            originalAsMagickImage.Resize(w, h);

            return originalAsMagickImage;
        }

        private IMagickImage drawWatermark(IMagickImage source, IMagickImage watermark, WaterMarkType watermarkType)
        {
            if (source == null || watermark == null)
            {
                return null;
            }

            if (watermarkType == WaterMarkType.Single)
            {
                float scale = 1;
                int x;
                int y;
                if (source.Width < watermark.Width)
                {
                    scale = (float)(source.Width) / (float)(watermark.Width);
                }
                else if (source.Height < watermark.Height)
                {
                    scale = (float)(source.Height) / (float)(watermark.Height);
                }
                if ((int)scale != 1)
                {
                    watermark.Resize((int)(watermark.Width * scale), (int)(watermark.Height * scale));
                    x = (source.Width - (int)(watermark.Width)) / 2;
                    y = (source.Height - (int)(watermark.Height)) / 2;
                }
                else
                {
                    x = (source.Width - (int)(watermark.Width)) / 2;
                    y = (source.Height - (int)(watermark.Height)) / 2;
                }
                source.Composite(watermark, x, y, CompositeOperator.Over);
            }
            else
            {
                for (int x = 0; x < source.Width; x += watermark.Width)
                {
                    for (int y = 0; y < source.Height; y += watermark.Height)
                    {
                        source.Composite(watermark, x, y, CompositeOperator.Over);
                    }
                }
            }
            return source;
        }
    }
}
