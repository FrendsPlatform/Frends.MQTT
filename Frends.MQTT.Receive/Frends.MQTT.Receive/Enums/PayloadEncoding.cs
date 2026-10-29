// self explanatory.
#pragma warning disable CS1591
#pragma warning disable SA1602 // Enumeration items should be documented
namespace Frends.MQTT.Receive.Definitions;

/// <summary>
/// File encoding for write operations.
/// </summary>
public enum PayloadEncoding
{
    UTF8,
    Default,
    ASCII,
    Unicode,
    Windows1252,
    Other,
}
