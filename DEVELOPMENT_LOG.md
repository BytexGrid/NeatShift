# Development Log

## Features Implemented

### Core Functionality
- File and folder movement with symbolic link creation
- Progress tracking during operations
- Error handling and user feedback
- System restore point creation (optional)
- Hidden symbolic links support
- Last used path remembering

### User Interface
- Modern WPF UI with Windows 11 style
- Dark/Light theme support with theme toggle (sun/moon icons)
- Drag and drop support
- File type icons
- Progress bar for operations
- Status messages
- Command bar with primary and secondary commands

### Settings and Configuration
- Settings dialog for configuring:
  - System restore point creation
  - Symbolic link visibility
- Persistent settings storage

### Symbolic Links Management
- View symbolic links in selected folders
- Show/hide symbolic links
- Delete symbolic links
- Show location in Explorer
- Multi-select support for batch operations

### Additional Features
- GitHub repository link
- About dialog with version info
- Error handling for administrative privileges
- Graceful handling of missing DLLs

## Recent Changes

### Theme Toggle Implementation
- Added theme toggle button with sun/moon icons using Segoe MDL2 Assets
- Created BooleanToGlyphConverter for icon switching
- Integrated with ModernWPF's theme system

### Symbolic Links Dialog Improvements
- Added multi-select support
- Added common action bar
- Improved error handling for administrative operations
- Added confirmation dialogs for deletions

### UI Enhancements
- Added gradient background
- Improved spacing and margins
- Added tooltips
- Enhanced visual feedback

### Bug Fixes
- Fixed multiple entry points issue
- Resolved assembly attribute conflicts
- Fixed icon integration
- Improved error handling for system restore
- Fixed symbolic link deletion permissions

## Technical Details

### Architecture
- MVVM pattern with ModernWPF
- Service-based architecture for operations
- Converter pattern for UI bindings
- Event-based progress tracking

### Dependencies
- ModernWPF UI Framework
- Windows API for symbolic links
- System.Windows.Forms for dialogs

### Requirements
- Windows 7 or later
- Administrative privileges for symbolic link operations
- .NET Framework compatibility

## Future Improvements
- Enhanced error reporting
- More customization options
- Batch operation support
- File filtering options
- Network path support 