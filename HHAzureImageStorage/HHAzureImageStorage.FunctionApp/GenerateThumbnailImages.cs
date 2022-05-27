using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HHAzureImageStorageFunctionApp
{
    public class GenerateThumbnailImages
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public GenerateThumbnailImages(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<GenerateThumbnailImages>();
            _uploadImageService = uploadImageService;
        }


        [Function("GenerateThumbnailImages")]
        [OpenApiOperation(operationId: "GenerateThumbnailImages", tags: new[] { "image" })]

        public async Task Run([QueueTrigger("testqueue")] string myQueueItem, FunctionContext context)
        {
            _logger.LogInformation($"GenerateThumbnailImages: Started processing: {myQueueItem}");

            GenerateThumbnailImagesDto generateThumbnailImagesDto = JsonSerializer
                .Deserialize<GenerateThumbnailImagesDto>(myQueueItem);

            if (generateThumbnailImagesDto.ImageId == Guid.Empty)
            {
                _logger.LogInformation("GenerateThumbnailImages: The imageId is empty");

                return;
            }

            try
            {
                if (generateThumbnailImagesDto.IsRebuildThumbnails)
                {
                    _logger.LogInformation($"GenerateThumbnailImages: Started RebuildWatermarkThumbnailsProcess for {generateThumbnailImagesDto.ImageId} ImageId");

                    await _uploadImageService.RebuildWatermarkThumbnailsProcess(generateThumbnailImagesDto);

                    _logger.LogInformation($"GenerateThumbnailImages: Finished RebuildWatermarkThumbnailsProcess for {generateThumbnailImagesDto.ImageId} ImageId");
                }
                else
                {
                    _logger.LogInformation($"GenerateThumbnailImages: Started ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} ImageId");

                    await _uploadImageService.ThumbnailImagesProcess(generateThumbnailImagesDto);

                    _logger.LogInformation($"GenerateThumbnailImages: Finished ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} ImageId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GenerateThumbnailImages: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation("GenerateThumbnailImages: Finished");
        }
    }
}
