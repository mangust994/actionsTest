using System;
using System.Text.Json;
using System.Threading.Tasks;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class RebuildThumbnailsProcessor
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public RebuildThumbnailsProcessor(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<RebuildThumbnailsProcessor>();
            _uploadImageService = uploadImageService;
        }

        [Function("RebuildThumbnailsProcessor")]
        public async Task Run([ServiceBusTrigger("rebuild-thumb", Connection = "SERVER_BUS_QUEUE_CON_STR")] string myQueueItem)
        {
            _logger.LogInformation($"RebuildThumbnailsProcessor function processed message: {myQueueItem}");

            try
            {
                RebuildThumbnailsWithWatermarkDto modelDto = JsonSerializer.Deserialize<RebuildThumbnailsWithWatermarkDto>(myQueueItem);

                await _uploadImageService.RebuildThumbnailsWithWatermarkAsync(modelDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RebuildThumbnailsProcessor: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation("RebuildThumbnailsProcessor: Finished");
        }
    }
}
