using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageStorageSizeRepositoty : IImageStorageSizeRepositoty
    {
        readonly List<ImageStorageSize> _collection;

        public InMemoryImageStorageSizeRepositoty()
        {
            _collection = new List<ImageStorageSize>()
            {
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 10,
                    imageVariantId = ImageVariant.SmallThumbnail,
                    Name = "SmallThumbnail",
                    Mnemonic = "S",
                    LongestPixelSize = 230,
                    id = new Guid("27c0c4c9-5cfc-480b-8e01-c89610bbd475")
                },
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 11,
                    imageVariantId = ImageVariant.SmallThumbnailWithWatermark,
                    Name = "SmallThumbnailWithWatermark",
                    Mnemonic = "SW",
                    LongestPixelSize = 230,
                    id = new Guid("64a10d34-8d97-4128-ad3b-aeb8db3cb85c")
                },
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 20,
                    imageVariantId = ImageVariant.MediumThumbnail,
                    Name = "MediumThumbnail",
                    Mnemonic = "M",
                    LongestPixelSize = 512,
                    id = new Guid("17895373-50e1-499e-af7f-8ac2e04091fd")
                },
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 21,
                    imageVariantId = ImageVariant.MediumThumbnailWithWatermark,
                    Name = "MediumThumbnailWithWatermark",
                    Mnemonic = "MW",
                    LongestPixelSize = 512,
                    id = new Guid("f03e4f5b-4b30-4ac2-9890-1182eae131b5")
                },
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 30,
                    imageVariantId = ImageVariant.LargeThumbnail,
                    Name = "LargeThumbnail",
                    Mnemonic = "L",
                    LongestPixelSize = 1024,
                    id = new Guid("e4a3aa98-8284-424b-9273-b09a5200a198")
                },
                new ImageStorageSize()
                {
                    ImageStorageSizeId = 31,
                    imageVariantId = ImageVariant.LargeThumbnailWithWatermark,
                    Name = "LargeThumbnailWithWatermark",
                    Mnemonic = "LW",
                    LongestPixelSize = 1024,
                    id = new Guid("da588c47-e92b-42fd-841e-57c0c01036c2")
                }
            };
        }

        public List<ImageStorageSize> GetThumbSizes()
        {
            return _collection;
        }

        public List<ImageStorageSize> GetWatermarkThumbSizes()
        {
            return _collection.
                    Where(x => x.imageVariantId == ImageVariant.SmallThumbnailWithWatermark ||
                        x.imageVariantId == ImageVariant.MediumThumbnailWithWatermark ||
                        x.imageVariantId == ImageVariant.LargeThumbnailWithWatermark)
                    .ToList();
        }
    }
}
