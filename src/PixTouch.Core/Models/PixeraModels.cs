namespace PixTouch.Core.Models;

/// <summary>
/// Represents a Pixera timeline
/// </summary>
public class Timeline
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Opacity { get; set; } = 100.0;
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public double Duration { get; set; }
}

/// <summary>
/// Represents a layer within a timeline
/// </summary>
public class Layer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TimelineId { get; set; } = string.Empty;
    public double Opacity { get; set; } = 100.0;
    public Position Position { get; set; } = new();
    public Scale Scale { get; set; } = new();
    public Rotation Rotation { get; set; } = new();
}

/// <summary>
/// Represents a 3D position
/// </summary>
public class Position
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

/// <summary>
/// Represents scale values
/// </summary>
public class Scale
{
    public double X { get; set; } = 100.0;
    public double Y { get; set; } = 100.0;
    public double Z { get; set; } = 100.0;
}

/// <summary>
/// Represents rotation angles in degrees
/// </summary>
public class Rotation
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

/// <summary>
/// Represents a Pixera resource
/// </summary>
public class Resource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Pixera screen
/// </summary>
public class Screen
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}
