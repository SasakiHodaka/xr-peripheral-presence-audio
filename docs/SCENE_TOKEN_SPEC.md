# Scene Token Specification v1.0

## Definition

Scene Token is the minimum communication unit that integrates spatial
information and semantic information required for situation understanding in a
VR communication scene, and controls spatial audio reconstruction on the
receiver side.

Scene Token is designed for semantic spatial voice communication. Its purpose is
not only to reconstruct where a sound should be heard from, but also to support
understanding of who is speaking, where they are, and what role they have in the
conversation.

In this thesis, Scene Token means:

```text
A Scene Token is a minimum communication unit that describes who is speaking,
where the speaker is located relative to the listener, what the utterance means,
how urgent it is, and which target object it refers to, so that the receiver can
reconstruct semantic spatial audio.
```

Japanese definition:

```text
Scene Tokenとは，VR空間における状況理解に必要な空間情報と意味情報を統合し，
受信側で空間音声を再構成するための最小通信単位である．
```

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

This project uses the idea of scene tokenization from vision research as a
reference, but redefines Scene Token for VR spatial voice communication.

```text
Vision scene tokenization:
image / scene -> objects, attributes, relations

Proposed VR voice scene tokenization:
VR conversation scene -> speakers, spatial states, speaking states, turn states,
and semantic roles
```

## Scene Parsing Definition

Scene Parsing is the process that converts the current VR communication scene
into Scene Tokens.

Input:

- avatar or speaker ID
- avatar position
- listener position
- listener head direction
- audio playback or scripted speaking state
- conversation script or manual semantic label

Processing:

- estimate listener-relative azimuth
- quantize azimuth into `Direction`
- estimate listener-relative distance
- quantize distance into `Distance`
- estimate `SpeakingState`
- estimate `TurnState` from the number of active speakers
- attach `SemanticToken` from the controlled script or manual label

Output:

- one Scene Token per speaker at each token update time

Pipeline:

```text
VR Space
  -> Scene Parsing
  -> SceneToken[]
  -> optional priority-based Scene Token selection
  -> token logging / communication
  -> token-based spatial audio rendering
```

In the current implementation, Scene Parsing is implemented mainly in:

```text
Assets/Scripts/SceneToken/SceneTokenManager.cs
```

The current parsing algorithm is rule-based so that the first experiment remains
controlled and reproducible. ASR or LLM-based semantic parsing is future work.

## Token Structure v1.0

Minimum research token:

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER"
}
```

Formal field table:

| Field | Meaning | Values in v1.0 | Generation rule |
| --- | --- | --- | --- |
| `speakerId` | Speaker identity | `A`, `B`, `C` | Assigned to each `SpeakerObject` |
| `direction` | Direction from listener to speaker | `FRONT`, `FRONT_RIGHT`, `RIGHT`, `BACK_RIGHT`, `BACK`, `BACK_LEFT`, `LEFT`, `FRONT_LEFT` | Quantize relative azimuth into 8 directions |
| `distance` | Distance from listener to speaker | `NEAR`, `MID`, `FAR` | `NEAR < 1.5 m`, `MID < 3.0 m`, otherwise `FAR` |
| `speakingState` | Whether the speaker is speaking | `SILENT`, `SPEAKING` | `SpeakerObject.IsSpeaking` or scripted state |
| `turnState` | Conversational turn role | `LISTENER`, `TURN_HOLDER`, `OVERLAPPER` | Silent speaker is `LISTENER`; one active speaker is `TURN_HOLDER`; multiple active speakers are `OVERLAPPER` |
| `semanticToken` | Utterance-level semantic role | `NONE`, `QUESTION`, `ANSWER`, `INSTRUCTION`, `AGREEMENT`, `DISAGREEMENT`, `CHAT`, `WARNING`, `EMERGENCY` | Manual or scripted label |
| `urgency` | Urgency of the utterance | `LOW`, `MEDIUM`, `HIGH`, `CRITICAL` | Manual or scripted label |
| `targetObjectId` | Object or equipment referred to by the utterance | e.g., `Pump3`, `Valve2` | Manual or scripted label |
| `priority` | Transmission priority | `0.0` to `1.0` | Rule-based score from speaking state, semantic token, urgency, and overlap |
| `selectedForTransmission` | Whether the token is transmitted after optional selection | `true`, `false` | Priority-based Scene Token selection |

Analysis fields:

| Field | Meaning |
| --- | --- |
| `azimuth` | Continuous listener-relative direction in degrees |
| `range` | Continuous listener-relative horizontal distance in meters |
| `timestamp` | Unity time when the token was generated |
| `sessionId`, `participantId`, `trialIndex`, `trialElapsed` | Experiment metadata |
| `utteranceText`, `semanticConfidence`, `condition` | Script/evaluation metadata |

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
  "urgency": "MEDIUM",
  "targetObjectId": "Pump3",
  "priority": 0.70,
  "selectedForTransmission": true,
  "utteranceText": "Can you check this object for me?",
  "semanticConfidence": 1.0,
  "condition": "FULL_SCENE_TOKEN",
  "timestamp": 12.50
}
```

Discrete fields are the main Scene Token representation. Continuous fields such
as `azimuth` and `range` are stored for analysis and comparison with
object-based metadata.

## Field Definitions and Rules

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

## Scene Token Selection

The base prototype can transmit every generated Scene Token. As an extension,
the current implementation can enable priority-based Scene Token selection in
`SceneTokenManager`.

Selection inputs:

- speaking state
- semantic token
- urgency
- overlapping speech state

Selection rule:

- `EMERGENCY` and `CRITICAL` tokens are always transmitted.
- `WARNING` and `INSTRUCTION` tokens are always transmitted.
- Other speaking tokens are transmitted if their priority is above
  `minimumTransmissionPriority`.
- Low-priority silent/listener tokens are dropped when selection is enabled.

This makes communication reduction a consequence of semantic selection, rather
than merely a consequence of using a compact token format.

## Rendering Pipeline

```text
SceneToken[]
  -> decode direction into AudioSource position
  -> decode distance into radius and volume
  -> decode speakingState into playback emphasis or suppression
  -> decode turnState, semanticToken, and urgency into volume/pitch modulation
  -> spatial audio output
```

Current implementation:

- `Assets/Scripts/SceneToken/SceneTokenDecoderRenderer.cs`

## Evaluation Conditions

The main user study uses three conditions:

1. `TRADITIONAL`
   - Uses the original speaker object positions.
2. `DIRECTION_DISTANCE`
   - Uses quantized direction and quantized distance.
3. `FULL_SCENE_TOKEN`
   - Adds turn-state and semantic-token modulation.

These conditions compare ordinary object-based spatial audio, spatial metadata
rendering, and the proposed semantic Scene Token rendering.

The implementation still keeps `DIRECTION_ONLY` and
`DIRECTION_DISTANCE_SPEAKING` as optional ablation conditions for development
checks, but they are not required for the main user study.

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
