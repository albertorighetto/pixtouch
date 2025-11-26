using PixTouch.Core.Models;
using Xunit;

namespace PixTouch.Tests;

public class ControlMappingTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var mapping = new ControlMapping();

        // Assert
        Assert.NotNull(mapping.Id);
        Assert.NotEmpty(mapping.Id);
        Assert.Equal(string.Empty, mapping.Label);
        Assert.Equal(string.Empty, mapping.ParameterPath);
        Assert.Equal(0.0, mapping.MinValue);
        Assert.Equal(100.0, mapping.MaxValue);
        Assert.Equal(0.0, mapping.DefaultValue);
        Assert.Equal(1.0, mapping.CoarseStep);
        Assert.Equal(0.1, mapping.FineStep);
        Assert.True(mapping.SyncWithPixera);
        Assert.Equal("{0:F2}", mapping.DisplayFormat);
    }

    [Fact]
    public void DisplayFormat_FormatsCorrectly()
    {
        // Arrange
        var mapping = new ControlMapping
        {
            DisplayFormat = "{0:F1}°"
        };
        double value = 45.67;

        // Act
        string formatted = string.Format(mapping.DisplayFormat, value);

        // Assert
        Assert.Equal("45.7°", formatted);
    }
}

public class EncoderControlTests
{
    [Fact]
    public void Label_WithMapping_ReturnsLabel()
    {
        // Arrange
        var encoder = new EncoderControl
        {
            Index = 0,
            Mapping = new ControlMapping { Label = "Position X" }
        };

        // Act
        var label = encoder.Label;

        // Assert
        Assert.Equal("Position X", label);
    }

    [Fact]
    public void Label_WithoutMapping_ReturnsDefaultLabel()
    {
        // Arrange
        var encoder = new EncoderControl { Index = 2 };

        // Act
        var label = encoder.Label;

        // Assert
        Assert.Equal("Encoder 3", label);
    }

    [Fact]
    public void FormattedValue_WithMapping_UsesFormat()
    {
        // Arrange
        var encoder = new EncoderControl
        {
            Index = 0,
            Mapping = new ControlMapping { DisplayFormat = "{0:F0}%" },
            CurrentValue = 75.6
        };

        // Act
        var formatted = encoder.FormattedValue;

        // Assert
        Assert.Equal("76%", formatted);
    }

    [Fact]
    public void FormattedValue_WithoutMapping_UsesDefault()
    {
        // Arrange
        var encoder = new EncoderControl
        {
            Index = 0,
            CurrentValue = 45.678
        };

        // Act
        var formatted = encoder.FormattedValue;

        // Assert
        Assert.Equal("45.68", formatted);
    }
}
