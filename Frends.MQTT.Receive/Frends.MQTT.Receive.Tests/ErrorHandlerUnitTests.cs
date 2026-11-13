namespace Frends.MQTT.Receive.Tests;

using System;
using System.Threading;
using Frends.MQTT.Receive.Definitions;
using Frends.MQTT.Receive.Helpers;
using NUnit.Framework;

[TestFixture]
public class ErrorHandlerUnitTests
{
    [Test]
    public void Handle_ReturnsFailedResultAndOriginalException_WhenThrowingIsDisabled()
    {
        var original = new InvalidOperationException("original failure");
        var options = new Options { ThrowErrorOnFailure = false };

        var result = original.Handle(options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.Message, Is.EqualTo("original failure"));
        Assert.That(result.Error.AdditionalInfo, Is.SameAs(original));
    }

    [Test]
    public void Handle_PrependsCustomMessage_WhenThrowingIsDisabled()
    {
        var original = new InvalidOperationException("original failure");
        var options = new Options { ThrowErrorOnFailure = false, ErrorMessageOnFailure = "MQTT failed" };

        var result = original.Handle(options);

        Assert.That(result.Error!.Message, Is.EqualTo("MQTT failed: original failure"));
    }

    [Test]
    public void Handle_ThrowsWithOriginalExceptionAsInnerException_WhenThrowingIsEnabled()
    {
        var original = new InvalidOperationException("original failure");
        var options = new Options { ThrowErrorOnFailure = true };

        var thrown = Assert.Throws<Exception>(() => original.Handle(options));

        Assert.That(thrown!.Message, Is.EqualTo("original failure"));
        Assert.That(thrown.InnerException, Is.SameAs(original));
    }

    [Test]
    public void Handle_ThrowsWithCustomMessage_WhenThrowingIsEnabled()
    {
        var original = new InvalidOperationException("original failure");
        var options = new Options { ThrowErrorOnFailure = true, ErrorMessageOnFailure = "MQTT failed" };

        var thrown = Assert.Throws<Exception>(() => original.Handle(options));

        Assert.That(thrown!.Message, Is.EqualTo("MQTT failed"));
        Assert.That(thrown.InnerException, Is.SameAs(original));
    }

    [Test]
    public void Handle_RethrowsCancellation_WhenThrowCanceledIsTrue()
    {
        var canceled = new OperationCanceledException("cancelled");
        var options = new Options { ThrowErrorOnFailure = false };

        var thrown = Assert.Throws<OperationCanceledException>(() => canceled.Handle(options));

        Assert.That(thrown, Is.SameAs(canceled));
    }

    [Test]
    public void Handle_ReturnsCancellationAsFailure_WhenThrowCanceledIsFalse()
    {
        var canceled = new OperationCanceledException("cancelled");
        var options = new Options { ThrowErrorOnFailure = false };

        var result = canceled.Handle(options, throwCanceled: false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("cancelled"));
    }
}
