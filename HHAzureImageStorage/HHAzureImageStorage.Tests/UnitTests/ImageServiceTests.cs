using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH.Models;
using HHAzureImageStorage.Tests.Extensions;
using HHAzureImageStorage.Tests.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class ImageServiceTests : IDisposable
    {
        private const string TestAccessUrl = "TestAccessUrl";
        private readonly ImageService _sut;

        private readonly Guid ValidImageId;
        private Guid InvalidImageId;
        private Guid ServiceImageId;
        private Guid MainImageId;
        private Guid UpdatedImageId;

        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly Mock<BlobStorageSettings> _storageSettingsMock;
        private readonly Mock<IStorageHelper> _storageHelperMock;
        private readonly Mock<IStorageProcessor> _storageProcessorMock;
        private readonly Mock<IQueueMessageService> _queueMessageServiceMock;
        private readonly Mock<IImageResizer> _imageResizerMock;

        private readonly IStorageHelper _storageHelper;
        private readonly IImageRepository _imageRepository;
        private readonly IImageStorageRepository _imageStorageRepository;
        private readonly IImageApplicationRetentionRepository _imageApplicationRetentionRepository;
        private readonly IImageStorageSizeRepositoty _imageStorageSizeRepository;
        private readonly IImageStorageAccessUrlRepository _imageStorageAccessUrlRepository;
        private readonly IImageUploadRepository _imageUploadRepository;
        private readonly IProcessThumbTrysCountRepository _processThumbTrysCountRepositor;

        public ImageServiceTests()
        {
            _logerMock = new Mock<ILogger<ImageService>>();

            _logerMock.Setup(x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            _logerMock.Setup(m => m.IsEnabled(LogLevel.Information))
                .Returns(true);

            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(() => _logerMock.Object);

            _storageSettingsMock = new Mock<BlobStorageSettings>();
            _storageHelperMock = new Mock<IStorageHelper>();
            _storageProcessorMock = new Mock<IStorageProcessor>();
            _queueMessageServiceMock = new Mock<IQueueMessageService>();
            _imageResizerMock = new Mock<IImageResizer>();

            var resizeResponse = RepositoryDtoTestData.GetTestImageResizeResponseInstance();

            _imageResizerMock.Setup(x => x.Resize(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(() => resizeResponse);

            _imageResizerMock.Setup(x => x.ResizeWithWatermark(It.IsAny<byte[]>(), It.IsAny<byte[]>(),
                It.IsAny<int>(), It.IsAny<WaterMarkType>(), It.IsAny<string>()))
                .Returns(() => resizeResponse);

            var testImageByteArray = RepositoryDtoTestData.GetEmptyImageByteArray();

            _storageProcessorMock.Setup(x => x.StorageFileBytesGetAsync(It.IsAny<string>(), It.IsAny<ImageVariant>()))
                .Returns(() => testImageByteArray);

            _storageProcessorMock.Setup(x => x.UploadFileGetCreateAccessUrl(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(() => TestAccessUrl);

            _storageProcessorMock.Setup(x => x.StorageFileGetReadAccessUrl(ImageVariant.Main, It.IsAny<string>(),
                It.IsAny<DateTimeOffset>()))
                .Returns(() => Task.FromResult(TestAccessUrl));

            _storageProcessorMock.Setup(x => x.StorageFileGetReadAccessUrl(ImageVariant.SmallThumbnail, It.IsAny<string>(),
                It.IsAny<DateTimeOffset>()))
                .Returns(() => Task.FromResult(string.Empty));

            _imageRepository = new InMemoryImageRepository();
            _imageStorageRepository = new InMemoryImageStorageRepository();
            _imageStorageAccessUrlRepository = new InMemoryImageStorageAccessUrlRepository();
            _imageApplicationRetentionRepository = new InMemoryImageApplicationRetentionRepository();
            _imageStorageSizeRepository = new InMemoryImageStorageSizeRepositoty();
            _imageUploadRepository = new InMemoryImageUploadRepository();
            _processThumbTrysCountRepositor = new InMemoryProcessThumbTrysCountRepository();

            _sut = new ImageService(_mockLoggerFactory.Object, _storageSettingsMock.Object, _imageRepository,
                _imageStorageRepository, _imageApplicationRetentionRepository,
                _storageHelperMock.Object, _imageResizerMock.Object, _imageStorageSizeRepository,
                _imageStorageAccessUrlRepository, _imageUploadRepository,
                _storageProcessorMock.Object, _queueMessageServiceMock.Object, _processThumbTrysCountRepositor);
        }

        [Fact]
        public async Task UploadServiceImageProcess_ValidImage_ImageDataWereAdded()
        {
            AddImageDto addImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto.ImageVariant = ImageVariant.Service;

            ServiceImageId = addImageDto.ImageId;

            // act
            await _sut.UploadServiceImageProcess(addImageDto);

            Image? imageFromRepos = await _imageRepository.GetByIdAsnc(ServiceImageId);

            Assert.NotNull(imageFromRepos);
            Assert.Equal(addImageDto.ImageId, imageFromRepos.id);
            Assert.Equal(addImageDto.ContentType, imageFromRepos.MimeType);
            Assert.Equal(addImageDto.OriginalImageName, imageFromRepos.OriginalImageName);
            Assert.Equal(addImageDto.HHIHEventKey, imageFromRepos.hhihEventKey);
            Assert.Equal(addImageDto.HHIHPhotographerKey, imageFromRepos.hhihPhotographerKey);
            Assert.Equal(addImageDto.WidthPixels, imageFromRepos.WidthPixels);
            Assert.Equal(addImageDto.HeightPixels, imageFromRepos.HeightPixels);
            Assert.Equal(addImageDto.SizeInBytes, imageFromRepos.SizeInBytes);
            Assert.Equal(addImageDto.HasTransparentAlphaLayer, imageFromRepos.HasTransparentAlphaLayer);
            Assert.Equal(addImageDto.AutoThumbnails, imageFromRepos.AutoThumbnails);

            ImageStorage? imageStorageFromRepos = _imageStorageRepository
                .GetByImageIdAndImageVariant(addImageDto.ImageId, addImageDto.ImageVariant);

            Assert.NotNull(imageStorageFromRepos);
            Assert.Equal(addImageDto.ImageId, imageStorageFromRepos.imageId);
            Assert.Equal(addImageDto.Name, imageStorageFromRepos.BlobName);

            var imageApplicationRetentionRepos = await _imageApplicationRetentionRepository
                .GetByIdAsnc(addImageDto.ImageId);

            Assert.NotNull(imageApplicationRetentionRepos);
            Assert.Equal(addImageDto.ImageId, imageApplicationRetentionRepos.id);
            Assert.Equal(addImageDto.SourceApplication.ToString(), imageApplicationRetentionRepos.sourceApplicationName);
        }

        [Fact]
        public async Task UploadImageProcessAsync_ValidImage_ImageDataWereAdded()
        {
            AddImageDto addImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto.ImageVariant = ImageVariant.Main;

            MainImageId = addImageDto.ImageId;

            // act
            await _sut.UploadImageProcessAsync(addImageDto);

            Image? imageFromRepos = await _imageRepository.GetByIdAsnc(MainImageId);

            Assert.NotNull(imageFromRepos);
            Assert.Equal(addImageDto.ImageId, imageFromRepos.id);
            Assert.Equal(addImageDto.ContentType, imageFromRepos.MimeType);
            Assert.Equal(addImageDto.OriginalImageName, imageFromRepos.OriginalImageName);
            Assert.Equal(addImageDto.HHIHEventKey, imageFromRepos.hhihEventKey);
            Assert.Equal(addImageDto.HHIHPhotographerKey, imageFromRepos.hhihPhotographerKey);
            Assert.Equal(addImageDto.WidthPixels, imageFromRepos.WidthPixels);
            Assert.Equal(addImageDto.HeightPixels, imageFromRepos.HeightPixels);
            Assert.Equal(addImageDto.SizeInBytes, imageFromRepos.SizeInBytes);
            Assert.Equal(addImageDto.HasTransparentAlphaLayer, imageFromRepos.HasTransparentAlphaLayer);
            Assert.Equal(addImageDto.AutoThumbnails, imageFromRepos.AutoThumbnails);

            ImageStorage? imageStorageFromRepos = _imageStorageRepository
                .GetByImageIdAndImageVariant(addImageDto.ImageId, addImageDto.ImageVariant);

            Assert.NotNull(imageStorageFromRepos);
            Assert.Equal(addImageDto.ImageId, imageStorageFromRepos.imageId);
            Assert.Equal(addImageDto.Name, imageStorageFromRepos.BlobName);

            var imageApplicationRetentionRepos = await _imageApplicationRetentionRepository
                .GetByIdAsnc(addImageDto.ImageId);

            Assert.NotNull(imageApplicationRetentionRepos);
            Assert.Equal(addImageDto.ImageId, imageApplicationRetentionRepos.id);
            Assert.Equal(addImageDto.SourceApplication.ToString(), imageApplicationRetentionRepos.sourceApplicationName);
        }


        [Fact]
        public async Task RemoveServiceImageAsync_InvalidImageId_ThrowsNullReferenceException()
        {
            // act & assert
            NullReferenceException exception = await Assert.ThrowsAsync<NullReferenceException>(
                () => _sut.RemoveServiceImageAsync(InvalidImageId));

            _storageProcessorMock.Verify(m =>
                m.StorageFileDeleteAsync(It.IsAny<string>(), It.IsAny<ImageVariant>()), Times.Never);

            //The thrown exception can be used for even more detailed assertions.
            //Assert.Equal($"There is no image with {InvalidImageId} id.", exception.Message);
        }

        [Fact]
        public async Task RemoveImageByIdAsync_InvalidImageId_ThrowsNullReferenceException()
        {
            // act & assert
            NullReferenceException exception = await Assert.ThrowsAsync<NullReferenceException>(
                () => _sut.RemoveImageByIdAsync(InvalidImageId));

            _storageProcessorMock.Verify(m =>
                m.StorageFileDeleteAsync(It.IsAny<string>(), It.IsAny<ImageVariant>()), Times.Never);
        }

        [Fact]
        public async Task RemoveImagesByEventKeyAsync_EventKey_AllImagesAreRemoved()
        {
            int eventKey = 666;
            
            AddImageDto addImageDto1 = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto1.HHIHEventKey = eventKey;
            addImageDto1.ImageVariant = ImageVariant.Main;

            await _sut.UploadImageProcessAsync(addImageDto1);

            AddImageDto addImageDto2 = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto2.HHIHEventKey = eventKey;
            addImageDto2.ImageVariant = ImageVariant.Main;

            await _sut.UploadImageProcessAsync(addImageDto2);

            var images = _imageRepository.GetByEventKey(eventKey);

            Assert.NotNull(images);
            Assert.NotEmpty(images);

            // act
            await _sut.RemoveImagesByEventKeyAsync(eventKey);

            images = _imageRepository.GetByEventKey(eventKey);

            // assert
            Assert.Empty(images);
        }

        [Fact]
        public async Task UpdateProcessedImagesAsync_TestAddImageInfoResponseModel_AllDataAreUpdated()
        {
            AddImageDto addImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto.ImageVariant = ImageVariant.Main;

            UpdatedImageId = addImageDto.ImageId;

            await _sut.UploadImageProcessAsync(addImageDto);

            AddImageInfoResponseModel hhihAddImageResponse = RepositoryDtoTestData
                .GetTestAddImageInfoResponseModelInstance();

            await _sut.UpdateProcessedImagesAsync(UpdatedImageId, addImageDto.ImageVariant,
                hhihAddImageResponse);

            Image? imageFromRepos = await _imageRepository.GetByIdAsnc(UpdatedImageId);

            Assert.NotNull(imageFromRepos);
            
            Assert.Equal(hhihAddImageResponse.HHIHEventKey, imageFromRepos.hhihEventKey);
            Assert.Equal(hhihAddImageResponse.HHIHPhotographerKey, imageFromRepos.hhihPhotographerKey);
            Assert.Equal(hhihAddImageResponse.WatermarkMethod, imageFromRepos.WatermarkMethod);
            Assert.Equal(hhihAddImageResponse.WatermarkImageId, imageFromRepos.WatermarkImageId);

            ImageStorage? imageStorageFromRepos = _imageStorageRepository
                .GetByImageIdAndImageVariant(addImageDto.ImageId, addImageDto.ImageVariant);

            Assert.NotNull(imageStorageFromRepos);
            Assert.Equal(ImageStatus.Ready, imageStorageFromRepos.Status);

            var imageApplicationRetentionRepos = await _imageApplicationRetentionRepository
                .GetByIdAsnc(addImageDto.ImageId);

            Assert.NotNull(imageApplicationRetentionRepos);
            Assert.Equal(hhihAddImageResponse.ExpirationDate, imageApplicationRetentionRepos.expirationDate);

            _storageProcessorMock.Verify(m =>
                m.StorageFileUploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<ImageVariant>(), It.IsAny<string>(), null), Times.Once);
        }

        [Fact]
        public async Task ThumbnailImagesProcess_TestGenerateThumbnailImagesDto_AllImagesAreCreated()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;            

            await _sut.UploadImageProcessAsync(addMainImageDto);

            AddImageDto addServiceImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addServiceImageDto.ImageVariant = ImageVariant.Service;

            await _sut.UploadServiceImageProcess(addServiceImageDto);

            GenerateThumbnailImagesDto generateThumbnailImagesDto = new GenerateThumbnailImagesDto
            {
                ImageId = addMainImageDto.ImageId,
                ContentType = addMainImageDto.ContentType,
                FileName = addMainImageDto.Name,
                OriginalFileName = addMainImageDto.OriginalImageName,
                AutoThumbnails = true,
                WatermarkMethod = WaterMarkType.Single.ToString(),
                WatermarkImageId = addServiceImageDto.ImageId.ToString(),
            };

            // act
            await _sut.ThumbnailImagesProcess(generateThumbnailImagesDto);

            var thumbImages = _imageStorageRepository.GetByImageId(addMainImageDto.ImageId);

            Assert.NotNull(thumbImages);
            Assert.NotEmpty(thumbImages);
            Assert.Equal(7, thumbImages.Count);

            await _sut.RemoveServiceImageAsync(addServiceImageDto.ImageId);
            await _sut.RemoveImageByIdAsync(addMainImageDto.ImageId); 
        }

        [Fact]
        public async Task RebuildThumbnailsWithWatermarkAsync_TestGenerateThumbnailImagesDto_AllThumbImagesAreRecreated()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;            

            AddImageDto addServiceImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addServiceImageDto.ImageVariant = ImageVariant.Service;

            addMainImageDto.HHIHEventKey = 333;
            addMainImageDto.HHIHPhotographerKey = 333;
            addMainImageDto.WatermarkImageId = addServiceImageDto.ImageId.ToString();

            await _sut.UploadServiceImageProcess(addServiceImageDto);
            await _sut.UploadImageProcessAsync(addMainImageDto);

            RebuildThumbnailsWithWatermarkDto rebuildThumbnailsWithWatermarkDto = new RebuildThumbnailsWithWatermarkDto
            {
                WatermarkMethod = WaterMarkType.Single.ToString(),
                WatermarkImageId = addServiceImageDto.ImageId.ToString(),
                StudioKey = addMainImageDto.HHIHPhotographerKey,
                EventKey = addMainImageDto.HHIHEventKey
            };

            // act
            await _sut.RebuildThumbnailsWithWatermarkAsync(rebuildThumbnailsWithWatermarkDto);

            _queueMessageServiceMock.Verify(m =>
                m.SendMessageProcessThumbnailImagesAsync(It.IsAny<GenerateThumbnailImagesDto>()), Times.Once);

            await _sut.RemoveServiceImageAsync(addServiceImageDto.ImageId);
            await _sut.RemoveImageByIdAsync(addMainImageDto.ImageId); 
        }

        [Fact]
        public async Task RebuildWatermarkThumbnailsProcess_TestGenerateThumbnailImagesDto_AllThumbImagesAreRecreated()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;

            AddImageDto addServiceImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addServiceImageDto.ImageVariant = ImageVariant.Service;

            addMainImageDto.HHIHEventKey = 333;
            addMainImageDto.HHIHPhotographerKey = 333;
            addMainImageDto.WatermarkImageId = addServiceImageDto.ImageId.ToString();

            await _sut.UploadServiceImageProcess(addServiceImageDto);
            await _sut.UploadImageProcessAsync(addMainImageDto);

            GenerateThumbnailImagesDto generateThumbnailImagesDto = new GenerateThumbnailImagesDto
            {
                ImageId = addMainImageDto.ImageId,
                ContentType = addMainImageDto.ContentType,
                FileName = addMainImageDto.Name,
                OriginalFileName = addMainImageDto.OriginalImageName,
                AutoThumbnails = true,
                WatermarkMethod = WaterMarkType.No.ToString()
            };

            await _sut.ThumbnailImagesProcess(generateThumbnailImagesDto);

            var thumbImages = _imageStorageRepository.GetByImageId(addMainImageDto.ImageId);

            Assert.NotNull(thumbImages);
            Assert.NotEmpty(thumbImages);
            Assert.Equal(4, thumbImages.Count);

            generateThumbnailImagesDto.IsRebuildThumbnails = true;
            generateThumbnailImagesDto.WatermarkMethod = WaterMarkType.Single.ToString();
            generateThumbnailImagesDto.WatermarkImageId = addServiceImageDto.ImageId.ToString();

            // act
            await _sut.RebuildWatermarkThumbnailsProcess(generateThumbnailImagesDto);

            thumbImages = _imageStorageRepository.GetByImageId(addMainImageDto.ImageId);

            Assert.NotNull(thumbImages);
            Assert.NotEmpty(thumbImages);
            Assert.Equal(7, thumbImages.Count);

            _storageProcessorMock.Verify(m =>
                m.StorageFileUploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<ImageVariant>(), It.IsAny<string>(), null), Times.Exactly(8));

            await _sut.RemoveServiceImageAsync(addServiceImageDto.ImageId);
            await _sut.RemoveImageByIdAsync(addMainImageDto.ImageId);
        }

        [Fact]
        public void GetCreateAccessUrl_TestBlobName_()
        {
            var accessUrl = _sut.GetCreateAccessUrl("TestBlobName");

            Assert.NotNull(accessUrl);
        }

        [Fact]
        public async Task GetAccesUrl_TestMainImage_TestAccessUrl()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;
            
            await _sut.UploadImageProcessAsync(addMainImageDto);

            AddImageInfoResponseModel hhihAddImageResponse = RepositoryDtoTestData
                .GetTestAddImageInfoResponseModelInstance();

            await _sut.UpdateProcessedImagesAsync(addMainImageDto.ImageId, addMainImageDto.ImageVariant,
                hhihAddImageResponse);

            var accessUrl = await _sut.GetAccesUrl(addMainImageDto.ImageId, addMainImageDto.ImageVariant, 0);

            Assert.NotNull(accessUrl);
            Assert.Equal(TestAccessUrl, accessUrl);

            await _sut.RemoveImageByIdAsync(addMainImageDto.ImageId);
        }

        [Fact]
        public async Task GetAccesUrl_TestMainImage_EmptyString()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;

            await _sut.UploadImageProcessAsync(addMainImageDto);

            AddImageInfoResponseModel hhihAddImageResponse = RepositoryDtoTestData
                .GetTestAddImageInfoResponseModelInstance();

            await _sut.UpdateProcessedImagesAsync(addMainImageDto.ImageId, addMainImageDto.ImageVariant,
                hhihAddImageResponse);

            var accessUrl = await _sut.GetAccesUrl(addMainImageDto.ImageId, ImageVariant.SmallThumbnail, 0);

            Assert.NotNull(accessUrl);
            Assert.Equal(string.Empty, accessUrl);

            await _sut.RemoveImageByIdAsync(addMainImageDto.ImageId);
        }

        [Fact]
        public async Task GetAccesUrl_InvalidImageId_EmptyString()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;

            var accessUrl = await _sut.GetAccesUrl(addMainImageDto.ImageId, ImageVariant.Main, 0);

            Assert.NotNull(accessUrl);
            Assert.Equal(string.Empty, accessUrl);
        }

        [Fact]
        public async Task GetImageUploadSasUrlAsync_GetImageUploadSasUrlDto_ImageUploadDataWereSaved()
        {
            GetImageUploadSasUrlDto addImageDto = GetImageUploadSasUrlDto.CreateInstance("TestFileName.jpg");

            addImageDto.PhotographerKey = 777;
            addImageDto.EventKey = 777;
            addImageDto.AlbumKey = 777;
            addImageDto.ColorCorrectLevel = true;
            addImageDto.HiResDownload = true;
            addImageDto.AutoThumbnails = true;
            addImageDto.WatermarkImageId = addImageDto.ImageId.ToString();
            addImageDto.WatermarkMethod = WaterMarkType.Single.ToString();
            addImageDto.HasTransparentAlphaLayer = true;

            string uploadFileAccessUrl = await _sut.GetImageUploadSasUrlAsync(addImageDto);

            Assert.NotNull(uploadFileAccessUrl);

            ImageUpload imageUploadEntity = await _sut.GetImageImageUploadAsync(addImageDto.ImageId);

            Assert.NotNull(imageUploadEntity);
            Assert.Equal(addImageDto.FileName, imageUploadEntity.FileName);
            Assert.Equal(addImageDto.OriginalFileName, imageUploadEntity.OriginalImageName);
            Assert.Equal(addImageDto.WatermarkImageId, imageUploadEntity.WatermarkImageId);
            Assert.Equal(addImageDto.WatermarkMethod, imageUploadEntity.WatermarkMethod);
            Assert.Equal(addImageDto.ExpirationDate, imageUploadEntity.ExpirationDate);
            Assert.Equal(addImageDto.EventKey, imageUploadEntity.hhihEventKey);
            Assert.Equal(addImageDto.PhotographerKey, imageUploadEntity.hhihPhotographerKey);
            Assert.Equal(addImageDto.AlbumKey, imageUploadEntity.AlbumKey);
            Assert.Equal(addImageDto.AutoThumbnails, imageUploadEntity.AutoThumbnails);
            Assert.Equal(addImageDto.HiResDownload, imageUploadEntity.HiResDownload);
            Assert.Equal(addImageDto.ColorCorrectLevel, imageUploadEntity.ColorCorrectLevel);
            Assert.Equal(addImageDto.HasTransparentAlphaLayer, imageUploadEntity.HasTransparentAlphaLayer);
        }

        [Fact]
        public async Task ReadAccessUrl_TestData_StorageFileGetReadAccessUrlWasCalledOnce()
        {
            var imageVariant = ImageVariant.Main;
            DateTimeOffset expireDateTimeOffset = DateTimeOffset.Now
                        .AddDays(1);

            var accessUrl = await _sut.ReadAccessUrl(expireDateTimeOffset, "TestBlobName", imageVariant);

            _storageProcessorMock.Verify(m =>
                m.StorageFileGetReadAccessUrl(It.IsAny<ImageVariant>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()),
                Times.Once);
        }

        [Fact]
        public async Task GetImageAsync_TestData_StorageFileGetReadAccessUrlWasCalledOnce()
        {
            AddImageDto addImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addImageDto.ImageVariant = ImageVariant.Main;

            await _sut.UploadImageProcessAsync(addImageDto);

            var imageFromRepos = await _sut.GetImageAsync(addImageDto.ImageId);

            Assert.NotNull(imageFromRepos);
            Assert.Equal(addImageDto.ImageId, imageFromRepos.id);
            Assert.Equal(addImageDto.ContentType, imageFromRepos.MimeType);
            Assert.Equal(addImageDto.OriginalImageName, imageFromRepos.OriginalImageName);
            Assert.Equal(addImageDto.HHIHEventKey, imageFromRepos.hhihEventKey);
            Assert.Equal(addImageDto.HHIHPhotographerKey, imageFromRepos.hhihPhotographerKey);
            Assert.Equal(addImageDto.WidthPixels, imageFromRepos.WidthPixels);
            Assert.Equal(addImageDto.HeightPixels, imageFromRepos.HeightPixels);
            Assert.Equal(addImageDto.SizeInBytes, imageFromRepos.SizeInBytes);
            Assert.Equal(addImageDto.HasTransparentAlphaLayer, imageFromRepos.HasTransparentAlphaLayer);
            Assert.Equal(addImageDto.AutoThumbnails, imageFromRepos.AutoThumbnails);

            await _sut.RemoveImageByIdAsync(addImageDto.ImageId);
        }

        [Fact]
        public async Task SetImageReadyStatus_TestImageStorage_ImageStatusIsReady()
        {
            AddImageDto addMainImageDto = RepositoryDtoTestData.GetTestAddImageDtoInstance();
            addMainImageDto.ImageVariant = ImageVariant.Main;

            ImageStorage? imageStorage = addMainImageDto
                .CreateImageStorageEntity("TestStorageAccoutName", "TestBloabContainerName");

            await _imageStorageRepository.AddAsync(imageStorage);

            await _sut.SetImageReadyStatus(addMainImageDto.ImageId, addMainImageDto.ImageVariant);

            var imageStorageFromRepos = _imageStorageRepository
                                .GetByImageIdAndImageVariant(addMainImageDto.ImageId, addMainImageDto.ImageVariant);

            Assert.NotNull(imageStorageFromRepos);
            Assert.Equal(ImageStatus.Ready, imageStorageFromRepos.Status);
        }

        public void Dispose()
        {
            //// RemoveServiceImage and Remove Main Image
            //var serviceImage = _imageRepository.GetByIdAsnc(ServiceImageId).Result;
            //var mainImage = _imageRepository.GetByIdAsnc(MainImageId).Result;

            //Assert.NotNull(serviceImage);
            //Assert.NotNull(mainImage);

            //// act
            //_sut.RemoveServiceImageAsync(ServiceImageId).Wait();
            //_sut.RemoveImageByIdAsync(MainImageId).Wait();

            //serviceImage = _imageRepository.GetByIdAsnc(ServiceImageId).Result;
            //mainImage = _imageRepository.GetByIdAsnc(MainImageId).Result;

            //Assert.Null(serviceImage);
            //Assert.Null(mainImage);
        }
    }
}
