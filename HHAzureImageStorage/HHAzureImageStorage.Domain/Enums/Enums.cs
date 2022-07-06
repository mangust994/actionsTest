using System.Drawing;

namespace HHAzureImageStorage.Domain.Enums
{
    public enum ImageVariant : byte
    {
        Temp = 0,
        Main = 1,
        SmallThumbnail = 10,
        SmallThumbnailWithWatermark = 11,
        MediumThumbnail = 20,
        MediumThumbnailWithWatermark = 21,
        LargeThumbnail = 30,
        LargeThumbnailWithWatermark = 31,
        Service = 40
    }

    public  enum  ImageStatus
    {
        InProgress = 'I',
        NeedsRebuild = 'N',
        Ready = 'R'
    }

    //InProgress = 73, // 0x00000049
    //NeedsRebuild = 78, // 0x0000004E
    //Ready = 82, // 0x00000052

public enum ImageUploader
    {
        DirectPost,
        AutoPost,
        UI
    }

    public enum WaterMarkType
    {
        No,
        Repeat,
        Single
    }

    public static class WaterMarkTypeHelper
    {
        public static WaterMarkType GetTypeFromString(string stringWaterMarkType)
        {
            switch (stringWaterMarkType)
            {
                case "No":
                    return WaterMarkType.No;
                case "Single":
                    return WaterMarkType.Single;
                default:
                    return WaterMarkType.Repeat;
            }
        }

        public static string GetStringFromEnum(WaterMarkType type)
        {
            switch (type)
            {
                case WaterMarkType.No:
                    return "No";
                case WaterMarkType.Single:
                    return "Single";
                default:
                    return "Repeat";
            }
        }

        public static Size GetWatermarkSizeByType(WaterMarkType type)
        {
            switch (type)
            {
                case WaterMarkType.No:
                    return new Size(0, 0);
                case WaterMarkType.Single:
                    return new Size(500, 500);
                default:
                    return new Size(500, 500); // TODO ConfigurationManager.Resolve<StorageConfig>().WatermarkSize;
            }
        }
    }
}
