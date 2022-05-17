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
    }
}
