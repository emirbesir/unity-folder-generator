<div align="center">

# Unity Folder Structure Generator

A Unity Editor tool that automatically creates organized folder structures for Unity projects using configurable templates.

![Last Commit](https://img.shields.io/github/last-commit/emirbesir/unity-folder-generator?style=flat&logo=git&logoColor=white&color=0080ff)
![Top Language](https://img.shields.io/github/languages/top/emirbesir/unity-folder-generator?style=flat&color=0080ff)
![Unity](https://img.shields.io/badge/Unity-FFFFFF.svg?style=flat&logo=Unity&logoColor=black)

_Tested with **Unity 6000.1.3f1**_

</div>

## Features

- **Configurable Templates**: Create custom folder structures using ScriptableObject configurations
- **Pre-built Configurations**: Includes 2D and 3D project templates out of the box
- **Git Integration**: Optional `.gitkeep` file generation for empty folders

## Installation & Usage

### Quick Start

1. Download or clone this repository
2. Copy the `FolderStructureGenerator` folder into your Unity project's `Assets/Editor/` directory
3. Open Unity and go to `Assets > Create Folders` in the menu bar
4. Enter your project name in the "Project Name" field
5. Select a configuration file (2D or 3D templates are included)
6. Click "Generate Folders!" to create your folder structure

### Creating Custom Configurations

1. Right-click in your Project window
2. Go to `Create > Unity Tools > Folder Structure Config`
3. Name your configuration file
4. Customize the folder structure in the Inspector:
   - **Default Project Name**: The default name that appears in the generator
   - **Folder Groups**: Main folders with their subfolders
      - **Main Folder**: The parent folder name
      - **Subfolders**: List of child folders
      - **Enabled**: Toggle to include/exclude this group
   - **Standalone Folders**: Independent folders created at the Assets root level
   - **Create Git Keep Files**: Enable/disable `.gitkeep` file generation

### [Built-in Templates for 2D/3D Games](docs/TEMPLATES.md)

## API Reference

### FolderStructureConfig
The main configuration ScriptableObject with the following properties:
- `defaultProjectName`: Default project name
- `folderGroups`: List of main folders and their subfolders
- `standaloneFolders`: Independent folders
- `createGitKeepFiles`: Enable/disable `.gitkeep` generation

### CreateFolders
The editor window class that handles the folder creation process.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.
