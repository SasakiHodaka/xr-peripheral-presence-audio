# SemanticSpatialAudio Implementation Flow

This document describes the implemented Unity flow in the active project:

```text
C:\Users\acd-pc67\SemanticSpatialAudio
```

The current runtime implementation is the `Assets/Scripts/SceneToken` pipeline.
It is the implementation used by `Assets/Scenes/SceneTokenMock.unity`.

The core flow is:

```text
SpeakerObject state
  -> SceneTokenManager sampling loop
  -> spatial and conversation analyzers
  -> SceneToken data model
  -> optional token selection
  -> token logging and communication metrics
  -> SceneTokenDecoderRenderer
  -> AudioSource position, volume, and pitch
  -> spatial audio output
```

## 1. Runtime Entry Point

The primary runtime entry point is:

```text
Assets/Scripts/SceneToken/SceneTokenManager.cs
```

`SceneTokenManager.Update()` runs every frame, but token generation is throttled
by `tokenUpdateInterval`. When the interval has elapsed, it calls
`GenerateTokens()`.

`GenerateTokens()` is responsible for the complete per-sample pipeline:

1. Clear the previous latest token list.
2. Count how many speakers are currently active.
3. Create one token per configured speaker.
4. Decide whether each generated token should be transmitted.
5. Write token logs when experiment logging is enabled.
6. Send selected tokens to the renderer.
7. Send generated and selected token lists to the metrics logger.
8. Update response-window events for participant response logging.

This design keeps token generation centralized. That is important because
turn-taking state cannot be decided from a single avatar alone. The manager must
observe all speakers at the same time to classify a speaking avatar as either a
single `TURN_HOLDER` or an `OVERLAPPER`.

## 2. Speaker State Acquisition

Speaker state comes from:

```text
Assets/Scripts/SceneToken/SpeakerObject.cs
```

Each `SpeakerObject` represents one mock remote speaker/avatar. It provides:

- `speakerId`
- `audioSource`
- `isSpeaking`
- `semanticToken`
- `urgency`
- `targetObjectId`
- `utteranceText`
- `semanticConfidence`

The speaker object also configures its `AudioSource` for spatial playback:

- `playOnAwake = false`
- `spatialBlend = desktopSpatialBlend`
- `spatialize = true`
- `minDistance = 0.5`
- `maxDistance = 20`
- `dopplerLevel = 0`

If no recorded voice clip is assigned, the object generates a short fallback
tone. This keeps the mock scene audible even without external voice assets.

Manual controls are also implemented here:

- `A`, `B`, `C`: toggle speaking state for each avatar in the mock scene
- `Q`, `W`, `E`: cycle the semantic token for each avatar

The scripted conversation controller can also set these same fields during a
repeatable experiment sequence.

## 3. Spatial Analysis

Spatial parsing is split into direction and distance analyzers.

Direction:

```text
Assets/Scripts/SceneToken/DirectionAnalyzer.cs
```

`CalculateSignedAzimuth(listener, speaker)` computes listener-relative azimuth:

1. Compute the world-space offset from listener to speaker.
2. Project the offset onto the horizontal plane.
3. Compare the flat offset with listener forward and right vectors.
4. Return signed azimuth in degrees.

`QuantizeDirection(azimuth)` maps azimuth to one of eight discrete tokens:

| Azimuth range | Direction token |
| --- | --- |
| -22.5 to 22.5 | `FRONT` |
| 22.5 to 67.5 | `FRONT_RIGHT` |
| 67.5 to 112.5 | `RIGHT` |
| 112.5 to 157.5 | `BACK_RIGHT` |
| -67.5 to -22.5 | `FRONT_LEFT` |
| -112.5 to -67.5 | `LEFT` |
| -157.5 to -112.5 | `BACK_LEFT` |
| otherwise | `BACK` |

Distance:

```text
Assets/Scripts/SceneToken/DistanceAnalyzer.cs
```

`CalculateHorizontalRange(listener, speaker)` computes horizontal distance by
projecting the listener-speaker offset onto the ground plane.

`QuantizeDistance(range)` maps range to three distance tokens:

| Range | Distance token |
| --- | --- |
| `< 1.5 m` | `NEAR` |
| `1.5 m` to `< 3.0 m` | `MID` |
| `>= 3.0 m` | `FAR` |

The implementation intentionally uses horizontal distance rather than full 3D
distance because the current experiment focuses on conversational direction and
near/mid/far awareness around the listener.

## 4. Conversation-State Analysis

Conversation parsing is implemented in:

```text
Assets/Scripts/SceneToken/ConversationAnalyzer.cs
```

The manager first calls `CountSpeakingSpeakers(speakers)`. The resulting count
is then used to derive the token-level conversation state for every speaker.

Speaking state:

| Speaker condition | Token value |
| --- | --- |
| not active | `SILENT` |
| active | `SPEAKING` |

Turn state:

| Speaker condition | Speaking count | Token value |
| --- | --- | --- |
| not active | any | `LISTENER` |
| active | `1` | `TURN_HOLDER` |
| active | `> 1` | `OVERLAPPER` |

This gives the renderer and evaluator information that ordinary object-based
spatial audio does not explicitly represent: whether the active voice is the
main turn holder or part of overlapping speech.

## 5. Scene Token Construction

The token data model is:

```text
Assets/Scripts/SceneToken/SceneToken.cs
```

`SceneTokenManager.CreateToken()` fills the token fields from the analyzers and
speaker state.

Main fields:

| Field | Source |
| --- | --- |
| `speakerId` | `SpeakerObject.speakerId` |
| `azimuth` | `DirectionAnalyzer.CalculateSignedAzimuth` |
| `range` | `DistanceAnalyzer.CalculateHorizontalRange` |
| `direction` | quantized azimuth |
| `distance` | quantized range |
| `speakingState` | `ConversationAnalyzer.QuantizeSpeakingState` |
| `turnState` | `ConversationAnalyzer.QuantizeTurnState` |
| `semanticToken` | active speaker's `SpeakerObject.semanticToken`; otherwise `NONE` |
| `urgency` | active speaker's urgency; otherwise `LOW` |
| `targetObjectId` | active speaker's target object id |
| `utteranceText` | active speaker's utterance text |
| `semanticConfidence` | active speaker's semantic confidence |
| `priority` | rule-based priority score |
| `condition` | current render condition |
| `participantId` | current experiment participant id |
| `sessionId` | current experiment session id |
| `trialIndex` | current trial index |
| `trialElapsed` | elapsed time in current trial |
| `timestamp` | Unity `Time.time` |

The token is not only a rendering input. It is also the experiment record used
for later analysis.

## 6. Priority and Token Selection

Priority is computed inside:

```text
SceneTokenManager.CalculatePriority(...)
```

The score combines:

- whether the speaker is currently speaking
- whether more than one speaker is active
- semantic token importance
- urgency level

High-priority semantic labels receive larger weights:

| Semantic token | Priority effect |
| --- | --- |
| `EMERGENCY` | highest semantic boost |
| `WARNING` | high boost |
| `INSTRUCTION` | medium-high boost |
| `QUESTION`, `ANSWER` | medium boost |
| `AGREEMENT`, `DISAGREEMENT` | low-medium boost |
| `CHAT` | low boost |

Urgency also increases priority:

| Urgency | Priority effect |
| --- | --- |
| `CRITICAL` | highest urgency boost |
| `HIGH` | high boost |
| `MEDIUM` | medium boost |
| `LOW` | low boost |

Token selection is controlled by:

```text
SceneTokenManager.enableTokenSelection
SceneTokenManager.minimumTransmissionPriority
```

When selection is disabled, all generated tokens are treated as transmitted.
When selection is enabled:

- `EMERGENCY` and `CRITICAL` tokens always pass.
- `WARNING` and `INSTRUCTION` tokens pass as important intent.
- speaking tokens pass when their priority is above the configured threshold.
- low-priority tokens are dropped.

The token records why it was selected or dropped through `selectionReason`.

## 7. Rendering Conditions

Token rendering is implemented in:

```text
Assets/Scripts/SceneToken/SceneTokenDecoderRenderer.cs
```

`SceneTokenDecoderRenderer.Render(tokens)` receives the selected token list and
updates each matching speaker's `AudioSource`.

It controls:

- AudioSource position
- AudioSource volume
- AudioSource pitch

The renderer supports five experiment conditions.

### 1. TRADITIONAL

The AudioSource position stays at the original speaker object position.

Purpose:

- baseline object-position spatial audio

### 2. DIRECTION_ONLY

The AudioSource direction is reconstructed from the quantized direction token.
Distance is fixed to the middle radius.

Purpose:

- isolate the effect of direction tokenization

### 3. DIRECTION_DISTANCE

Direction and distance are both reconstructed from discrete tokens.

Purpose:

- evaluate direction plus near/mid/far distance representation

### 4. DIRECTION_DISTANCE_SPEAKING

Direction and distance are reconstructed, and silent speakers are suppressed by
setting volume to zero.

Purpose:

- evaluate the value of explicitly representing speaking state

### 5. FULL_SCENE_TOKEN

The full token is used. Direction, distance, speaking state, turn state,
semantic token, and urgency all affect the output.

Additional full-token mappings:

| Token field | Rendering effect |
| --- | --- |
| `turnState = TURN_HOLDER` | volume boost |
| `turnState = OVERLAPPER` | volume reduction |
| `semanticToken = WARNING` | volume and pitch boost |
| `semanticToken = EMERGENCY` | stronger volume and pitch boost |
| `semanticToken = INSTRUCTION` | volume boost |
| `semanticToken = QUESTION` | slight pitch boost |
| `semanticToken = DISAGREEMENT` | slight pitch reduction |
| `urgency = HIGH` | volume boost |
| `urgency = CRITICAL` | stronger volume and pitch boost |

This condition is the implemented semantic spatial audio condition.

## 8. Condition Control

Condition switching is implemented in:

```text
Assets/Scripts/SceneToken/SceneTokenConditionController.cs
```

Keyboard controls:

| Key | Condition |
| --- | --- |
| `1` | `TRADITIONAL` |
| `2` | `DIRECTION_DISTANCE` |
| `3` | `FULL_SCENE_TOKEN` |
| `4` | `DIRECTION_ONLY` |
| `5` | `DIRECTION_DISTANCE_SPEAKING` |

The experiment session can also advance conditions according to its configured
condition order.

Condition changes are written to the event log through `SceneTokenEventLogger`.

## 9. Experiment Session Flow

Experiment control is implemented in:

```text
Assets/Scripts/SceneToken/SceneTokenExperimentSession.cs
```

The session controller manages:

- participant id
- session id
- trial index
- current condition
- trial elapsed time
- timed or manual condition advancement
- session start/stop/restart
- response event logging

`SceneTokenManager` can gate token and metric logging with:

```text
logOnlyDuringExperimentSession
```

When this flag is enabled, token and metric rows are written only while
`SceneTokenExperimentSession.IsRunning` is true. This avoids mixing setup or
debug interaction with experiment data.

## 10. Scripted Conversation

Repeatable conversation playback is implemented in:

```text
Assets/Scripts/SceneToken/SceneTokenScriptedConversation.cs
```

The scripted conversation sets speaker state over time:

- active speaker
- utterance duration
- semantic token
- urgency
- utterance text
- semantic confidence
- optional voice clip

This makes trials repeatable across rendering conditions. The same conversation
sequence can be played under baseline spatial audio and full Scene Token
rendering, which is required for a controlled comparison.

## 11. Participant Response Logging

Participant response recording is split between:

```text
Assets/Scripts/SceneToken/SceneTokenManager.cs
Assets/Scripts/SceneToken/SceneTokenExperimentSession.cs
```

The manager exposes response controls in the debug HUD and through keyboard
input.

Direction response controls:

- arrow keys for cardinal directions
- HUD buttons for all eight direction tokens

Speaker response controls:

- `J`, `K`, `L` for speakers `A`, `B`, `C`
- HUD buttons for speakers `A`, `B`, `C`

When a response is recorded, the system attaches:

- participant id
- session id
- trial index
- current condition
- response value
- expected value when there is exactly one active speaker
- correctness flag
- ambiguity flag
- response latency from trial start

Responses are marked ambiguous when there is no active speaker or when multiple
speakers overlap.

## 12. Logging Outputs

Token logs:

```text
Assets/Scripts/SceneToken/SceneTokenLogger.cs
```

Each token row includes spatial fields, conversation fields, semantic fields,
selection fields, condition, participant/session fields, and timing fields.

Event logs:

```text
Assets/Scripts/SceneToken/SceneTokenEventLogger.cs
```

Events include:

- session start/stop
- trial start/end
- condition changes
- scripted conversation events
- response-window start/end
- direction responses
- speaker responses

Metrics:

```text
Assets/Scripts/SceneToken/SceneTokenMetrics.cs
```

The metrics component estimates:

- generated tokens per second
- selected tokens per second
- JSON-like bytes per second
- selected JSON-like bytes per second
- compact token bytes per second
- object metadata bytes per second
- compact-token savings ratio
- token drop ratio
- important-token send ratio
- selection savings ratio

These metrics are communication-cost estimates. They are useful for comparing
representation strategies, but they are not yet a real network bitrate
measurement.

## 13. Validation Tools

Analyzer self-check:

```text
Assets/Editor/SceneTokenAnalyzerSelfCheck.cs
```

Menu:

```text
Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check
```

This validates:

- direction quantization
- distance quantization
- speaking-state quantization
- turn-state quantization
- listener-relative azimuth
- horizontal range

Scene validation:

```text
Assets/Editor/SceneTokenSceneValidator.cs
```

Menu:

```text
Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene
```

This validates the mock scene wiring:

- manager
- renderer
- token logger
- event logger
- metrics logger
- condition controller
- experiment session
- scripted conversation
- listener
- three speakers
- three debug labels
- required speaker key bindings

## 14. Current Implementation Boundary

The current prototype implements a controlled, rule-based Scene Token pipeline.
It does not yet implement:

- real multi-user networking
- microphone capture and live speech recognition
- LLM-based automatic semantic classification
- IVAS codec internals
- MASA bitstream coding
- neural audio codec tokens
- generative audio reconstruction

This boundary is intentional. The current implementation is designed to
evaluate whether explicit scene tokens can improve multi-speaker spatial
conversation awareness before adding automatic extraction or real network
transport.

## 15. Research Mapping

The implemented system maps to the research claim as follows:

```text
Speech object
  -> SpeakerObject

Position
  -> DirectionAnalyzer + DistanceAnalyzer

Meaning / turn state
  -> SpeakerObject.semanticToken + ConversationAnalyzer

Scene Token
  -> SceneToken data model

Semantic spatial audio rendering
  -> SceneTokenDecoderRenderer FULL_SCENE_TOKEN condition

Evaluation data
  -> token logs + event logs + response logs + metrics logs
```

The key technical contribution in this prototype is not simply playing spatial
audio. It is the explicit conversion of a multi-speaker conversation scene into
a token representation that can drive controlled rendering conditions and
produce analyzable experiment logs.
