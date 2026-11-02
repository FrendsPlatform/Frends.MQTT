namespace Frends.MQTT.Send.Definitions;

using System;
using System.ComponentModel.DataAnnotations;
using Frends.MQTT.Send.Enums;

/// <summary>
/// MqttMessage class.
/// </summary>
public class MqttMessage
{
    /// <summary>
    /// The topic to publish the message to.
    /// </summary>
    /// <example>your_topic</example>
    required public string Topic { get; set; }

    /// <summary>
    /// The topic to response the message to.
    /// </summary>
    /// <example>your_topic</example>
    public string? ResponseTopic { get; set; }

    /// <summary>
    /// Determines tyhe type of the payload.
    /// </summary>
    /// <example>PayloadType.String</example>
    public PayloadType PayloadType { get; set; }

    /// <summary>
    /// The message content in byte[] or string format to be send to the MQTT broker.
    /// </summary>
    /// <example>{"temperature":22.5,"humidity":45}</example>
    required public dynamic Payload { get; set; }

    /// <summary>
    /// The Quality of Service level used for message delivery.
    /// </summary>
    /// <example>QoS.AtLeastOnce</example>
    public QoS QoS { get; set; }

    /// <summary>
    /// Indicates whether the broker stored the message as the retained message for the topic.
    /// </summary>
    /// <example>true</example>
    public bool Retain { get; set; }

    /// <summary>
    /// Correlation Id of the message. Can be left empty if it's not used.
    /// </summary>
    /// <example>3f29a1c4-9e2b-4d11-8a77-1b6e4f9c2d90</example>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The MIME type or format of the payload.
    /// </summary>
    /// <example>application/json</example>
    public string? ContentType { get; set; }

    /// <summary>
    /// Custom MQTT v5 user-defined key-value metadata attached to the message in JSON format.
    /// </summary>
    /// <example>{ "DeviceId": "sensor-001", "Location": "Office" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string? UserProperties { get; set; }

    /// <summary>
    /// The timestamp when the message was received by the application.
    /// </summary>
    /// <example>2026-06-18T14:30:15Z</example>
    public DateTime ReceivedAt { get; set; }
}
