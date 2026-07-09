# Evaluation Hypotheses

This document defines what the Semantic Spatial Audio / Scene Token evaluation should test. The main goal is to evaluate Scene Token as a discrete scene representation that supports conversation understanding, not merely as position metadata.

## Research Questions

```text
In multi-speaker VR conversations, can Scene Tokens that integrate spatial information and conversation state improve speaker awareness, direction awareness, and conversation understanding compared with conventional spatial audio presentation?
```

The evaluation is organized into four research questions.

| RQ | Question | Main module | Main evaluation |
| --- | --- | --- | --- |
| RQ1 | Communication Efficiency: can Scene Token selection and packetization reduce communication metadata while preserving important tokens? | Selection Function + Communication Layer | `PacketBytes`, `DropRatio` |
| RQ2 | Perceptual Performance: can Scene Token rendering preserve direction and active-speaker perception? | Scene Analysis + Scene Representation + Rendering + Evaluation Layer | `DirectionAccuracy`, `SpeakerAccuracy` |
| RQ3 | Cognitive Performance: can semantic and turn-state tokens improve objective situation understanding? | Full Scene Token + Rendering Layer | `SituationUnderstanding`, `ResponseTime` |
| RQ4 | User Experience: does semantic spatial rendering preserve acceptable subjective experience? | Rendering Layer + Evaluation Layer | NASA-TLX, naturalness, ease of following |

The thesis structure should present these RQs through four layers:

```text
Research Layer: what the method represents
Implementation Layer: how Unity realizes it
Logging Layer: what data is recorded
Evaluation Layer: how RQs and hypotheses are tested
```

## Main Evaluation Conditions

The first user study should use three main conditions.

| Condition | Implementation name | Included information | Purpose |
| --- | --- | --- | --- |
| C1 | `C1_TRADITIONAL` | original object position | baseline spatial audio |
| C2 | `C2_DIRECTION_DISTANCE` | quantized direction + distance | spatial metadata only |
| C3 | `C3_FULL_SCENE_TOKEN` | direction + distance + speaking + turn + semantic token | proposed semantic scene representation |

`DIRECTION_ONLY` and `DIRECTION_DISTANCE_SPEAKING` remain useful for development checks and later ablation analysis, but they are not required for the first main user study.

## Representative Metrics

The evaluation chapter should use one small set of representative metrics.

| RQ | Representative metrics | Evaluation type |
| --- | --- | --- |
| RQ1 Communication Efficiency | `PacketBytes`, `DropRatio`, `ImportantTokenKeptRatio`, `TokensPerPacket` | objective communication metrics |
| RQ2 Perceptual Performance | `DirectionAccuracy`, `SpeakerAccuracy`, `ResponseLatency` | objective perceptual metrics |
| RQ3 Cognitive Performance | `SituationUnderstanding`, `ResponseTime`, overlap recognition, important-utterance recognition | objective cognitive metrics |
| RQ4 User Experience | NASA-TLX, naturalness, ease of following, annoyance/distraction | subjective user-experience metrics |

RQ3 should be treated as objective cognitive evaluation. RQ4 should be treated
as subjective user-experience evaluation.

## Experimental Variable Structure

Selection is not only an RQ1 communication mechanism. It is also an independent
variable that can indirectly affect RQ2 and RQ3 through rendering.

```text
Independent variable:
  Selection method / rendering condition

Dependent variables:
  PacketBytes
  DirectionAccuracy
  SpeakerAccuracy
  SituationUnderstanding
  NASA-TLX
  Naturalness
```

The overall evaluation logic is:

```text
Proposed Method

Scene Analysis
  -> Scene Token
  -> Selection Function
  -> Scene Packet
  -> Rendering

Evaluation

Communication
  -> PacketBytes

Perception
  -> DirectionAccuracy
  -> SpeakerAccuracy

Situation Awareness
  -> Understanding
  -> ResponseTime

User Experience
  -> NASA-TLX
  -> Naturalness
  -> Ease of Following
```

## Hypothesis 1: Speaker Localization

Corresponding RQ:

- RQ2

Hypothesis:

```text
C2_DIRECTION_DISTANCE improves direction response accuracy and reduces direction response latency compared with C1_TRADITIONAL.
```

Comparison:

- `C1_TRADITIONAL`
- `C2_DIRECTION_DISTANCE`
- `C3_FULL_SCENE_TOKEN`

Metrics:

- direction response accuracy
- direction response latency
- direction error pattern by condition

Required log fields:

- `condition`
- `direction`
- `response_direction`
- `expected`
- `isCorrect`
- `responseLatency`
- `ambiguous`

## Hypothesis 2: Active Speaker Identification

Corresponding RQ:

- RQ2

Hypothesis:

```text
C3_FULL_SCENE_TOKEN improves active-speaker identification accuracy and reduces speaker response latency compared with spatial-metadata-only rendering.
```

Comparison:

- `C1_TRADITIONAL`
- `C2_DIRECTION_DISTANCE`
- `C3_FULL_SCENE_TOKEN`

Metrics:

- speaker response accuracy
- speaker response latency
- active speaker recognition accuracy

Required log fields:

- `condition`
- `speakerId`
- `speakingState`
- `turnState`
- `response_speaker`
- `expected`
- `isCorrect`
- `responseLatency`
- `ambiguous`

## Hypothesis 3: Conversation Understanding

Corresponding RQ:

- RQ3

Hypothesis:

```text
C3_FULL_SCENE_TOKEN improves understanding of conversation flow, important utterances, and overlap compared with C2_DIRECTION_DISTANCE.
```

Comparison:

- `C2_DIRECTION_DISTANCE`
- `C3_FULL_SCENE_TOKEN`

Metrics:

- conversation comprehension score
- turn/overlap recognition accuracy
- subjective ease of following the conversation
- subjective usefulness of semantic emphasis

Example comprehension questions:

- Who asked the question?
- Who answered?
- Which speaker gave the instruction or warning?
- Did the participant notice the overlap?
- Was the conversation flow easy to follow?

## Hypothesis 4: Workload and Naturalness

Corresponding RQ:

- RQ4

Hypothesis:

```text
C3_FULL_SCENE_TOKEN supports conversation understanding without substantially increasing workload or unnaturalness, as long as volume and pitch emphasis remain moderate.
```

Metrics:

- NASA-TLX short form
- naturalness rating
- ease of identifying the speaker
- ease of following conversation
- annoyance or distraction rating

Risk:

```text
If semantic emphasis is too strong, C3_FULL_SCENE_TOKEN may feel unnatural or distracting. The first study should use light emphasis and evaluate this explicitly.
```

## Communication Metadata Volume

Corresponding RQ:

- RQ1

Communication volume evaluates whether Scene Token can work as a compact
communication representation while preserving important semantic tokens.

Hypothesis:

```text
Scene Token selection and packetization reduce transmitted metadata while keeping important semantic tokens.
```

Metrics:

- tokens per second
- JSON-like bytes per second
- compact token bytes per second
- object metadata bytes per second
- compact savings ratio
- token selection metrics when enabled
- packet bytes and packet rate

Required log fields:

- `tokensPerSecond`
- `jsonBytesPerSecond`
- `compactBytesPerSecond`
- `objectMetadataBytesPerSecond`
- `compactSavingsRatio`
- `generatedTokensPerSecond`
- `selectedTokensPerSecond`
- `tokenDropRatio`
- `importantTokenSendRatio`
- `selectionSavingsRatio`
- `estimatedBytes`
- `payloadBytes`
- `packetsPerSecond`
- `tokensPerPacket`

## Minimum Valid Evaluation Output

A minimum pilot evaluation should produce:

- token, event, and metrics CSV logs for all three main conditions
- direction response accuracy by condition
- speaker response accuracy by condition
- average response latency by condition
- conversation comprehension score by condition
- subjective rating by condition
- NASA-TLX short-form results
- communication metrics by condition
- packet metrics by condition

## Discussion Targets

The thesis or presentation should discuss:

1. Whether Scene Token made speakers easier to identify.
2. Whether Scene Token made conversation flow easier to follow.
3. Which token fields were useful.
4. Whether Scene Token increased workload or unnaturalness.
5. Whether communication metrics support the representation design as a secondary result.
