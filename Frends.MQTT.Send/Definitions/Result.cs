namespace Frends.MQTT.Send.Definitions
{
    public class Result
    {
        public bool Success { get; }
        public string Details { get; }

        public Result(bool success, string details)
        {
            Success = success;
            Details = details;
        }
    }
}
