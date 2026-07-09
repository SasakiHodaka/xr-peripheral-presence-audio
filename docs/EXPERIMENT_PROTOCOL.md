# Experiment Protocol

This document describes the current prototype workflow and the planned
evaluation structure.

## Experiment Goal

The prototype is designed to compare spatial audio rendering conditions with
different levels of Scene Token information.

The main question is:

```text
Does adding speaking-state, turn-state, and semantic-token information to
spatial audio help users understand a multi-speaker VR conversation?
```

## Scene

Open:

```text
Assets/Scenes/SceneTokenMock.unity
```

The scene contains three speaker avatars and one listener camera.

## Controls

### Speaker State

- `A`: toggle speaker A speaking
- `B`: toggle speaker B speaking
- `C`: toggle speaker C speaking

### Semantic State

- `Q`: cycle speaker A semantic token
- `W`: cycle speaker B semantic token
- `E`: cycle speaker C semantic token

### Rendering Condition

- `1`: `C1_TRADITIONAL`
- `2`: `C2_DIRECTION_DISTANCE`
- `3`: `C3_FULL_SCENE_TOKEN`
- `4`: `C4_SELECTED_SCENE_TOKEN`
- `5`: `DIRECTION_ONLY` optional ablation
- `6`: `DIRECTION_DISTANCE_SPEAKING` optional ablation

### Experiment Session

- `Space`: start or stop session
- `N`: advance to next condition
- `R`: restart session from the first condition

### Scripted Conversation

- `T`: start or stop the deterministic conversation script
- `Y`: stop the deterministic conversation script
- The script starts automatically with an experiment session by default.

### Participant Response

- Respond when the HUD shows `RESPOND NOW`.
- Wait when the HUD shows `WAIT`; responses during this state are still logged
  but are marked as ambiguous.
- Arrow Up: FRONT
- Arrow Right: RIGHT
- Arrow Down: BACK
- Arrow Left: LEFT
- `J`: speaker A
- `K`: speaker B
- `L`: speaker C
- Use the response HUD buttons for all eight directions:
  `FRONT`, `FRONT_RIGHT`, `RIGHT`, `BACK_RIGHT`, `BACK`, `BACK_LEFT`,
  `LEFT`, and `FRONT_LEFT`.
- The event log records `response_window_start` and `response_window_end`
  events when a single active-speaker response window opens or closes.

## Current Prototype Trial Flow

1. Open `Assets/Scenes/SceneTokenMock.unity`.
2. Run `Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check`.
3. Run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.
4. Press Play.
5. Confirm that the `Scene Tokens` HUD is visible.
6. Press `Space` to start the experiment session.
7. Let the scripted conversation run.
8. Confirm that the sequence includes:
   - one active speaker
   - question or answer semantic token
   - short overlap using two speakers
   - warning or instruction token
9. During each condition, record at least one direction guess and one speaker
   guess using the response keys or HUD buttons.
   - Prefer moments where the HUD shows `RESPOND NOW`.
   - Responses during no-speaker or overlap moments are logged as
     `ambiguous=true` and are excluded from scored accuracy.
10. Let timed condition advancement occur, or press `N`.
11. Stop after the final condition, or press `Space`.
12. Collect CSV logs from Unity's `Application.persistentDataPath`.

## Editor Validation

Before collecting logs, run both editor checks:

```text
Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check
Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene
```

The analyzer self check validates:

- 8-direction quantization
- 3-level distance quantization
- speaking-state and turn-state quantization
- listener-relative azimuth and horizontal range calculation

The scene validation checks whether `SceneTokenMock.unity` has the required
manager, renderer, logger, metrics, experiment, response, and speaker
components wired correctly.

## Rendering Conditions

### Condition 1: C1_TRADITIONAL

Uses original object positions.

Purpose:

- Baseline spatial audio rendering.

### Condition 2: C2_DIRECTION_DISTANCE

Uses quantized direction and distance.

Purpose:

- Spatial audio baseline using listener-relative direction and distance.

### Condition 3: C3_FULL_SCENE_TOKEN

Uses direction, distance, speaking state, turn state, and semantic token.

Purpose:

- Tests the full proposed Scene Token representation.

### Condition 4: C4_SELECTED_SCENE_TOKEN

Uses the full Scene Token representation with priority-based token selection.

Purpose:

- Tests whether semantic selection can keep important utterances while reducing
  transmitted token volume.

### Optional Ablation Conditions

`DIRECTION_ONLY` and `DIRECTION_DISTANCE_SPEAKING` are retained in the
implementation for development and ablation checks, but they are not required
for the main user study.

## Logs

### Token Logs

File pattern:

```text
scene_tokens_<timestamp>.csv
```

Columns:

```text
timestamp,
sessionId,
participantId,
trialIndex,
trialElapsed,
speakerId,
azimuth,
range,
direction,
distance,
speakingState,
turnState,
semanticToken,
urgency,
targetObjectId,
utteranceText,
semanticConfidence,
priority,
selectedForTransmission,
selectionReason,
condition
```

Purpose:

- Analyzes generated token sequences.
- Checks whether speaking, turn, and semantic labels appeared correctly.
- Supports later behavioral or comprehension analysis.

### Metrics Logs

File pattern:

```text
scene_token_metrics_<timestamp>.csv
```

Columns:

```text
timestamp,
sessionId,
participantId,
trialIndex,
trialElapsed,
condition,
tokensPerSecond,
jsonBytesPerSecond,
compactBytesPerSecond,
objectMetadataBytesPerSecond,
compactSavingsRatio,
generatedTokensPerSecond,
selectedTokensPerSecond,
selectedJsonBytesPerSecond,
selectedCompactBytesPerSecond,
tokenDropRatio,
importantTokenSendRatio,
selectionSavingsRatio
```

Purpose:

- Estimates communication volume.
- Compares compact Scene Token format with object metadata.
- Measures optional priority-based Scene Token selection when
  `SceneTokenManager.enableTokenSelection` is enabled.
- This is a secondary communication evaluation, separate from the main user
  study conditions.

### Event Logs

File pattern:

```text
scene_token_events_<timestamp>.csv
```

Columns:

```text
timestamp,eventType,value
```

Purpose:

- Tracks session start/stop.
- Tracks trial start/stop.
- Tracks condition changes.
- Tracks scripted conversation start/stop.

### Packet Logs

File pattern:

```text
scene_packets_<timestamp>.csv
```

Purpose:

- Tracks the generated token count and selected token count per packet.
- Tracks dropped tokens, important-token retention, and estimated packet bytes.
- Supports the secondary communication-cost analysis.

## Log Analysis

Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
python Tools/analyze_scene_packet_logs.py <scene_packets_csv_or_log_directory>
python Tools/analyze_token_logs.py <scene_tokens_csv_or_log_directory>
python Tools/analyze_event_logs.py <scene_token_events_csv_or_log_directory>
python Tools/summarize_experiment_run.py <log_directory> [summary.md]
```

The metrics script summarizes:

- tokens per second
- JSON-like bytes per second
- compact Scene Token bytes per second
- object metadata bytes per second
- compact savings ratio
- selected token throughput and selection savings ratio when token selection is enabled

## Communication-Only Token Selection Check

Token selection is not a separate user-study condition. To evaluate it as a
communication feature:

1. Use the same scripted conversation.
2. Run once with `SceneTokenManager.enableTokenSelection = false`.
3. Run once with `SceneTokenManager.enableTokenSelection = true`.
4. Compare `generatedTokensPerSecond`, `selectedTokensPerSecond`,
   `tokenDropRatio`, `importantTokenSendRatio`, and `selectionSavingsRatio`.

The expected result is that important tokens remain transmitted while
low-priority tokens are reduced.

The token script summarizes:

- token rows per condition
- speaking row ratio
- semantic-token row ratio
- turn-holder rows
- overlap rows
- observed speakers
- most frequent direction, distance, turn state, and semantic token
- basic data quality checks

The event script summarizes:

- event counts by condition
- session and trial events
- direction responses
- speaker responses
- direction response accuracy when the current target is not ambiguous
- speaker response accuracy when the current target is not ambiguous
- average response latency for direction and speaker responses
- response-log quality checks

The packet script summarizes:

- packets per second
- estimated bytes per second
- selected tokens per packet
- token drop ratio
- important-token kept ratio

The summary script writes a Markdown report with:

- quality checks
- token summary
- response accuracy and latency
- communication metrics
- packet metrics
- weekly-report draft text

## Data Quality Checklist

Before treating a run as valid, confirm:

- all planned conditions appear in the metrics CSV:
  `C1_TRADITIONAL`, `C2_DIRECTION_DISTANCE`, `C3_FULL_SCENE_TOKEN`,
  and optionally `C4_SELECTED_SCENE_TOKEN`
- event log includes `session_start`
- event log includes `trial_start`
- event log includes `trial_stop`
- event log includes `script_start`
- token log contains at least one `SPEAKING` row
- token log contains at least one non-`NONE` semantic token
- token log contains at least one `TURN_HOLDER` row
- token log contains at least one `OVERLAPPER` row if overlap is part of the trial
- event log contains at least one `response_direction` row if direction responses were collected
- event log contains at least one `response_speaker` row if speaker responses were collected
- response rows include `expected`, `isCorrect`, `ambiguous`, and `responseLatency` fields
- HUD condition matches the intended condition
- packet log is present when evaluating communication selection

## Planned User Study Metrics

### Objective Metrics

- speaker localization accuracy
- speaker identification time
- active speaker recognition accuracy
- turn/overlap recognition accuracy
- conversation comprehension score
- task completion time if a collaborative task is added

### Subjective Metrics

- conversation understanding
- ease of identifying the active speaker
- ease of following turn changes
- naturalness of spatial audio
- perceived workload, such as NASA-TLX
- usefulness of semantic emphasis

## Planned Hypotheses

H1:

```text
Direction and distance tokens improve localization compared with direction-only
or traditional rendering.
```

H2:

```text
Speaking-state tokens improve active-speaker identification.
```

H3:

```text
Full Scene Token rendering improves conversation understanding compared with
spatial metadata-only rendering.
```

H4:

```text
Compact Scene Token representation may reduce metadata volume compared with
object metadata, but this remains a secondary analysis.
```

## Current Limitation

The current prototype uses manual or scripted semantic labels. This is suitable
for a controlled first experiment, but automatic semantic-token generation using
ASR or LLMs remains future work.
