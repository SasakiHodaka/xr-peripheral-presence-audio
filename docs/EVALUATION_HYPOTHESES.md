# Evaluation Hypotheses

This document defines what the Semantic Spatial Audio / Scene Token evaluation should test. The main goal is to evaluate Scene Token as a discrete scene representation that supports conversation understanding, not merely as position metadata.

## Research Question

```text
In multi-speaker VR conversations, can Scene Tokens that integrate spatial information and conversation state improve speaker awareness, direction awareness, and conversation understanding compared with conventional spatial audio presentation?
```

## Main Evaluation Conditions

The first user study should use three main conditions.

| Condition | Implementation name | Included information | Purpose |
| --- | --- | --- | --- |
| C1 | `TRADITIONAL` | original object position | baseline spatial audio |
| C2 | `DIRECTION_DISTANCE` | quantized direction + distance | spatial metadata only |
| C3 | `FULL_SCENE_TOKEN` | direction + distance + speaking + turn + semantic token | proposed semantic scene representation |

`DIRECTION_ONLY` and `DIRECTION_DISTANCE_SPEAKING` remain useful for development checks and later ablation analysis, but they are not required for the first main user study.

## Hypothesis 1: Speaker Localization

Hypothesis:

```text
DIRECTION_DISTANCE improves direction response accuracy and reduces direction response latency compared with TRADITIONAL.
```

Comparison:

- `TRADITIONAL`
- `DIRECTION_DISTANCE`
- `FULL_SCENE_TOKEN`

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

Hypothesis:

```text
FULL_SCENE_TOKEN improves active-speaker identification accuracy and reduces speaker response latency compared with spatial-metadata-only rendering.
```

Comparison:

- `TRADITIONAL`
- `DIRECTION_DISTANCE`
- `FULL_SCENE_TOKEN`

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

Hypothesis:

```text
FULL_SCENE_TOKEN improves understanding of conversation flow, important utterances, and overlap compared with DIRECTION_DISTANCE.
```

Comparison:

- `DIRECTION_DISTANCE`
- `FULL_SCENE_TOKEN`

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

Hypothesis:

```text
FULL_SCENE_TOKEN supports conversation understanding without substantially increasing workload or unnaturalness, as long as volume and pitch emphasis remain moderate.
```

Metrics:

- NASA-TLX short form
- naturalness rating
- ease of identifying the speaker
- ease of following conversation
- annoyance or distraction rating

Risk:

```text
If semantic emphasis is too strong, FULL_SCENE_TOKEN may feel unnatural or distracting. The first study should use light emphasis and evaluate this explicitly.
```

## Secondary Analysis: Communication Metadata Volume

Communication volume is a secondary analysis. The central claim should remain conversation understanding support, not proven bandwidth reduction.

Hypothesis:

```text
Scene Token can represent scene state as compact discrete metadata compared with richer object-level metadata, but this should be reported as supporting evidence rather than the main contribution.
```

Metrics:

- tokens per second
- JSON-like bytes per second
- compact token bytes per second
- object metadata bytes per second
- compact savings ratio
- token selection metrics when enabled

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

## Discussion Targets

The thesis or presentation should discuss:

1. Whether Scene Token made speakers easier to identify.
2. Whether Scene Token made conversation flow easier to follow.
3. Which token fields were useful.
4. Whether Scene Token increased workload or unnaturalness.
5. Whether communication metrics support the representation design as a secondary result.
