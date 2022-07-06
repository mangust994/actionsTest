using System;
using System.Text.Json;
using System.Threading.Tasks;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class DeleteImagesByPhotographerProcessor
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public DeleteImagesByPhotographerProcessor(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImagesByPhotographerProcessor>();
            _uploadImageService = uploadImageService;
        }

        [Function("DeleteImagesByPhotographerProcessor")]
        public async Task Run([ServiceBusTrigger("delete-images-by-photographer", Connection = "SERVER_BUS_QUEUE_CON_STR")] string myQueueItem)
        {
            _logger.LogInformation($"DeleteImagesByEventProcessor function processed message: {myQueueItem}");

            try
            {
                var photographerKey = JsonSerializer.Deserialize<int>(myQueueItem);

                await _uploadImageService.RemoveImagesByStudioKeyAsync(photographerKey);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteImagesByEventProcessor: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation("DeleteImagesByEventProcessor: Finished");
        }
    }
}
