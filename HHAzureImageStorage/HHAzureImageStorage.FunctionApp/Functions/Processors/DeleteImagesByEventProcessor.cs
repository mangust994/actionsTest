using System;
using System.Text.Json;
using System.Threading.Tasks;
using HHAzureImageStorage.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class DeleteImagesByEventProcessor
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;

        public DeleteImagesByEventProcessor(ILoggerFactory loggerFactory, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImagesByEventProcessor>();
        }

        [Function("DeleteImagesByEventProcessor")]
        public async Task Run([ServiceBusTrigger("delete-images-by-event", Connection = "SERVER_BUS_QUEUE_CON_STR")] string myQueueItem)
        {
            _logger.LogInformation($"DeleteImagesByEventProcessor function processed message: {myQueueItem}");

            try
            {
                var eventKey = JsonSerializer.Deserialize<int>(myQueueItem);

                await _uploadImageService.RemoveImagesByEventKeyAsync(eventKey);
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
