# SemanticSpatialAudio

Unity 2022.3 LTS project for a semantic spatial audio research prototype.

## Active Working Project

The active Unity project for current implementation and validation work is:

```text
C:\Users\acd-pc67\SemanticSpatialAudio
```

Older worktree copies, such as `xr-peripheral-presence-audio/.worktrees/scene-token-prototype`,
should be treated as reference copies only unless explicitly synchronized.

The older peripheral presence audio prototype has been replaced in this
repository. The current research target is spatial conversation tokenization:

`Speech Object + Position + Meaning/Turn State -> Scene Token -> Spatial Audio Rendering`

## Quick Start

1. Open this repository with Unity 2022.3.62f3 or another Unity 2022.3 LTS editor.
2. Open `Assets/Scenes/SceneTokenMock.unity`.
3. Run `Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check`.
4. Run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.
5. Press Play.
6. Use `Space` to start or stop an experiment session.
7. Use `N` to advance to the next condition, or wait for the timer.
8. Use `1`-`5` to switch evaluation conditions manually.
9. Use `T` to start or stop the scripted conversation sequence.
10. Use `Y` to stop the scripted conversation sequence.
11. Use `A`, `B`, or `C` to toggle each avatar's speaking state manually.
12. Use `Q`, `W`, or `E` to cycle each avatar's semantic token manually.
13. Record participant responses from the HUD buttons, or use:
    - arrow keys for `FRONT`, `RIGHT`, `BACK`, `LEFT`
    - `J`, `K`, `L` for speaker `A`, `B`, `C`

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
- direction and speaker response logging
- deterministic scripted conversation playback
- communication volume metrics
- HUD participant/session fields and completion feedback
- token and metric logging gated by experiment session state
- token-based AudioSource position, volume, and pitch reconstruction
- generated fallback tone when no recorded speaking clip is assigned

## Documentation

- `docs/PROJECT_STATUS.md`: current state, validation result, known issues
- `docs/EXPERIMENT_PROTOCOL.md`: how to run a trial and collect logs
- `docs/ARCHITECTURE.md`: script responsibilities and data flow
- `docs/SCENE_TOKEN_SPEC.md`: research definition, token fields, and design rationale
- `docs/RELATED_WORK_QA.md`: concise related-work comparison and defense Q&A
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
python Tools/check_latest_response_run.py <unity_log_directory>
python Tools/collect_latest_scene_token_run.py <unity_log_directory> Runs/latest_run
python Tools/analyze_scene_token_logs.py Runs/latest_run
python Tools/analyze_token_logs.py Runs/latest_run token_summary.csv
python Tools/analyze_event_logs.py Runs/latest_run event_summary.csv
python Tools/summarize_experiment_run.py Runs/latest_run summary.md
```

## Scene Validation

In Unity, run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.

Batch mode:

```bash
Unity.exe -batchmode -quit -projectPath <project> -executeMethod SceneTokenSceneValidator.ValidateSceneForBatch
```
