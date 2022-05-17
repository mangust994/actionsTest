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
        private readonly IUploadImageService _uploadImageService;

        public GenerateThumbnailImages(IUploadImageService uploadImageService)
        {
            _uploadImageService = uploadImageService;
        }


        [Function("GenerateThumbnailImages")]
        [OpenApiOperation(operationId: "GenerateThumbnailImages", tags: new[] { "image" })]

        public async Task Run([QueueTrigger("testqueue")] string myQueueItem, FunctionContext context)
        {
            var logger = context.GetLogger("GenerateThumbnailImages");

            logger.LogInformation("GenerateThumbnailImages: Started");
            logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            GenerateThumbnailImagesDto generateThumbnailImagesDto = JsonSerializer
                .Deserialize<GenerateThumbnailImagesDto>(myQueueItem);

            if (generateThumbnailImagesDto.ImageId == Guid.Empty)
            {
                logger.LogInformation("GenerateThumbnailImages: The imageId is empty");

                return;
            }

            try
            {
                await _uploadImageService.ThumbnailImagesProcess(generateThumbnailImagesDto);
            }
            catch (Exception ex)
            {
                logger.LogError($"GenerateThumbnailImages: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            logger.LogInformation("GenerateThumbnailImages: Finished");
        }
    }
}
