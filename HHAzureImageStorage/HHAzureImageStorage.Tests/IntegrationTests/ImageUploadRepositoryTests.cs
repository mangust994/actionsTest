using HHAzureImageStorage.CosmosRepository.Containers;
using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Repositories;
using HHAzureImageStorage.CosmosRepository.Settings;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Tests.Extensions;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageUploadRepositoryTests : IDisposable
    {
        private readonly ImageUpload _testItem;
        private readonly IImageUploadRepository _repository;

        const string ChangedName = "ChangedTestName.png";
        const string TestName = "TestName.jpg";
        const string TestGuid = "f8a41011-9559-486f-a813-29b7d9a94290";

        public ImageUploadRepositoryTests()
        {
            CosmosSettings cosmosSettings = CosmosSettingsExtension.GetTestCosmosSettings();

            CosmosClient cosmosClient = new CosmosClient(cosmosSettings.EndPoint,
                cosmosSettings.Key);

            IImageUploadCosmosContext imageCosmosContext = new ImageUploadCosmosContext(
                cosmosSettings, cosmosClient);

            _repository = new ImageUploadCosmosRepository(imageCosmosContext);

            _testItem = GetTestItem();
        }

        private ImageUpload GetTestItem()
        {
            return new ImageUpload()
            {
                id = new Guid(TestGuid),
                WatermarkImageId = TestGuid,
                WatermarkMethod = "WatermarkMethod",
                FileName = "FileName",
                AlbumKey = 1,
                AutoThumbnails = true,
                HiResDownload = true,
                ColorCorrectLevel = true,
                OriginalImageName = TestName,
                HasTransparentAlphaLayer = true,
                hhihEventKey = 777,
                hhihPhotographerKey = 777
            };
        }

        [Fact]
        public async Task AddAsync_TestImage_ItemIsAdded()
        {
            var item = await _repository.GetByIdAsnc(_testItem.id);

            if (item != null)
            {
                return;
            }

            var response = await _repository.AddAsync(_testItem);

            Assert.NotNull(response);
            Assert.Equal(_testItem.AutoThumbnails, response.AutoThumbnails);
            Assert.Equal(_testItem.id, response.id);
            Assert.Equal(_testItem.WatermarkImageId, response.WatermarkImageId);
            Assert.Equal(_testItem.WatermarkMethod, response.WatermarkMethod);
            Assert.Equal(_testItem.FileName, response.FileName);
            Assert.Equal(_testItem.AlbumKey, response.AlbumKey);
            Assert.Equal(_testItem.HiResDownload, response.HiResDownload);
            Assert.Equal(_testItem.ColorCorrectLevel, response.ColorCorrectLevel);
            Assert.Equal(_testItem.OriginalImageName, response.OriginalImageName);
            Assert.Equal(_testItem.HasTransparentAlphaLayer, response.HasTransparentAlphaLayer);
            Assert.Equal(_testItem.hhihEventKey, response.hhihEventKey);
            Assert.Equal(_testItem.hhihPhotographerKey, response.hhihPhotographerKey);
        }

        [Fact]
        public async Task AddAsync_Null_ItemIsNotAdded()
        {
            var response = await _repository.AddAsync(null);

            Assert.Null(response);
        }

        [Fact]
        public async Task GetByIdAsnc_TestImageId_ImageIsCorrect()
        {
            var response = await _repository.GetByIdAsnc(_testItem.id);

            if (response == null)
            {
                await _repository.AddAsync(_testItem);

                response = await _repository.GetByIdAsnc(_testItem.id);
            }

            Assert.NotNull(response);
            Assert.Equal(_testItem.AutoThumbnails, response.AutoThumbnails);
            Assert.Equal(_testItem.id, response.id);
            Assert.Equal(_testItem.WatermarkImageId, response.WatermarkImageId);
            Assert.Equal(_testItem.WatermarkMethod, response.WatermarkMethod);
            Assert.Equal(_testItem.FileName, response.FileName);
            Assert.Equal(_testItem.AlbumKey, response.AlbumKey);
            Assert.Equal(_testItem.HiResDownload, response.HiResDownload);
            Assert.Equal(_testItem.ColorCorrectLevel, response.ColorCorrectLevel);
            Assert.Equal(_testItem.OriginalImageName, response.OriginalImageName);
            Assert.Equal(_testItem.HasTransparentAlphaLayer, response.HasTransparentAlphaLayer);
            Assert.Equal(_testItem.hhihEventKey, response.hhihEventKey);
            Assert.Equal(_testItem.hhihPhotographerKey, response.hhihPhotographerKey);
        }

        [Fact]
        public async Task UpdateAsync_TestItemId_ItemIsValid()
        {
            var item = await _repository.GetByIdAsnc(_testItem.id);

            if (item == null)
            {
                await _repository.AddAsync(_testItem);

                item = await _repository.GetByIdAsnc(_testItem.id);
            }

            Assert.NotNull(item);

            item.OriginalImageName = ChangedName;

            var response = await _repository.UpdateAsync(item);

            Assert.NotNull(response);
            Assert.Equal(ChangedName, response.OriginalImageName);

            response.OriginalImageName = TestName;

            response = await _repository.UpdateAsync(response);

            Assert.NotNull(response);
            Assert.Equal(TestName, response.OriginalImageName);
        }

        public void Dispose()
        {
            var image = _repository.RemoveAsync(_testItem.id).Result;

            Assert.Null(image);
        }
    }
}
