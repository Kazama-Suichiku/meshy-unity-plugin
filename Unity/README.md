# Meshy Unity Plugin - Bridge Mode

This Unity plugin provides a seamless bridge between Meshy.ai's web interface and Unity Editor, allowing you to import generated 3D models directly into your Unity projects with a single click.

## Quick Start

### 1. Start the Bridge Server

In Unity Editor:
- Go to **`Meshy > Bridge`** in the menu bar
- Click **"Run Bridge"** button
- The server will start listening on port 5326
- Button will change to **"Bridge ON"** (blue) when running

<p align="center">
  <img src="https://user-images.githubusercontent.com/placeholder-bridge-ui.png" alt="Bridge UI" width="400">
</p>

### 2. Import Models from Meshy.ai

1. Visit [Meshy.ai](https://www.meshy.ai) in your browser
2. Create and generate your 3D model (Text to 3D, Image to 3D, etc.)
3. When the model is ready, click **"Send to Unity"** button on the website
4. The model will automatically appear in your Unity scene!

### 3. Find Your Imported Models

Imported models are saved to: **`Assets/MeshyImports/`**

Each import creates a unique folder containing:
- Model file (GLB/FBX)
- Textures and materials
- AnimatorController (for animated models)

## Supported Features

### File Formats
- **GLB**: Full support with materials and animations
- **FBX**: Support with texture maps
- **ZIP**: Automatically extracted and processed

### Automatic Processing
- ✅ Material creation and assignment
- ✅ Texture import and mapping
- ✅ Animation clip detection
- ✅ AnimatorController generation for multi-clip animations
- ✅ Scene instantiation at origin

### Animation Handling

When importing GLB files with multiple animation clips:
- All clips are detected automatically
- An AnimatorController is created with all animations
- First animation is set as default state
- Animator component is automatically attached

## How It Works

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│  Meshy.ai   │ ──────> │   Bridge    │ ──────> │    Unity    │
│   Website   │  HTTP   │  (Port 5326)│  Import │   Editor    │
└─────────────┘         └─────────────┘         └─────────────┘
```

1. **Bridge Server**: Runs locally in Unity Editor on port 5326
2. **Web Request**: Meshy website sends model URL to bridge
3. **Download**: Bridge downloads model from Meshy servers
4. **Import**: Model is imported into Unity with full setup
5. **Scene Addition**: Model appears in active scene automatically

## Troubleshooting

### Bridge Won't Start
**Problem**: Error when clicking "Run Bridge"

**Solutions**:
- Check if port 5326 is already in use
- Close any other applications using this port
- Restart Unity Editor

### Model Not Importing
**Problem**: Clicked "Send to Unity" but nothing happens

**Solutions**:
- Ensure Bridge is running (button shows "Bridge ON")
- Check Unity Console for error messages
- Verify internet connection
- Try restarting the Bridge server

### Missing Textures
**Problem**: Model imports but has no textures

**Solutions**:
- For FBX: Ensure ZIP contains texture files
- Check Unity Console for texture import logs
- Verify `Assets/MeshyImports/` folder has textures
- Manually reassign textures in Unity if needed

### Port Already in Use
**Problem**: "Address already in use" error

**Solutions**:
```bash
# macOS/Linux: Find process using port 5326
lsof -i :5326
kill -9 <PID>

# Windows: Find and kill process
netstat -ano | findstr :5326
taskkill /PID <PID> /F
```

## Technical Details

### Server Configuration
- **Port**: 5326 (TCP)
- **Protocol**: HTTP/1.1
- **CORS**: Enabled for Meshy domains only
- **Endpoints**:
  - `GET /status` - Server health check
  - `GET /ping` - Connection test
  - `POST /import` - Model import endpoint

### Allowed Origins
For security, the bridge only accepts requests from:
- `https://www.meshy.ai`
- `https://app-staging.meshy.ai`
- `http://localhost:3700` (development)

### Import Directory Structure
```
Assets/
└── MeshyImports/
    ├── ModelName_20241022_123456/
    │   ├── model.glb
    │   ├── texture_basecolor.png
    │   ├── texture_normal.png
    │   └── model_Controller.controller
    └── AnotherModel_20241022_123457/
        └── ...
```

## API Reference

### Import Request Format
```json
{
  "url": "https://cdn.meshy.ai/...",
  "format": "glb",
  "name": "MyModel",
  "frameRate": 30
}
```

### Import Response Format
```json
{
  "status": "ok",
  "message": "File queued for import",
  "path": "/tmp/Meshy/model_xyz.glb"
}
```

## Performance Considerations

- **File Size**: Large models (>100MB) may take longer to download
- **Texture Resolution**: 4K textures require more memory
- **Animation Clips**: Multiple clips increase import time
- **Concurrent Imports**: Process one model at a time for best results

## Privacy & Security

- ✅ Server runs **locally** on your machine only
- ✅ No data sent to external servers (except model download)
- ✅ Only accepts connections from whitelisted Meshy domains
- ✅ No API keys or credentials stored
- ✅ All communication uses HTTPS from Meshy servers

## Known Limitations

- Can only import one model at a time
- Requires internet connection for model download
- FBX texture mapping may need manual adjustment
- Extremely large files (>500MB) may timeout

## Developer Notes

### Code Structure
- `MeshyBridgeWindow`: Editor window UI
- `MeshyBridge`: TCP server and import queue
- `ProcessImportRequest`: HTTP request handler
- `ProcessMeshTransfer`: Format-specific import logic

### Extending the Plugin
The bridge can be extended to support:
- Custom import settings
- Additional file formats
- Post-import processing
- Material template system

See `Bridge.cs` for implementation details.

## Support

If you encounter any issues:
1. Check Unity Console for error messages
2. Enable Unity Developer Console for detailed logs
3. Report issues with:
   - Unity version
   - Model format and size
   - Complete error message
   - Steps to reproduce

---

For more information, visit [Meshy Documentation](https://docs.meshy.ai)
