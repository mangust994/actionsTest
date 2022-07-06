using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class GenerateThumbnailImagesProcessor
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public GenerateThumbnailImagesProcessor(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<GenerateThumbnailImagesProcessor>();
            _uploadImageService = uploadImageService;
        }


        [Function("GenerateThumbnailImagesProcessor")]
        public async Task Run([ServiceBusTrigger("process-thumb-images", Connection = "SERVER_BUS_QUEUE_CON_STR")] string myQueueItem)
        {
            _logger.LogInformation($"GenerateThumbnailImagesProcessor: Started processing: {myQueueItem}");

            GenerateThumbnailImagesDto generateThumbnailImagesDto = JsonSerializer
                .Deserialize<GenerateThumbnailImagesDto>(myQueueItem);

            try
            {
                if (generateThumbnailImagesDto.IsRebuildThumbnails)
                {
                    _logger.LogInformation($"GenerateThumbnailImagesProcessor: Started RebuildWatermarkThumbnailsProcess for {generateThumbnailImagesDto.ImageId} ImageId");

                    await _uploadImageService.RebuildWatermarkThumbnailsProcess(generateThumbnailImagesDto);

                    _logger.LogInformation($"GenerateThumbnailImagesProcessor: Finished RebuildWatermarkThumbnailsProcess for {generateThumbnailImagesDto.ImageId} ImageId");
                }
                else
                {
                    _logger.LogInformation($"GenerateThumbnailImagesProcessor: Started ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} ImageId");

                    await _uploadImageService.ThumbnailImagesProcess(generateThumbnailImagesDto);

                    _logger.LogInformation($"GenerateThumbnailImagesProcessor: Finished ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} ImageId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GenerateThumbnailImagesProcessor: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation("GenerateThumbnailImagesProcessor: Finished");
        }
    }
}
