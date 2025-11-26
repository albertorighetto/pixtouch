using PixTouch.Core.Services;
using PixTouch.Core.Models;
using Xunit;

namespace PixTouch.Tests;

public class ControlSurfaceServiceTests
{
    [Fact]
    public void Constructor_InitializesEncodersAndFaders()
    {
        // Arrange & Act
        var service = new ControlSurfaceService();

        // Assert
        Assert.NotNull(service.Encoders);
        Assert.NotNull(service.Faders);
        Assert.Equal(6, service.Encoders.Count);
        Assert.Equal(8, service.Faders.Count);
    }

    [Fact]
    public void SetEncoderMapping_ValidIndex_UpdatesMapping()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping
        {
            Label = "Test Encoder",
            ParameterPath = "Test.Path",
            MinValue = 0,
            MaxValue = 100,
            DefaultValue = 50
        };

        // Act
        service.SetEncoderMapping(0, mapping);

        // Assert
        Assert.Equal("Test Encoder", service.Encoders[0].Label);
        Assert.Equal(50.0, service.Encoders[0].CurrentValue);
    }

    [Fact]
    public void SetEncoderMapping_InvalidIndex_ThrowsException()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.SetEncoderMapping(-1, mapping));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.SetEncoderMapping(10, mapping));
    }

    [Fact]
    public void HandleEncoderInput_WithMapping_UpdatesValue()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping
        {
            Label = "Test",
            ParameterPath = "Test.Path",
            MinValue = 0,
            MaxValue = 100,
            DefaultValue = 50,
            CoarseStep = 5.0
        };
        service.SetEncoderMapping(0, mapping);

        // Act
        service.HandleEncoderInput(0, 1, false); // Increment by 1 with coarse step

        // Assert
        Assert.Equal(55.0, service.Encoders[0].CurrentValue);
    }

    [Fact]
    public void HandleEncoderInput_ClampToMinMax()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping
        {
            Label = "Test",
            ParameterPath = "Test.Path",
            MinValue = 0,
            MaxValue = 100,
            DefaultValue = 95,
            CoarseStep = 10.0
        };
        service.SetEncoderMapping(0, mapping);

        // Act
        service.HandleEncoderInput(0, 1, false); // Would go to 105, but should clamp to 100

        // Assert
        Assert.Equal(100.0, service.Encoders[0].CurrentValue);
    }

    [Fact]
    public void HandleEncoderInput_FineMode_UsesFineStep()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping
        {
            Label = "Test",
            ParameterPath = "Test.Path",
            MinValue = 0,
            MaxValue = 100,
            DefaultValue = 50,
            CoarseStep = 5.0,
            FineStep = 0.5
        };
        service.SetEncoderMapping(0, mapping);

        // Act
        service.HandleEncoderInput(0, 1, true); // Fine mode

        // Assert
        Assert.Equal(50.5, service.Encoders[0].CurrentValue);
    }

    [Fact]
    public void HandleEncoderInput_RaisesEvent()
    {
        // Arrange
        var service = new ControlSurfaceService();
        var mapping = new ControlMapping
        {
            Label = "Test",
            ParameterPath = "Test.Path",
            MinValue = 0,
            MaxValue = 100,
            DefaultValue = 50,
            CoarseStep = 5.0
        };
        service.SetEncoderMapping(0, mapping);

        bool eventRaised = false;
        string? parameterPath = null;
        double newValue = 0;

        service.EncoderInput += (sender, e) =>
        {
            eventRaised = true;
            parameterPath = e.ParameterPath;
            newValue = e.NewValue;
        };

        // Act
        service.HandleEncoderInput(0, 1, false);

        // Assert
        Assert.True(eventRaised);
        Assert.Equal("Test.Path", parameterPath);
        Assert.Equal(55.0, newValue);
    }
}
