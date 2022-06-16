using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class ImageServiceTests
    {
        private readonly ImageService _sut;

        private readonly Guid ValidImageId;
        private readonly Guid InvalidImageId;

        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly Mock<BlobStorageSettings> _storageSettingsMock;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IImageStorageRepository> _imageStorageRepositoryMock;
        private readonly Mock<IImageApplicationRetentionRepository> _imageApplicationRetentionRepositoryMock;
        private readonly Mock<IImageStorageSizeRepositoty> _imageStorageSizeRepositoryMock;
        private readonly Mock<IImageStorageAccessUrlRepository> _imageStorageAccesUrlRepositoryMock;
        private readonly Mock<IImageUploadRepository> _imageUploadRepositoryMock;
        private readonly Mock<IStorageHelper> _storageHelperMock;
        private readonly Mock<IStorageProcessor> _storageProcessorMock;
        private readonly Mock<IQueueMessageService> _queueMessageServiceMock;
        private readonly Mock<IImageResizer> _imageResizerMock;

        public ImageServiceTests()
        {
            ValidImageId = Guid.NewGuid();
            InvalidImageId = Guid.NewGuid();

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
            _imageRepositoryMock = new Mock<IImageRepository>();

            var image = new Image()
            {

            };

            _imageRepositoryMock.Setup(x => x.GetByIdAsnc(ValidImageId))
                .Returns(() => Task.FromResult(image));

            _imageRepositoryMock.Setup(x => x.GetByIdAsnc(InvalidImageId))
                .Returns(() => null);

            _imageRepositoryMock.Setup(x => x.RemoveAsync(ValidImageId));

            _imageStorageRepositoryMock = new Mock<IImageStorageRepository>();

            _imageStorageRepositoryMock.Setup(x => x.GetByImageIdAndImageVariant(InvalidImageId,
                It.IsAny<ImageVariant>()))
                .Returns(() => null);

            var imageStorage = new ImageStorage()
            {

            };

            _imageStorageRepositoryMock.Setup(x => x.GetByImageIdAndImageVariant(ValidImageId,
                It.IsAny<ImageVariant>()))
                .Returns(() => imageStorage);

            _imageStorageRepositoryMock.Setup(x => x.RemoveAsync(ValidImageId,
                It.IsAny<ImageVariant>()));

            _imageStorageAccesUrlRepositoryMock = new Mock<IImageStorageAccessUrlRepository>();

            _imageStorageAccesUrlRepositoryMock.Setup(x => x.RemoveAsync(ValidImageId,
                It.IsAny<ImageVariant>()));

            _imageApplicationRetentionRepositoryMock = new Mock<IImageApplicationRetentionRepository>();

            _imageApplicationRetentionRepositoryMock.Setup(x => x.RemoveAsync(ValidImageId));

            

            
            _imageStorageSizeRepositoryMock = new Mock<IImageStorageSizeRepositoty>();
            
            _imageUploadRepositoryMock = new Mock<IImageUploadRepository>();
            _storageHelperMock = new Mock<IStorageHelper>();
            _storageProcessorMock = new Mock<IStorageProcessor>();
            _queueMessageServiceMock = new Mock<IQueueMessageService>();
            _imageResizerMock = new Mock<IImageResizer>();

            _sut = new ImageService(_mockLoggerFactory.Object, _storageSettingsMock.Object, _imageRepositoryMock.Object,
                _imageStorageRepositoryMock.Object, _imageApplicationRetentionRepositoryMock.Object,
                _storageHelperMock.Object, _imageResizerMock.Object, _imageStorageSizeRepositoryMock.Object,
                _imageStorageAccesUrlRepositoryMock.Object, _imageUploadRepositoryMock.Object,
                _storageProcessorMock.Object, _queueMessageServiceMock.Object);
        }


        [Fact]        
        public async Task RemoveServiceImageAsync_InvalidImageId_ThrowsNullReferenceException()
        {
            // act & assert
            NullReferenceException exception = await Assert.ThrowsAsync<NullReferenceException>(
                () => _sut.RemoveServiceImageAsync(InvalidImageId));

            _imageRepositoryMock.Verify(m => m.GetByIdAsnc(InvalidImageId), Times.Once);
            _imageStorageRepositoryMock.Verify(m => 
                m.GetByImageIdAndImageVariant(InvalidImageId, It.IsAny<ImageVariant>()), Times.Never);
            _storageProcessorMock.Verify(m => 
                m.StorageFileDeleteAsync(It.IsAny<string>(), It.IsAny<ImageVariant>()), Times.Never);

            //The thrown exception can be used for even more detailed assertions.
            //Assert.Equal($"There is no image with {InvalidImageId} id.", exception.Message);
        }

        [Fact]
        public async Task RemoveServiceImageAsync_ValidImageId_VerifyAllActions()
        {
            // act
            await _sut.RemoveServiceImageAsync(ValidImageId);

            //await _imageRepository.RemoveAsync(imageId);
            //await _imageStorageRepository.RemoveAsync(imageId, imageVariant);
            //await _imageStorageAccesUrlRepository.RemoveAsync(imageId, imageVariant);
            //await _imageApplicationRetentionRepository.RemoveAsync(imageId);

            // assert
            _imageRepositoryMock.Verify(m => m.GetByIdAsnc(ValidImageId), Times.Once);
            _imageStorageRepositoryMock.Verify(m => 
                m.GetByImageIdAndImageVariant(ValidImageId, It.IsAny<ImageVariant>()), Times.Once);
            _storageProcessorMock.Verify(m => 
                m.StorageFileDeleteAsync(It.IsAny<string>(), It.IsAny<ImageVariant>()), Times.Once);

            _imageRepositoryMock.Verify(m => m.RemoveAsync(ValidImageId), Times.Once);
            _imageApplicationRetentionRepositoryMock.Verify(m => m.RemoveAsync(ValidImageId), Times.Once);
            _imageStorageRepositoryMock.Verify(m =>
                m.RemoveAsync(ValidImageId, It.IsAny<ImageVariant>()), Times.Once);
            _imageStorageAccesUrlRepositoryMock.Verify(m =>
                m.RemoveAsync(ValidImageId, It.IsAny<ImageVariant>()), Times.Once);
        }
    }
}
