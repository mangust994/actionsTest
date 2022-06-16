using Azure.Messaging.ServiceBus;
using HHAzureImageStorage.BL.Models.DTOs;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HHAzureImageStorage.BL.Services
{
    public class AzurServiceBusQueueMessageService : IQueueMessageService
    {
        private readonly ServiceBusClient client;

        public AzurServiceBusQueueMessageService()
        {
            var connString = "Endpoint=sb://imagestorage.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=o5SDtrjizpSTsYkL47GxALbE2SHjvsQ0r+PdcEdHknI=";//Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_CON_STR");
            client = new ServiceBusClient(connString);
        }

        public async Task SendMessageProcessThumbnailImagesAsync(GenerateThumbnailImagesDto data)
        {
            var senderName = Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_PROCESS_THUMB_IMAGES_NAME");

            await this.SendMessageAsync(data, senderName);
        }

        public async Task SendMessageRemoveImageAsync(Guid imageId)
        {
            var senderName = Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_DELETE_IMAGE_NAME");

            await this.SendMessageAsync(imageId, senderName);
        }

        public async Task SendMessageDeleteImagesByEventAsync(int eventKey)
        {
            var senderName = Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_DELETE_IMAGE_BY_EVENT_NAME");

            await this.SendMessageAsync(eventKey, senderName);
        }

        public async Task SendMessageDeleteImagesByPhotographerAsync(int photographerKey)
        {
            var senderName = Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_DELETE_IMAGE_BY_PHOTOGRAPHER_NAME");

            await this.SendMessageAsync(photographerKey, senderName);
        }

        public async Task SendMessageRebuildThumbnailsWithWatermarkAsync(RebuildThumbnailsWithWatermarkDto modelDto)
        {
            var senderName = Environment.GetEnvironmentVariable("SERVER_BUS_QUEUE_REBUILD_THUMB_NAME");

            await this.SendMessageAsync(modelDto, senderName);
        }

        public async Task SendMessageAsync<T>(T data, string senderName)
        {
            var sender = client.CreateSender(senderName);

            var body = JsonSerializer.Serialize(data);
            var message = new ServiceBusMessage(body);

            await sender.SendMessageAsync(message);
        }
    }
}
