using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Core.Models.Responses;
using HHAzureImageStorage.Domain.Enums;
using ImageMagick;
using System;
using System.IO;

namespace HHAzureImageStorage.BL.Utilities
{
    public class ImageResizerMagickImage : IImageResizer
    {
        public ImageResizeResponse Resize(byte[] sourceArray, int longestPixelSize, string contentType)
        {
            using (IMagickImage thumbAsMagickImage = GetThumbAsMagickImage(sourceArray, longestPixelSize))
            {
                var byteArray = thumbAsMagickImage.ToByteArray();

                return new ImageResizeResponse()

                {
                    ResizedStream = new MemoryStream(byteArray),
                    HeightPixels = thumbAsMagickImage.Height,
                    WidthPixels = thumbAsMagickImage.Width,
                    SizeInBytes = byteArray.Length
                };
            }
        }

        public ImageResizeResponse ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType, string contentType)
        {

            using (IMagickImage thumbAsMagickImage = GetThumbAsMagickImage(sourceArray, longestPixelSize))
            using (IMagickImage watermarkAsMagickImage = new MagickImage(watermarkArray))
            {
                var thumbWithWatermark = new MagickImage(drawWatermark(thumbAsMagickImage, watermarkAsMagickImage, watermarkType).ToByteArray());

                var byteArray = thumbWithWatermark.ToByteArray();

                return new ImageResizeResponse()

                {
                    ResizedStream = new MemoryStream(byteArray),
                    HeightPixels = thumbWithWatermark.Height,
                    WidthPixels = thumbWithWatermark.Width,
                    SizeInBytes = byteArray.Length
                };
            }
        }

        public static bool HasImageTransparentAlphaLayer(Stream imageStream)
        {
            using(MagickImage magickImage = new MagickImage(imageStream))
            {
                magickImage.AutoOrient();
                magickImage.RemoveProfile("exif");

                return magickImage.HasAlpha && !magickImage.IsOpaque;
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

        public ProcessUploadedImageResponse ProcessUploadedImageAndGetData(byte[] inputImageBytes, int shortestPixelSize, string contentType)
        {
            throw new NotImplementedException();
        }

        public ProcessUploadedImageResponse ProcessUploadedImageAndGetData(Stream inputImageStream, int shortestPixelSize, string contentType)
        {
            throw new NotImplementedException();
        }
    }
}
