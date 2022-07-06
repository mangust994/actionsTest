using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageStorageSizeCosmosRepositoty : IImageStorageSizeRepositoty
    {
        private readonly IImageStorageSizeCosmosContext _context;

        public ImageStorageSizeCosmosRepositoty(IImageStorageSizeCosmosContext context) => this._context = context;

        public List<ImageStorageSize> GetThumbSizes()
        {
            try
            {
                var getImageStorageQuery = _context.Container
                            .GetItemLinqQueryable<ImageStorageSize>(true);

                return getImageStorageQuery.ToList();
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public List<ImageStorageSize> GetWatermarkThumbSizes()
        {
            try
            {
                var getImageStorageQuery = _context.Container
                            .GetItemLinqQueryable<ImageStorageSize>(true);

                return getImageStorageQuery.
                    Where(x => x.imageVariantId == Domain.Enums.ImageVariant.SmallThumbnailWithWatermark ||
                        x.imageVariantId == Domain.Enums.ImageVariant.MediumThumbnailWithWatermark ||
                        x.imageVariantId == Domain.Enums.ImageVariant.LargeThumbnailWithWatermark)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }
    }
}
