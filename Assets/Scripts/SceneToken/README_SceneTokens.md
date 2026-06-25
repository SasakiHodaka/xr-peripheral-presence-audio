# Scene Token Scripts

This folder contains the Unity runtime implementation for the Scene Token
prototype.

For research-level explanation, read:

- `docs/RESEARCH_OVERVIEW.md`
- `docs/SCENE_TOKEN_SPEC.md`
- `docs/RELATED_WORK_QA.md`

## Runtime Flow

```text
SpeakerObject
  -> SceneTokenManager
  -> SceneToken[]
  -> SceneTokenLogger
  -> SceneTokenMetrics
  -> SceneTokenDecoderRenderer
  -> AudioSource output
```

## Main Scripts

`SceneToken.cs`

- Defines the token fields and CSV serialization.

`SpeakerObject.cs`

- Represents one speaker/avatar.
- Stores ID, speaking state, semantic token, utterance text, and AudioSource.

`SceneTokenManager.cs`

- Samples all speakers.
- Computes direction, distance, speaking state, turn state, and semantic token.
- Sends tokens to logging, metrics, rendering, and HUD.

`SceneTokenDecoderRenderer.cs`

- Decodes token fields into AudioSource position, volume, and pitch.

`SceneTokenConditionController.cs`

- Switches between experimental rendering conditions.

`SceneTokenExperimentSession.cs`

- Controls session, trial, condition order, and event logging.

`SceneTokenScriptedConversation.cs`

- Runs a repeatable three-speaker scripted conversation.

`SceneTokenLogger.cs`

- Writes per-token CSV rows.

`SceneTokenEventLogger.cs`

- Writes session/trial/script events.

`SceneTokenMetrics.cs`

- Estimates JSON-like, compact token, and object metadata communication volume.

## Current Token Fields

CSV columns:

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
utteranceText,
semanticConfidence,
condition
```

Discrete fields are the research token representation. Continuous fields are
kept for analysis and comparison with object-based metadata.

## Current Scope

- Direction: 8 classes
- Distance: `NEAR`, `MID`, `FAR`
- Speaking state: keyboard, scripted state, or AudioSource playback
- Turn state: `LISTENER`, `TURN_HOLDER`, `OVERLAPPER`
- Semantic token: manual/scripted label first, ASR/LLM later
- Rendering: token direction and distance decoded into AudioSource position and volume
- Metrics: JSON-style bytes/s, compact token bytes/s, object metadata bytes/s

If `SpeakerObject.speakingClip` is empty, the component generates a simple
looping tone so spatial playback can be tested without recorded voice files.
