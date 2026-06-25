# Architecture

This document explains how the Unity prototype implements the research pipeline:

```text
Speech Object + Position + Conversation State
  -> Scene Token
  -> Token-Based Spatial Audio Rendering
  -> Logs and Metrics
```

## Research Pipeline

```text
VR speaker avatars
  -> speaker-state acquisition
  -> spatial-state analysis
  -> conversation-state analysis
  -> Scene Token generation
  -> Scene Token logging
  -> Scene Token decoding / rendering
  -> spatial audio output
```

## Unity Scene

Primary scene:

```text
Assets/Scenes/SceneTokenMock.unity
```

The mock scene contains:

- listener camera
- three speaker avatars
- `SceneTokenSystem`
- token logger
- event logger
- metrics logger
- condition controller
- experiment session controller
- scripted conversation controller
- debug HUD

## Component Responsibilities

### SpeakerObject

File:

```text
Assets/Scripts/SceneToken/SpeakerObject.cs
```

Role:

- Represents one speaker/avatar.
- Stores speaker ID, AudioSource, speaking state, semantic token, utterance text,
  and semantic confidence.
- Configures the AudioSource for spatial playback.
- Provides keyboard controls for manual mock trials.
- Generates a fallback tone if no voice clip is assigned.

Research mapping:

```text
Object-Based Audio speaker object
  -> source of speakerId, voice, and semantic label
```

### SceneToken

File:

```text
Assets/Scripts/SceneToken/SceneToken.cs
```

Role:

- Serializable data model for one speaker at one timestamp.
- Contains discrete token fields:
  - `direction`
  - `distance`
  - `speakingState`
  - `turnState`
  - `semanticToken`
- Keeps continuous analysis fields:
  - `azimuth`
  - `range`
- Serializes itself to CSV.

Research mapping:

```text
Formal Scene Token representation
```

### SceneTokenManager

File:

```text
Assets/Scripts/SceneToken/SceneTokenManager.cs
```

Role:

- Samples all speakers at a fixed interval.
- Computes listener-relative azimuth and range.
- Quantizes azimuth into 8 direction tokens.
- Quantizes range into 3 distance tokens.
- Computes speaking state and turn state.
- Attaches semantic labels from the speaker object.
- Sends generated tokens to logger, renderer, metrics, and HUD.

Research mapping:

```text
Scene Token Generator
```

### SceneTokenDecoderRenderer

File:

```text
Assets/Scripts/SceneToken/SceneTokenDecoderRenderer.cs
```

Role:

- Reconstructs audio rendering behavior from Scene Tokens.
- Decodes direction into AudioSource position.
- Decodes distance into radius and volume.
- Uses speaking state to suppress or emphasize speakers.
- Uses turn state and semantic token for volume and pitch modulation.

Research mapping:

```text
Scene Token -> Spatial Audio Rendering
```

### SceneTokenConditionController

File:

```text
Assets/Scripts/SceneToken/SceneTokenConditionController.cs
```

Role:

- Switches between rendering conditions with keys `1` to `5`.
- Updates the renderer's condition.

Research mapping:

```text
Experimental condition control
```

### SceneTokenExperimentSession

File:

```text
Assets/Scripts/SceneToken/SceneTokenExperimentSession.cs
```

Role:

- Controls experiment session state.
- Tracks participant ID, session ID, trial index, condition order, and elapsed time.
- Starts and stops trials.
- Supports timed or manual condition advancement.
- Writes session and trial events through `SceneTokenEventLogger`.

Research mapping:

```text
Controlled experiment workflow
```

### SceneTokenScriptedConversation

File:

```text
Assets/Scripts/SceneToken/SceneTokenScriptedConversation.cs
```

Role:

- Runs a deterministic sequence of utterances.
- Sets active speaker, duration, semantic token, utterance text, and confidence.
- Includes question, answer, instruction, warning, and agreement labels.
- Can start automatically with an experiment session.

Research mapping:

```text
Controlled conversation scenario for repeatable evaluation
```

### SceneTokenResponseRecorder

File:

```text
Assets/Scripts/SceneToken/SceneTokenResponseRecorder.cs
```

Role:

- Records participant direction guesses and speaker guesses.
- Provides keyboard controls and a small response HUD.
- Writes responses into the event log as `response_direction` and
  `response_speaker`.
- Uses `SceneTokenManager.LatestTokens` to attach the current target direction
  or target speaker and an `isCorrect` flag when exactly one speaker is active.
- Adds `responseLatency` from the start of the current single-speaker target.
- Marks responses as `ambiguous=true` when there is no active speaker or when
  multiple speakers overlap.

Controls:

- Numpad `7/8/9/4/6/1/2/3`: direction response
- `F1/F2/F3`: speaker response

Research mapping:

```text
Participant response logging for objective evaluation
```

### SceneTokenSceneValidator

File:

```text
Assets/Editor/SceneTokenSceneValidator.cs
```

Role:

- Validates whether the mock scene has the required Scene Token components.
- Checks manager, renderer, logger, metrics, condition controller, experiment
  session, scripted conversation, response recorder, and speaker wiring.
- Can be run from the Unity editor menu or from batchmode.

Menu path:

```text
Tools/Semantic Spatial Audio/Validate Scene Token Mock Scene
Tools/Scene Tokens/Validate Spatial Conversation Mock Scene
```

Batch entry point:

```text
SceneTokenSceneValidator.ValidateSceneForBatch
```

Research mapping:

```text
Pre-experiment scene wiring validation
```

### SceneTokenLogger

File:

```text
Assets/Scripts/SceneToken/SceneTokenLogger.cs
```

Role:

- Writes per-token CSV logs.
- Flushes rows periodically and on disable.

Research mapping:

```text
Token sequence data for analysis
```

### SceneTokenEventLogger

File:

```text
Assets/Scripts/SceneToken/SceneTokenEventLogger.cs
```

Role:

- Writes session, trial, condition, and scripted conversation events.

Research mapping:

```text
Experiment timeline data
```

### SceneTokenMetrics

File:

```text
Assets/Scripts/SceneToken/SceneTokenMetrics.cs
```

Role:

- Estimates communication volume for:
  - JSON-like token format
  - compact Scene Token format
  - object metadata format
- Writes summary metrics per time window.

Research mapping:

```text
Secondary communication-volume analysis
```

### SpeakerDebugLabel

File:

```text
Assets/Scripts/UI/SpeakerDebugLabel.cs
```

Role:

- Shows speaker ID, semantic token, and speaking state above each avatar.

Research mapping:

```text
Debug visualization only
```

## Runtime Data Flow

```text
SpeakerObject A/B/C
  -> SceneTokenManager
      -> direction quantization
      -> distance quantization
      -> speaking-state analysis
      -> turn-state analysis
      -> semantic label attachment
  -> SceneToken[]
      -> SceneTokenLogger
      -> SceneTokenMetrics
      -> SceneTokenDecoderRenderer
  -> AudioSource control
  -> spatial audio output

SceneTokenResponseRecorder
  -> response_direction / response_speaker events
  -> SceneTokenEventLogger
```

## Render Conditions

### 1. TRADITIONAL

Uses the original speaker object positions.

Purpose:

- Baseline spatial audio/object-position condition.

### 2. DIRECTION_ONLY

Uses quantized direction with a fixed middle radius.

Purpose:

- Tests the effect of direction tokenization alone.

### 3. DIRECTION_DISTANCE

Uses quantized direction and quantized distance.

Purpose:

- Tests the effect of adding distance tokenization.

### 4. DIRECTION_DISTANCE_SPEAKING

Adds speaking-state gating to suppress silent speakers.

Purpose:

- Tests the effect of explicitly representing who is speaking.

### 5. FULL_SCENE_TOKEN

Adds turn-state and semantic-token modulation.

Purpose:

- Tests the full proposed representation.

## What This Prototype Does Not Implement

- IVAS codec internals
- MASA bitstream coding
- real multi-user networking
- automatic speech recognition
- LLM-based semantic classification
- neural audio codec/audio token generation

These are future extensions after the controlled Scene Token baseline is
validated.
