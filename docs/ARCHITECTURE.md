# Architecture

## Data Flow

```text
SpeakerObject
  -> SceneTokenManager
  -> SceneToken
  -> SceneTokenLogger
  -> SceneTokenDecoderRenderer
  -> SceneTokenMetrics
  -> SceneTokenExperimentSession
```

## Core Runtime Scripts

`SpeakerObject`

- Owns speaker identity, speaking state, semantic token, utterance text, and fallback tone generation.
- Configures its assigned `AudioSource` for spatial playback.
- Provides keyboard controls for mock trials.

`SceneToken`

- Serializable data model for one speaker at one timestamp.
- Contains discrete token fields and continuous analysis fields.
- Serializes itself to a CSV row.

`SceneTokenManager`

- Samples all speakers at a fixed interval.
- Computes azimuth, range, direction token, distance token, speaking token, turn state, and semantic token.
- Sends generated tokens to logger, renderer, metrics, and HUD.

`SceneTokenDecoderRenderer`

- Reconstructs audio rendering behavior from scene tokens.
- Supports five render conditions, from traditional position rendering to full token-based rendering.
- Modulates AudioSource position, volume, and pitch.

`SceneTokenLogger`

- Writes per-token CSV logs.
- Flushes periodically and on disable.

`SceneTokenMetrics`

- Estimates JSON-like, compact token, and object metadata bandwidth.
- Writes metrics CSV rows per summary window.

`SceneTokenEventLogger`

- Writes session, trial, and condition events.

`SceneTokenConditionController`

- Handles direct keyboard switching between render conditions.

`SceneTokenExperimentSession`

- Owns experiment session state.
- Tracks condition order, trial index, session ID, participant ID, and elapsed time.
- Supports timed or manual condition advancement.

`SpeakerDebugLabel`

- Shows speaker ID, semantic token, and speaking state above each avatar.

## Editor Tooling

`SceneTokenMockSceneWizard`

- Menu path:
  - `Tools/Semantic Spatial Audio/Create Scene Token Mock Scene`
  - `Tools/Scene Tokens/Create Spatial Conversation Mock Scene`
- Rebuilds the mock scene with floor, light, listener camera, three avatars, and the token system.

## Render Conditions

`TRADITIONAL`

- Uses the original speaker object positions.

`DIRECTION_ONLY`

- Uses quantized direction with a fixed middle radius.

`DIRECTION_DISTANCE`

- Uses quantized direction and quantized distance.

`DIRECTION_DISTANCE_SPEAKING`

- Adds speaking-state gating to suppress silent speakers.

`FULL_SCENE_TOKEN`

- Adds turn-state and semantic-token modulation.
