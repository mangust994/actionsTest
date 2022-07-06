namespace HHAzureImageStorage.FunctionApp.Models
{
    public class BaseResponseModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BaseResponseModel(string message, bool success = true)
        {
            Success = success;
            Message = message;
        }
    }
}
