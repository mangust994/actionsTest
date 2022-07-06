using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Core.Models.Responses;
using HHAzureImageStorage.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace HHAzureImageStorage.BL.Utilities
{
    public class ImageSharpResizer : IImageResizer
    {
        public static ProcessUploadedImageResponse ProcessUploadedImageAndGetData(Stream inputImageStream, int shortestPixelSize, string contentType)
        {
            ProcessUploadedImageResponse dto = new ProcessUploadedImageResponse();

            using (Image image = Image.Load(inputImageStream))
            {
                IImageEncoder encoder = IsContantTypePng(contentType) ? new PngEncoder() : new JpegEncoder();

                image.Mutate(x => x.AutoOrient());

                if (shortestPixelSize != 0 && shortestPixelSize < Math.Min(image.Width, image.Height))
                {
                    double scale = Math.Max(shortestPixelSize / (double)image.Width, shortestPixelSize / (double)image.Height);

                    int width = (int)(image.Width * scale);
                    int height = (int)(image.Height * scale);

                    image.Mutate(x => x.Resize(width, height));
                }

                int shortestPixelSizeForMagicImage = 20;

                double scaleForMagicImage = Math.Max(shortestPixelSizeForMagicImage / (double)image.Width, shortestPixelSizeForMagicImage / (double)image.Height);

                int widthForMagicImage = (int)(image.Width * scaleForMagicImage);
                int heightForMagicImage = (int)(image.Height * scaleForMagicImage);


                Image imageForMagicImage = image.Clone(x => x.Resize(widthForMagicImage, heightForMagicImage));

                dto.HeightPixels = image.Height;
                dto.WidthPixels = image.Width;

                dto.ImageStream = new MemoryStream();
                dto.ImageStreamForMagicImage = new MemoryStream();

                image.Save(dto.ImageStream, encoder);
                imageForMagicImage.Save(dto.ImageStreamForMagicImage, encoder);

                dto.SizeInBytes = dto.ImageStream.Length;

                dto.ImageStream.Seek(0L, SeekOrigin.Begin);
                dto.ImageStreamForMagicImage.Seek(0L, SeekOrigin.Begin);
                inputImageStream.Seek(0L, SeekOrigin.Begin);

                imageForMagicImage.Dispose();
            }

            return dto;
        }

        public ProcessUploadedImageResponse ProcessUploadedImageAndGetData(byte[] inputImageBytes, int shortestPixelSize, string contentType)
        {
            ProcessUploadedImageResponse dto = new ProcessUploadedImageResponse();

            using (Image image = Image.Load(inputImageBytes))
            {
                IImageEncoder encoder = IsContantTypePng(contentType) ? new PngEncoder() : new JpegEncoder();

                image.Mutate(x => x.AutoOrient());

                if (shortestPixelSize != 0 && shortestPixelSize < Math.Min(image.Width, image.Height))
                {
                    double scale = Math.Max(shortestPixelSize / (double)image.Width, shortestPixelSize / (double)image.Height);

                    int width = (int)(image.Width * scale);
                    int height = (int)(image.Height * scale);

                    image.Mutate(x => x.Resize(width, height));
                }

                dto.HeightPixels = image.Height;
                dto.WidthPixels = image.Width;

                dto.ImageStream = new MemoryStream();
                dto.ImageStreamForMagicImage = new MemoryStream();

                image.Save(dto.ImageStream, encoder);

                dto.SizeInBytes = dto.ImageStream.Length;
                dto.ImageStream.Seek(0L, SeekOrigin.Begin);
            }

            return dto;
        }

        public ImageResizeResponse ResizeWithWatermark(byte[] sourceArray, byte[] watermarkArray, int longestPixelSize, WaterMarkType watermarkType, string contentType)
        {
            ImageResizeResponse dto = new ImageResizeResponse();

            using (Image sourceImage = Image.Load(sourceArray))
            using (Image watermarkImage = Image.Load(watermarkArray))
            {
                IImageEncoder encoder = IsContantTypePng(contentType) ? new PngEncoder() : new JpegEncoder();

                if (longestPixelSize == 0)
                {
                    longestPixelSize = Math.Max(sourceImage.Width, sourceImage.Height);
                }

                double scale = Math.Min(longestPixelSize / (double)sourceImage.Width, longestPixelSize / (double)sourceImage.Height);
                int width = (int)(sourceImage.Width * scale);
                int height = (int)(sourceImage.Height * scale);

                sourceImage.Mutate(x => x.Resize(width, height));

                if (watermarkType == WaterMarkType.Single)
                {
                    float scaleForWatermark = 1;
                    int x;
                    int y;
                    if (sourceImage.Width < watermarkImage.Width)
                    {
                        scaleForWatermark = (float)(sourceImage.Width) / (float)(watermarkImage.Width);
                    }
                    else if (sourceImage.Height < watermarkImage.Height)
                    {
                        scaleForWatermark = (float)(sourceImage.Height) / (float)(watermarkImage.Height);
                    }
                    if ((int)scaleForWatermark != 1)
                    {
                        //watermarkImage.Resize((int)(watermarkImage.Width * scaleForWatermark), (int)(watermarkImage.Height * scaleForWatermark));
                        watermarkImage.Mutate(x => x.Resize((int)(watermarkImage.Width * scaleForWatermark), (int)(watermarkImage.Height * scaleForWatermark)));

                        x = (sourceImage.Width - (int)(watermarkImage.Width)) / 2;
                        y = (sourceImage.Height - (int)(watermarkImage.Height)) / 2;
                    }
                    else
                    {
                        x = (sourceImage.Width - (int)(watermarkImage.Width)) / 2;
                        y = (sourceImage.Height - (int)(watermarkImage.Height)) / 2;
                    }

                    var point = new Point()
                    { 
                        X = x,
                        Y = y 
                    };

                    //sourceImage.Composite(watermark, x, y, CompositeOperator.Over);
                    sourceImage.Mutate(x => x.DrawImage(watermarkImage, point, 1));
                }
                else
                {
                    var point = new Point();


                    for (int x = 0; x < sourceImage.Width; x += watermarkImage.Width)
                    {
                        for (int y = 0; y < sourceImage.Height; y += watermarkImage.Height)
                        {
                            point.X = x;
                            point.Y = y;


                            //source.Composite(watermark, x, y, CompositeOperator.Over);
                            sourceImage.Mutate(x => x.DrawImage(watermarkImage, point, 1));
                        }
                    }
                }

                dto.HeightPixels = sourceImage.Height;
                dto.WidthPixels = sourceImage.Width;

                var stream = new MemoryStream();

                sourceImage.Save(stream, encoder);

                dto.SizeInBytes = stream.Length;
                stream.Seek(0L, SeekOrigin.Begin);
                dto.ResizedStream = stream;
            }

            return dto;
        }

        public ImageResizeResponse Resize(byte[] sourceArray, int longestPixelSize, string contentType)
        {
            ImageResizeResponse dto = new ImageResizeResponse();

            using (Image image = Image.Load(sourceArray))
            {
                IImageEncoder encoder = IsContantTypePng(contentType) ? new PngEncoder() : new JpegEncoder();

                if (longestPixelSize == 0)
                {
                    longestPixelSize = Math.Max(image.Width, image.Height);
                }

                double scale = Math.Min(longestPixelSize / (double)image.Width, longestPixelSize / (double)image.Height);
                int width = (int)(image.Width * scale);
                int height = (int)(image.Height * scale);

                image.Mutate(x => x.Resize(width, height));

                dto.HeightPixels = image.Height;
                dto.WidthPixels = image.Width;

                dto.ResizedStream = new MemoryStream();

                image.Save(dto.ResizedStream, encoder);

                dto.SizeInBytes = dto.ResizedStream.Length;
                dto.ResizedStream.Seek(0L, SeekOrigin.Begin);
            }

            return dto;
        }

        private static bool IsContantTypePng(string contentType)
        {
            //("image/jpeg", "image/png", "image/svg+xml");
            switch (contentType)
            {
                case "image/png":
                    return true;
                case "image/jpg":
                    return false;
                default:
                    return false;
            }
        }
    }
}
