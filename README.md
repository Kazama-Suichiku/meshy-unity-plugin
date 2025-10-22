# Meshy Unity Plugin - Bridge Edition

A streamlined Unity plugin that enables direct model import from [Meshy.ai](https://www.meshy.ai) web interface to Unity Editor.

## Features

- **🌉 Bridge Mode**: Direct import of 3D models from Meshy web interface to Unity
- **📦 Multi-Format Support**: GLB, FBX, and ZIP archives
- **🎨 Automatic Material Setup**: Materials and textures imported automatically
- **🎬 Animation Support**: Built-in animation clip and AnimatorController generation
- **🚀 One-Click Operation**: Simple menu integration in Unity Editor

## Requirements

- Unity 2021.3 or higher
- Meshy account ([Sign up here](https://www.meshy.ai))

## Installation

### Option 1: Import UnityPackage (Recommended)
1. Download `meshy-unity-plugin.unitypackage` from releases
2. In Unity, go to `Assets > Import Package > Custom Package...`
3. Select the downloaded package and import all files

### Option 2: From Source
1. Clone this repository:
   ```bash
   git clone https://github.com/meshy-dev/meshy-unity-plugin.git
   ```
2. Copy `Unity/Packages/ai.meshy` to your Unity project's `Packages` folder
3. Unity will automatically detect and load the plugin

## Usage

### Starting the Bridge

1. In Unity Editor, go to **`Meshy > Bridge`**
2. Click **"Run Bridge"** to start the local server
3. The server will listen on port **5326**

### Importing Models

1. Make sure Bridge is running in Unity
2. Go to [Meshy.ai](https://www.meshy.ai) and create your 3D model
3. When your model is ready, click the **"Send to Unity"** button on the Meshy website
4. The model will automatically import into your Unity scene at `Assets/MeshyImports/`

### Supported Formats

- **GLB**: Imported with materials and animations
- **FBX**: Imported with textures and materials
- **ZIP**: Automatically extracted and processed

## How It Works

The plugin runs a local TCP server on port 5326 that:
1. Listens for requests from the Meshy web interface
2. Downloads the model file from provided URL
3. Imports it into Unity with proper material setup
4. Automatically creates AnimatorController for animated models
5. Instantiates the model in the current scene

## Security

The bridge server only accepts connections from:
- `https://www.meshy.ai`

## Troubleshooting

**Bridge won't start:**
- Check if port 5326 is already in use
- Try restarting Unity Editor

**Model import fails:**
- Check Unity Console for error messages
- Ensure you have write permissions to `Assets/MeshyImports/`
- Verify your internet connection

**Textures missing:**
- FBX files should include textures in the same folder
- Check the Unity Console for texture import logs

## Building from Source

If you want to export your own UnityPackage:

1. Open the Unity project in `Unity/` folder
2. In Unity Editor, go to **`Meshy > Export Package`** (if available)
3. Or manually: `Assets > Export Package...` and select `Packages/ai.meshy`

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Support

- Documentation: [Meshy Docs](https://docs.meshy.ai)
- Issues: [GitHub Issues](https://github.com/meshy-dev/meshy-unity-plugin/issues)
- Community: [Discord](https://discord.gg/meshy) (if available)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Changelog

### Version 0.2.0 (Current)
- Removed API browser functionality
- Streamlined to Bridge-only mode
- Improved stability and performance
- Added support for multiple animation clips

### Version 0.1.3 (Legacy)
- Full API integration with asset browser
- Text to Model / Text to Texture features

---

Made with ❤️ by [Meshy](https://www.meshy.ai)

