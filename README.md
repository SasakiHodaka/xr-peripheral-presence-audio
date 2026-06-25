# SemanticSpatialAudio

Unity 2022.3 LTS / 3D Built-in Render Pipeline project for the semantic spatial audio research prototype.

## Research Scope

This project is separated from the older peripheral presence audio work.
The goal here is spatial conversation tokenization:

`Speech Object + Position + Meaning/Turn State -> Scene Token -> Spatial Audio Rendering`

## Folder Layout

- `Assets/Scripts/Core`: shared project logic
- `Assets/Scripts/SceneToken`: token model, generator, logger, decoder
- `Assets/Scripts/Audio`: audio-specific components
- `Assets/Scripts/UI`: debug and experiment UI
- `Assets/Scenes`: Unity scenes
- `Assets/Data`: CSV logs, sample metadata, analysis data
- `Assets/Audio`: voice clips
- `Assets/Prefabs`: avatars and reusable objects

## First Demo

1. Open this folder with Unity 2022.3.62f3 or another 2022.3 LTS editor.
2. Open `Assets/Scenes/SceneTokenMock.unity`.
3. Press Play.
4. Press `A`, `B`, or `C` to toggle each avatar's speaking state.
5. Press `Q`, `W`, or `E` to cycle each avatar's semantic token.
6. Press `1`-`5` to switch evaluation conditions.
7. Press `Space` to start/stop an experiment session.
8. Press `N` to advance to the next condition, or wait for the timer.
9. Check the `Scene Tokens` debug HUD.

The current prototype generates:

- 8-direction token
- 3-level distance token
- speaking state
- simple turn state
- manual semantic token
- CSV log
- communication volume metrics
- visible avatar state labels
- token-based AudioSource position and volume reconstruction
- experiment session events with condition order and trial timing

If `SpeakerObject.speakingClip` is empty, the component generates a simple looping tone so spatial playback can be tested without recorded voice files.

## Log Analysis

Metric logs are written to Unity's `Application.persistentDataPath`.

Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
```
