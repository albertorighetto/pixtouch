# Contributing to PixTouch

Thank you for your interest in contributing to PixTouch! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- Git
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider (recommended)
- Optional: Loupedeck device for hardware testing

### Setting Up Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/albertorighetto/pixtouch.git
   cd pixtouch
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/PixTouch/PixTouch.csproj
   ```

## Project Structure

```
pixtouch/
├── src/
│   ├── PixTouch/              # Main Avalonia application
│   ├── PixTouch.Core/         # Shared library
│   └── PixTouch.LoupedeckPlugin/  # Loupedeck plugin
├── docs/                      # Documentation
└── tests/                     # Unit tests (future)
```

## Development Guidelines

### Coding Standards

- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for I/O operations

### Code Style

```csharp
// Good example
public async Task<Timeline> GetTimelineAsync(string timelineId)
{
    if (string.IsNullOrEmpty(timelineId))
        throw new ArgumentNullException(nameof(timelineId));

    return await _client.InvokeAsync<Timeline>(
        "Pixera.Timelines.GetTimeline", 
        new { timelineId }
    );
}
```

### Git Workflow

1. **Create a branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Write clean, documented code
   - Follow existing patterns
   - Test your changes

3. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add feature: your feature description"
   ```

4. **Push and create a Pull Request**
   ```bash
   git push origin feature/your-feature-name
   ```

### Commit Messages

Follow conventional commit format:

```
feat: add timeline selection panel
fix: resolve encoder value synchronization issue
docs: update API usage examples
refactor: simplify connection management
test: add unit tests for ControlSurfaceService
```

## Areas for Contribution

### High Priority

1. **Enhanced UI Components**
   - Element selection panel (timelines, layers, effects)
   - Property editor interface
   - Preset management system
   - Touch gesture support

2. **Pixera API Coverage**
   - Complete API namespace implementations
   - Response model definitions
   - Error handling improvements
   - API documentation

3. **Testing**
   - Unit tests for core services
   - Integration tests
   - Mock Pixera server
   - UI tests

### Medium Priority

4. **Features**
   - Full-screen mode
   - Configuration UI
   - Parameter preset system
   - Group operations
   - Undo/redo support

5. **Plugin Enhancements**
   - Additional hardware support
   - Custom button mappings
   - Visual feedback improvements
   - Plugin configuration UI

### Low Priority

6. **Performance**
   - Message batching
   - Connection pooling
   - Caching strategies
   - Memory optimizations

7. **Documentation**
   - Video tutorials
   - Advanced usage examples
   - Troubleshooting guide
   - API reference

## Adding New Features

### Adding a New Encoder/Fader

1. Update `ControlSurfaceService` if needed
2. Add UI elements in MainWindow.axaml
3. Update ViewModel bindings
4. Add configuration support
5. Update documentation

### Adding Pixera API Methods

1. Add extension method to `PixeraApiExtensions.cs`
```csharp
public static Task<Cue> GetCueAsync(this PixeraApiClient client, string timelineId, string cueId)
    => client.InvokeAsync<Cue>("Pixera.Timelines.GetCue", new { timelineId, cueId });
```

2. Add models if needed in `Models/PixeraModels.cs`
3. Update `docs/API_USAGE.md`
4. Add usage examples

### Adding a New View

1. Create XAML view in `Views/` folder
2. Create ViewModel in `ViewModels/` folder
3. Register in `ViewLocator.cs` if needed
4. Add navigation/access from MainWindow
5. Follow MVVM pattern

## Testing

### Manual Testing

1. **Connection Testing**
   - Test with/without Pixera running
   - Test auto-reconnect
   - Test error scenarios

2. **UI Testing**
   - Test all encoder/fader controls
   - Test configuration loading
   - Test dark theme

3. **Plugin Testing**
   - Test with real hardware if available
   - Test TCP communication
   - Test disconnect/reconnect

### Future: Automated Testing

```csharp
[Fact]
public void SetEncoderMapping_ValidIndex_UpdatesMapping()
{
    var service = new ControlSurfaceService();
    var mapping = new ControlMapping { Label = "Test" };
    
    service.SetEncoderMapping(0, mapping);
    
    Assert.Equal("Test", service.Encoders[0].Label);
}
```

## Documentation

### Code Documentation

Add XML comments for public APIs:

```csharp
/// <summary>
/// Sets the encoder mapping for the specified encoder.
/// </summary>
/// <param name="index">Zero-based encoder index (0-5)</param>
/// <param name="mapping">Control mapping configuration</param>
/// <exception cref="ArgumentOutOfRangeException">Thrown when index is invalid</exception>
public void SetEncoderMapping(int index, ControlMapping? mapping)
{
    // Implementation
}
```

### Updating Documentation

When adding features, update relevant docs:
- `README.md` - Overview and quick start
- `docs/API_USAGE.md` - API examples
- `docs/ARCHITECTURE.md` - Architecture changes
- `docs/LOUPEDECK_SETUP.md` - Plugin setup

## Pull Request Process

1. **Before submitting**
   - Ensure code builds without errors
   - Run manual tests
   - Update documentation
   - Follow coding standards

2. **PR Description**
   ```markdown
   ## Description
   Brief description of changes
   
   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Breaking change
   - [ ] Documentation update
   
   ## Testing
   Describe testing performed
   
   ## Screenshots (if applicable)
   Add screenshots for UI changes
   ```

3. **Review Process**
   - Address review comments
   - Keep PRs focused and small
   - Maintain conversation
   - Update based on feedback

## Common Tasks

### Adding a Package

```bash
cd src/PixTouch.Core
dotnet add package PackageName
```

### Building for Release

```bash
dotnet build -c Release
```

### Publishing

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Questions and Support

- **Issues**: Use GitHub Issues for bugs and feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Check docs/ folder first

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers
- Focus on constructive feedback
- Help others learn and grow

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be acknowledged in:
- README.md
- Release notes
- Project documentation

Thank you for contributing to PixTouch!
