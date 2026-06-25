# Project Status

Last organized: 2026-06-25

## Repository

- GitHub repository: `SasakiHodaka/xr-peripheral-presence-audio`
- Main branch contains the SemanticSpatialAudio replacement.
- Recent setup commits:
  - `611e183 Enable Meta XR Audio spatializer`
  - `5306641 Replace peripheral audio prototype with semantic spatial audio`

## Unity Version

- Target editor: Unity 2022.3.62f3
- Render pipeline: Built-in 3D
- Audio package: Meta XR Audio SDK package is listed in `Packages/manifest.json`.

## Validation

Confirmed on 2026-06-25:

- Unity Editor normal launch succeeded.
- Project import completed.
- Script compilation completed.
- No `error CS` or `CompilerError` entries were found in `Editor.log`.
- Unity automatically enabled:
  - Spatializer Plugin: `Meta XR Audio`
  - Ambisonic Decoder Plugin: `Meta XR Audio`

Known validation limitation:

- Unity batchmode validation returned code `199` because the Unity Licensing Client IPC channel timed out.
- This was a licensing service IPC issue in batchmode, not a script compilation error.
- Normal Unity Editor launch compiled the project successfully.

## Current Demo Scene

Primary scene:

`Assets/Scenes/SceneTokenMock.unity`

The scene contains:

- listener camera
- three mock speaker avatars
- `SceneTokenSystem`
- token logger
- event logger
- metrics logger
- decoder renderer
- condition controller
- experiment session controller

## Current Git State

Expected clean state after setup:

```bash
git status --short --branch
```

Expected output:

```text
## main...origin/main
```
