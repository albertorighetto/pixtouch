namespace PixTouch.Core.Models;

public class FaderControl
{
    public int Index { get; set; }
    public ControlMapping? Mapping { get; set; }
    public double CurrentValue { get; set; }
    
    public string Label => Mapping?.Label ?? $"Fader {Index + 1}";
    public string FormattedValue => Mapping != null 
        ? string.Format(Mapping.DisplayFormat, CurrentValue)
        : CurrentValue.ToString("F2");
}
