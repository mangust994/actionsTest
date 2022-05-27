using System.Collections.Generic;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class UrlsDto
    {
        public ICollection<string> Urls { get; }

        public UrlsDto(ICollection<string> urls) => Urls = urls;
    }
}
