using System;

public class MqttSenderException : Exception
{
    public MqttSenderException(string message, Exception innerException)
        : base(message, innerException) { }
}

