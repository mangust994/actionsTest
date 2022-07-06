using System;
using System.Text.Json;
using System.Threading.Tasks;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class DeleteImageProcessor
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public DeleteImageProcessor(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImageProcessor>();
            _uploadImageService = uploadImageService;
        }

        [Function("DeleteImageProcessor")]
        public async Task Run([ServiceBusTrigger("delete-image", Connection = "SERVER_BUS_QUEUE_CON_STR")] string myQueueItem)
        {
            _logger.LogInformation($"DeleteImageProcessor function processed message: {myQueueItem}");

            try
            {
                var imageId = JsonSerializer.Deserialize<Guid>(myQueueItem);

                await _uploadImageService.RemoveImageByIdAsync(imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteImageProcessor: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation("DeleteImageProcessor: Finished");
        }
    }
}
