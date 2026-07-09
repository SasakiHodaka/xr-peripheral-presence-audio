# Architecture

This document explains how the Unity prototype implements the research pipeline:

```text
Speech Object + Position + Conversation State
  -> Scene Parsing
  -> Scene Token
  -> Token-Based Spatial Audio Rendering
  -> Logs and Metrics
```

## Research Pipeline

```text
VR speaker avatars
  -> speaker-state acquisition
  -> Scene Parsing
  -> spatial-state analysis
  -> conversation-state analysis
  -> Scene Token generation
  -> Scene Token logging
  -> Scene Token decoding / rendering
  -> spatial audio output
```

In this prototype, `SceneTokenManager` performs the first rule-based Scene
Parsing implementation. It observes all speaker objects together, rather than
letting each avatar generate tokens independently. This design keeps turn-taking
and overlap detection centralized and makes later extensions such as addressee
estimation or LLM-based semantic parsing easier to add.

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
- Calls the Scene Parsing analyzers for direction, distance, speaking state,
  and turn state.
- Attaches semantic labels from the speaker object.
- Sends generated tokens to logger, renderer, metrics, and HUD.

Research mapping:

```text
Scene Parsing + Scene Token Generator
```

### DirectionAnalyzer

File:

```text
Assets/Scripts/SceneToken/DirectionAnalyzer.cs
```

Role:

- Computes listener-relative azimuth from listener and speaker transforms.
- Quantizes azimuth into 8 direction tokens.

Research mapping:

```text
Scene Parsing: spatial direction analysis
```

### DistanceAnalyzer

File:

```text
Assets/Scripts/SceneToken/DistanceAnalyzer.cs
```

Role:

- Computes horizontal listener-speaker range.
- Quantizes range into `NEAR`, `MID`, and `FAR`.

Research mapping:

```text
Scene Parsing: spatial distance analysis
```

### ConversationAnalyzer

File:

```text
Assets/Scripts/SceneToken/ConversationAnalyzer.cs
```

Role:

- Counts active speakers.
- Converts speaking state into `SPEAKING` or `SILENT`.
- Converts speaking overlap into `TURN_HOLDER`, `OVERLAPPER`, or `LISTENER`.

Research mapping:

```text
Scene Parsing: conversation-state analysis
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

### Participant Response Logging

File:

```text
Assets/Scripts/SceneToken/SceneTokenManager.cs
Assets/Scripts/SceneToken/SceneTokenExperimentSession.cs
```

Role:

- Records participant direction guesses and speaker guesses.
- Provides keyboard controls and response HUD buttons from `SceneTokenManager`.
- Writes responses into the event log as `response_direction` and
  `response_speaker` through `SceneTokenExperimentSession`.
- Uses `SceneTokenManager.LatestTokens` to attach the current target direction
  or target speaker and an `isCorrect` flag when exactly one speaker is active.
- Adds `responseLatency` from the start of the current trial.
- Marks responses as `ambiguous=true` when there is no active speaker or when
  multiple speakers overlap.

Controls:

- Arrow keys: cardinal direction response
- HUD buttons: all eight direction responses
- `J/K/L`: speaker A/B/C response

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

### SceneTokenAnalyzerSelfCheck

File:

```text
Assets/Editor/SceneTokenAnalyzerSelfCheck.cs
```

Role:

- Validates the rule-based Scene Parsing analyzers without requiring Play Mode.
- Checks direction quantization, distance quantization, turn-state
  quantization, listener-relative azimuth, and horizontal range.

Menu path:

```text
Tools/Semantic Spatial Audio/Run Scene Token Analyzer Self Check
Tools/Scene Tokens/Run Analyzer Self Check
```

Batch entry point:

```text
SceneTokenAnalyzerSelfCheck.RunForBatch
```

Research mapping:

```text
Scene Parsing rule validation
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
      -> DirectionAnalyzer
      -> DistanceAnalyzer
      -> ConversationAnalyzer
      -> semantic label attachment
  -> SceneToken[]
      -> SceneTokenLogger
      -> SceneTokenMetrics
      -> SceneTokenDecoderRenderer
  -> AudioSource control
  -> spatial audio output

SceneTokenManager response HUD / keys
  -> SceneTokenExperimentSession
  -> response_direction / response_speaker events
  -> SceneTokenEventLogger
```

## Render Conditions

### 1. C1_TRADITIONAL

Uses the original speaker object positions.

Purpose:

- Baseline spatial audio/object-position condition.

### 2. C2_DIRECTION_DISTANCE

Uses quantized direction and quantized distance.

Purpose:

- Tests the effect of adding distance tokenization.

### 3. C3_FULL_SCENE_TOKEN

Adds turn-state and semantic-token modulation.

Purpose:

- Tests the full proposed representation.

### 4. C4_SELECTED_SCENE_TOKEN

Adds priority-based Scene Token selection on top of the full representation.

Purpose:

- Tests whether semantic selection can preserve important utterances while
  reducing transmitted token volume.

### Development Ablations

The implementation also keeps `DIRECTION_ONLY` and
`DIRECTION_DISTANCE_SPEAKING` for development checks.

## What This Prototype Does Not Implement

- IVAS codec internals
- MASA bitstream coding
- real multi-user networking
- automatic speech recognition
- LLM-based semantic classification
- neural audio codec/audio token generation

These are future extensions after the controlled Scene Token baseline is
validated.
