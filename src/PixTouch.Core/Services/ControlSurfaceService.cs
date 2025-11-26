using System.Collections.ObjectModel;
using PixTouch.Core.Models;

namespace PixTouch.Core.Services;

public class ControlSurfaceService
{
    private const int DefaultEncoderCount = 6;
    private const int DefaultFaderCount = 8;

    public ObservableCollection<EncoderControl> Encoders { get; }
    public ObservableCollection<FaderControl> Faders { get; }

    public event EventHandler<EncoderInputEventArgs>? EncoderInput;
    public event EventHandler<FaderInputEventArgs>? FaderInput;

    public ControlSurfaceService()
    {
        Encoders = new ObservableCollection<EncoderControl>();
        Faders = new ObservableCollection<FaderControl>();

        InitializeEncoders();
        InitializeFaders();
    }

    private void InitializeEncoders()
    {
        for (int i = 0; i < DefaultEncoderCount; i++)
        {
            Encoders.Add(new EncoderControl
            {
                Index = i,
                CurrentValue = 0.0
            });
        }
    }

    private void InitializeFaders()
    {
        for (int i = 0; i < DefaultFaderCount; i++)
        {
            Faders.Add(new FaderControl
            {
                Index = i,
                CurrentValue = 0.0
            });
        }
    }

    public void SetEncoderMapping(int index, ControlMapping? mapping)
    {
        if (index < 0 || index >= Encoders.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Encoders[index].Mapping = mapping;
        if (mapping != null)
        {
            Encoders[index].CurrentValue = mapping.DefaultValue;
        }
    }

    public void SetFaderMapping(int index, ControlMapping? mapping)
    {
        if (index < 0 || index >= Faders.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Faders[index].Mapping = mapping;
        if (mapping != null)
        {
            Faders[index].CurrentValue = mapping.DefaultValue;
        }
    }

    public void HandleEncoderInput(int index, int delta, bool fineMode = false)
    {
        if (index < 0 || index >= Encoders.Count)
            return;

        var encoder = Encoders[index];
        if (encoder.Mapping == null)
            return;

        var step = fineMode ? encoder.Mapping.FineStep : encoder.Mapping.CoarseStep;
        var newValue = encoder.CurrentValue + (delta * step);
        newValue = Math.Clamp(newValue, encoder.Mapping.MinValue, encoder.Mapping.MaxValue);

        encoder.CurrentValue = newValue;

        OnEncoderInput(new EncoderInputEventArgs
        {
            Index = index,
            Delta = delta,
            FineMode = fineMode,
            NewValue = newValue,
            ParameterPath = encoder.Mapping.ParameterPath
        });
    }

    public void HandleFaderInput(int index, double value)
    {
        if (index < 0 || index >= Faders.Count)
            return;

        var fader = Faders[index];
        if (fader.Mapping == null)
            return;

        var newValue = Math.Clamp(value, fader.Mapping.MinValue, fader.Mapping.MaxValue);
        fader.CurrentValue = newValue;

        OnFaderInput(new FaderInputEventArgs
        {
            Index = index,
            Value = newValue,
            ParameterPath = fader.Mapping.ParameterPath
        });
    }

    protected virtual void OnEncoderInput(EncoderInputEventArgs e)
    {
        EncoderInput?.Invoke(this, e);
    }

    protected virtual void OnFaderInput(FaderInputEventArgs e)
    {
        FaderInput?.Invoke(this, e);
    }
}

public class EncoderInputEventArgs : EventArgs
{
    public int Index { get; set; }
    public int Delta { get; set; }
    public bool FineMode { get; set; }
    public double NewValue { get; set; }
    public string ParameterPath { get; set; } = string.Empty;
}

public class FaderInputEventArgs : EventArgs
{
    public int Index { get; set; }
    public double Value { get; set; }
    public string ParameterPath { get; set; } = string.Empty;
}
