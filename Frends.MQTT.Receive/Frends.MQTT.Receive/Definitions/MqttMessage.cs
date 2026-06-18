namespace Frends.MQTT.Receive.Definitions;

using System;
using System.Collections.Generic;
using MQTTnet.Protocol;

/// <summary>
/// MqttMessage class.
/// </summary>
public class MqttMessage
{
    /// <summary>
    /// The message content received from the MQTT broker.
    /// </summary>
    /// <example>{"temperature":22.5,"humidity":45}</example>
    public string Payload { get; set; }

    /// <summary>
    /// The MQTT topic on which the message was received.
    /// </summary>
    /// <example>sensors/office/environment</example>
    public string Topic { get; set; }

    /// <summary>
    /// The Quality of Service level used for message delivery.
    /// </summary>
    /// <example>MqttQualityOfServiceLevel.AtLeastOnce</example>
    public MqttQualityOfServiceLevel QoS { get; set; }

    /// <summary>
    /// Indicates whether the broker stored the message as the retained message for the topic.
    /// </summary>
    /// <example>true</example>
    public bool Retain { get; set; }

    /// <summary>
    /// The MIME type or format of the payload.
    /// </summary>
    /// <example>application/json</example>
    public string ContentType { get; set; }

    /// <summary>
    /// Custom MQTT v5 user-defined key-value metadata attached to the message.
    /// </summary>
    /// <example>{ "DeviceId": "sensor-001", "Location": "Office" }</example>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// The timestamp when the message was received by the application.
    /// </summary>
    /// <example>2026-06-18T14:30:15Z</example>
    public DateTime ReceivedAt { get; set; }
}
