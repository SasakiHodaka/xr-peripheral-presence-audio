# Scene Token Specification

## Definition

Scene Token is a discrete semantic unit that represents both the spatial state
and the conversational state of a speaker in a VR communication scene.

Scene Token is designed for semantic spatial voice communication. Its purpose is
not only to reconstruct where a sound should be heard from, but also to support
understanding of who is speaking, where they are, and what role they have in the
conversation.

## Design Position

Existing spatial audio communication methods mainly represent:

- speech signal
- direction
- distance or object position
- spatial rendering parameters

This project extends that idea from spatial metadata to scene metadata:

```text
Speech Object + Spatial Metadata + Conversation State -> Scene Token
```

The core distinction is:

```text
MASA / IVAS: How should the sound be reproduced?
Scene Token: What is happening in the communication scene?
```

## Current Token Structure

Minimum token:

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER"
}
```

Extended token used in the prototype:

```json
{
  "speakerId": "A",
  "sessionId": "20260625_161500",
  "participantId": "P01",
  "trialIndex": 1,
  "trialElapsed": 12.50,
  "azimuth": 42.8,
  "range": 1.24,
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER",
  "semanticToken": "QUESTION",
  "utteranceText": "Can you check this object for me?",
  "semanticConfidence": 1.0,
  "condition": "FULL_SCENE_TOKEN",
  "timestamp": 12.50
}
```

Discrete fields are the main Scene Token representation. Continuous fields such
as `azimuth` and `range` are stored for analysis and comparison with
object-based metadata.

## Field Definitions

### speakerId

Purpose:

- Represents which avatar or speaker the token describes.

Current source:

- `SpeakerObject.speakerId`

Research role:

- Preserves speaker identity in an object-based communication scene.
- Allows logs and analysis to distinguish multiple simultaneous speakers.

### direction

Purpose:

- Represents the speaker direction relative to the listener's head direction.

Values:

- `FRONT`
- `FRONT_RIGHT`
- `RIGHT`
- `BACK_RIGHT`
- `BACK`
- `BACK_LEFT`
- `LEFT`
- `FRONT_LEFT`

Current source:

- listener transform
- speaker transform
- relative azimuth quantized into 8 classes

Design reason:

- Direction is a core spatial metadata element in spatial audio coding.
- 8 classes are understandable in user experiments and stable for a first
  prototype.

### distance

Purpose:

- Represents the speaker distance relative to the listener.

Values:

- `NEAR`: less than 1.5 m
- `MID`: 1.5 m to less than 3.0 m
- `FAR`: 3.0 m or more

Current source:

- horizontal range between listener and speaker

Design reason:

- Coarse distance categories are easier to evaluate than continuous distance in
  the first VR conversation experiment.
- Continuous range is still logged for later analysis.

### speakingState

Purpose:

- Represents whether the speaker is currently speaking.

Values:

- `SILENT`
- `SPEAKING`

Current source:

- `SpeakerObject.IsSpeaking`
- AudioSource playback state or scripted/manual speaking state

Design reason:

- In multi-speaker VR communication, knowing who is currently speaking is
  directly related to turn tracking and conversation comprehension.

### turnState

Purpose:

- Represents the speaker's current conversational turn status.

Values:

- `LISTENER`
- `TURN_HOLDER`
- `OVERLAPPER`

Current source:

- if the speaker is not speaking: `LISTENER`
- if exactly one speaker is speaking: `TURN_HOLDER`
- if multiple speakers are speaking: `OVERLAPPER`

Design reason:

- This is the minimum useful turn-taking representation that can be generated
  reliably without speech recognition.
- Richer labels such as questioner, answerer, instructor, and warning speaker
  are represented by `semanticToken` in the current prototype.

### semanticToken

Purpose:

- Represents a manually assigned utterance-level meaning label.

Values:

- `NONE`
- `QUESTION`
- `ANSWER`
- `INSTRUCTION`
- `AGREEMENT`
- `DISAGREEMENT`
- `CHAT`
- `WARNING`

Current source:

- manual or scripted label in `SpeakerObject.semanticToken`

Design reason:

- Manual labels make the first evaluation controllable.
- Automatic classification using ASR and LLMs should be introduced only after
  the token and rendering baseline is stable.

### timestamp

Purpose:

- Represents the Unity time when the token was generated.

Current source:

- `Time.time`

Research role:

- Enables token sequence analysis, response-time analysis, and synchronization
  with event logs.

### sessionId, participantId, trialIndex, trialElapsed

Purpose:

- Stores experiment metadata with every token row.
- Makes it possible to merge logs across participants, sessions, and trials.

Current source:

- `SceneTokenExperimentSession`

Research role:

- Supports condition-level and participant-level analysis.

## Generation Pipeline

```text
SpeakerObject[]
  -> listener-relative position analysis
  -> direction quantization
  -> distance quantization
  -> speaking-state analysis
  -> turn-state analysis
  -> semantic label attachment
  -> SceneToken[]
```

Current implementation:

- `Assets/Scripts/SceneToken/SceneTokenManager.cs`

## Rendering Pipeline

```text
SceneToken[]
  -> decode direction into AudioSource position
  -> decode distance into radius and volume
  -> decode speakingState into playback emphasis or suppression
  -> decode turnState and semanticToken into volume/pitch modulation
  -> spatial audio output
```

Current implementation:

- `Assets/Scripts/SceneToken/SceneTokenDecoderRenderer.cs`

## Evaluation Conditions

The prototype supports five conditions:

1. `TRADITIONAL`
   - Uses the original speaker object positions.
2. `DIRECTION_ONLY`
   - Uses quantized direction with a fixed radius.
3. `DIRECTION_DISTANCE`
   - Uses quantized direction and quantized distance.
4. `DIRECTION_DISTANCE_SPEAKING`
   - Adds speaking-state gating.
5. `FULL_SCENE_TOKEN`
   - Adds turn-state and semantic-token modulation.

These conditions make it possible to test whether each token layer contributes
to speaker localization, speaker identification, and conversation
understanding.

## Current Research Claim

The minimum claim is:

```text
Scene Token integrates spatial information and conversation-state information
into a discrete representation for VR spatial voice communication. Compared
with spatial metadata alone, it can support not only sound reproduction but also
conversation understanding.
```

Communication-volume reduction should not be the main claim until experimental
data is sufficient. It should be treated as a secondary analysis.

## Future Extensions

Possible future fields:

- emotion
- attention target
- gaze target
- addressee
- gesture
- urgency
- classifier confidence

Possible automatic generation:

```text
Microphone audio -> ASR -> LLM or classifier -> semanticToken
```

The current manual/scripted semantic labels should be presented as a controlled
baseline.
