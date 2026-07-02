# Project Status

Last organized: 2026-06-25

## Repository

- GitHub repository: `SasakiHodaka/xr-peripheral-presence-audio`
- Main branch contains the SemanticSpatialAudio replacement.
- Current local Unity project path for implementation and validation:
  `C:\Users\acd-pc67\SemanticSpatialAudio`
- Older worktree copies, including
  `xr-peripheral-presence-audio/.worktrees/scene-token-prototype`, are reference
  copies unless changes are explicitly synchronized into `SemanticSpatialAudio`.
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
- Unity batch scene regeneration for `SceneTokenMockSceneWizard.CreateSpatialConversationMockScene` completed after the HUD/session update.
- No `error CS`, `Compilation failed`, `Build failed`, script compiler error, or exception entries were found in `unity_hud_session_recheck.log`.
- Unity batch validation with `SceneTokenSceneValidator.ValidateSceneForBatch` completed.
- `unity_scene_validator_recheck.log` reported `[SceneTokenValidation] Passed.`
- Token and metric logging are now gated by `SceneTokenExperimentSession.IsRunning`.
- Unity batch regeneration and validation after gated logging completed.
- `unity_session_gated_logging_validator.log` reported `[SceneTokenValidation] Passed.`

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
- HUD participant/session fields
- session start/stop/next/restart HUD buttons
- trial/session completion status feedback
- scene validation editor tool
- token and metric logging only during active experiment sessions

## Current Git State

Expected clean state after setup:

```bash
git status --short --branch
```

Expected output:

```text
## main...origin/main
```
