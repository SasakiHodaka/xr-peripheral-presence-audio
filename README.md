# SemanticSpatialAudio

Unity 2022.3 LTS project for a semantic spatial audio research prototype.

The older peripheral presence audio prototype has been replaced in this
repository. The current research target is spatial conversation tokenization:

`Speech Object + Position + Meaning/Turn State -> Scene Token -> Spatial Audio Rendering`

## Quick Start

1. Open this repository with Unity 2022.3.62f3 or another Unity 2022.3 LTS editor.
2. Open `Assets/Scenes/SceneTokenMock.unity`.
3. Press Play.
4. Use `A`, `B`, or `C` to toggle each avatar's speaking state.
5. Use `Q`, `W`, or `E` to cycle each avatar's semantic token.
6. Use `1`-`5` to switch evaluation conditions.
7. Use `Space` to start or stop an experiment session.
8. Use `N` to advance to the next condition, or wait for the timer.

## Current Prototype

The mock scene currently supports:

- 8-direction scene tokens
- 3-level distance tokens
- speaking state
- simple turn state
- manual semantic labels
- visible avatar state labels
- CSV token logging
- event logging for experiment sessions
- communication volume metrics
- token-based AudioSource position, volume, and pitch reconstruction
- generated fallback tone when no recorded speaking clip is assigned

## Documentation

- `docs/PROJECT_STATUS.md`: current state, validation result, known issues
- `docs/EXPERIMENT_PROTOCOL.md`: how to run a trial and collect logs
- `docs/ARCHITECTURE.md`: script responsibilities and data flow
- `docs/NEXT_STEPS.md`: recommended next development tasks
- `Assets/Scripts/SceneToken/README_SceneTokens.md`: implementation-level notes

## Repository Layout

- `Assets/Editor`: Unity editor tooling and scene wizard
- `Assets/Scenes`: Unity scenes
- `Assets/Scripts/SceneToken`: token model, manager, logger, decoder, metrics
- `Assets/Scripts/UI`: debug labels and UI helpers
- `Assets/Audio`: optional voice clips
- `Assets/Data`: sample metadata and analysis data
- `Assets/Prefabs`: reusable scene objects
- `Packages`: Unity package manifest and lock file
- `ProjectSettings`: Unity project settings
- `Tools`: analysis scripts

## Log Analysis

Metric logs are written to Unity's `Application.persistentDataPath`.

Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
```
