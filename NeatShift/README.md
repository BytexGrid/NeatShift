# NeatShift

A modern Windows application for organizing files using symbolic links, allowing you to move files while maintaining their original access points.

## Features

### Core Functionality
- Move files and folders to a new location while creating symbolic links in their original location
- Hidden symbolic links for a cleaner file system view
- System Restore point creation before operations (optional)
- Modern WPF interface with dark/light theme support

### Key Features
1. **File Operations**
   - Add individual files or entire folders
   - Move files/folders to a new location
   - Automatic symbolic link creation
   - Hidden symbolic links (configurable)

2. **User Interface**
   - Modern WPF design using ModernWPF UI
   - Drag and drop support
   - Progress tracking
   - Status messages
   - Clean, intuitive layout

3. **Settings**
   - System Restore point creation toggle
   - Symbolic link visibility toggle
   - Settings persistence between sessions

### Technical Details

#### Architecture
- Modern .NET 6.0 WPF application
- MVVM architecture using CommunityToolkit.Mvvm
- Dependency Injection for services
- Async operations for better responsiveness

#### Key Components

1. **Services**
   - `FileOperationService`: Handles file moving and symbolic link creation
   - `SystemRestoreService`: Manages system restore points
   - `SettingsService`: Handles application settings
   - `IOHelper`: Provides file system utilities

2. **Models**
   - `FileSystemItem`: Represents files/folders in the UI
   - `Settings`: Application settings model

3. **ViewModels**
   - `MainWindowViewModel`: Main application logic
   - Command implementations for UI actions

4. **Views**
   - `MainWindow`: Main application window
   - Modern, clean interface design

#### File Operations
1. **Moving Files**
   ```csharp
   public async Task<bool> MoveWithSymbolicLink(string sourcePath, string destinationPath)
   ```
   - Moves files/folders to destination
   - Creates hidden symbolic links at source
   - Handles errors gracefully

2. **Symbolic Links**
   ```csharp
   public static bool CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory, bool hideLink = true)
   ```
   - Creates symbolic links
   - Optional hiding (Hidden + System attributes)
   - Preserves functionality while maintaining clean organization

### Settings Management
- Settings stored in AppData
- JSON serialization
- Automatic creation of default settings
- Settings include:
  - CreateRestorePoint (default: true)
  - HideSymbolicLinks (default: true)
  - LastUsedPath

## Development Progress

### Completed
1. ✅ Project setup and architecture
2. ✅ Modern WPF UI implementation
3. ✅ File operations functionality
4. ✅ Symbolic link creation and management
5. ✅ Hidden symbolic links feature
6. ✅ Settings management
7. ✅ Error handling and validation
8. ✅ Progress tracking and status updates

### Planned Features
1. Settings dialog UI
2. Show/hide existing symbolic links
3. Advanced file organization options
4. Batch operations
5. Link management tools

## Technical Requirements
- Windows OS (Windows 10 or later recommended)
- .NET 6.0 Runtime
- Administrator privileges (for symbolic link creation)

## Dependencies
- ModernWPF UI
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- System.Text.Json

## Building and Running
1. Open solution in Visual Studio 2022
2. Restore NuGet packages
3. Build and run

## Notes
- Requires administrator privileges for symbolic link creation
- System Restore functionality requires srrestorept.dll
- Hidden symbolic links can be viewed by enabling "Show hidden files" and "Show system files" in File Explorer