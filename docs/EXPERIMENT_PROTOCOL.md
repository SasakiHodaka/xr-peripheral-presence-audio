# Experiment Protocol

This document describes the current manual prototype workflow.

## Goal

Collect scene token logs and communication-volume metrics while switching
between different spatial audio reconstruction conditions.

## Scene

Open:

`Assets/Scenes/SceneTokenMock.unity`

## Controls

Speaker state:

- `A`: toggle speaker A speaking
- `B`: toggle speaker B speaking
- `C`: toggle speaker C speaking

Semantic state:

- `Q`: cycle speaker A semantic token
- `W`: cycle speaker B semantic token
- `E`: cycle speaker C semantic token

Rendering condition:

- `1`: traditional object position
- `2`: direction only
- `3`: direction + distance
- `4`: direction + distance + speaking state
- `5`: full scene token

Experiment session:

- `Space`: start or stop session
- `N`: advance to next condition
- `R`: restart session from the first condition

Scripted conversation:

- `T`: start or stop the deterministic conversation script
- `Y`: stop the deterministic conversation script
- The script starts automatically with an experiment session by default.

## Recommended Trial Flow

1. Open the scene.
2. Press Play.
3. Confirm the `Scene Tokens` HUD is visible.
4. Press `Space` to start the session.
5. Let the scripted conversation run, or press `T` to start it manually.
6. Confirm that the sequence includes:
   - one active speaker
   - a question or answer semantic token
   - a short overlap using two speakers
   - one warning or instruction token
7. Let timed condition advance occur, or press `N`.
8. Stop after the final condition or press `Space`.
9. Collect CSV logs from `Application.persistentDataPath`.

## Logs

Token logs:

`scene_tokens_<timestamp>.csv`

Columns:

`timestamp,speakerId,azimuth,range,direction,distance,speakingState,turnState,semanticToken,utteranceText,semanticConfidence,condition`

Metrics logs:

`scene_token_metrics_<timestamp>.csv`

Columns:

`timestamp,condition,tokensPerSecond,jsonBytesPerSecond,compactBytesPerSecond,objectMetadataBytesPerSecond,compactSavingsRatio`

Event logs:

`scene_token_events_<timestamp>.csv`

Columns:

`timestamp,eventType,value`

## Metrics Summary

Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
```

The script prints condition-level averages for:

- tokens per second
- JSON-like bytes per second
- compact scene token bytes per second
- object metadata bytes per second
- compact savings ratio

## Data Quality Checklist

Before treating a run as valid, confirm:

- all five conditions appear in the metrics CSV
- event log includes `session_start`, `trial_start`, and `trial_stop`
- event log includes `script_start`
- token log contains at least one `SPEAKING` row
- token log contains at least one non-`NONE` semantic token
- HUD condition matches the intended key or timed condition
