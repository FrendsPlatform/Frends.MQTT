namespace Frends.MQTT.Receive.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;

    /// <summary>
    /// Sets encoding for the written content. By selecting 'Other' you can use any encoding.
    /// </summary>
    /// <example>FileEncoding.UTF8</example>
    [DefaultValue(PayloadEncoding.UTF8)]
    public PayloadEncoding PayloadEncoding { get; set; }

    /// <summary>
    /// Sets a value indicating whether enablation BOM for UTF-8.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(PayloadEncoding), "", PayloadEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// Sets file encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>ISO-8859-2</example>
    [UIHint(nameof(PayloadEncoding), "", PayloadEncoding.Other)]
    public string EncodingInString { get; set; }
}
