# Scene Token Setup Guide

This module implements a rule-based prototype for semantic spatial audio presentation in VR collaborative work.

## Concept

Scene Token Model is an information model for collaborative speech events.

It structures the information required for situation awareness:

1. speaker
2. spatial relation
3. object relation
4. collaborative intent
5. priority
6. conversation / event state

The implemented pipeline is:

1. Scene context acquisition
2. Scene Token instantiation
3. Scene Token generation
4. Rendering policy selection
5. Audio strategy generation
6. Semantic spatial audio reconstruction

`SceneTokenSelector` is optional. Disable `enableSelection` to send all generated tokens.

## Basic Scene Setup

1. Add `SceneTokenParticipant` to each remote collaborator avatar.
2. Set `speakerId` and `role`.
3. Assign `headTransform` and `bodyTransform`.
4. Add `SceneTokenGenerator` to an empty GameObject.
5. Assign the local user head to `listenerHead`.
6. Assign all participant components to `participants`.
7. Add `SceneTokenSelector` to the same or another GameObject.
8. Add `SemanticSpatialAudioReconstructor`.
9. Add `SceneTokenLoopbackTransport` and assign the generator, selector, and reconstructor.

For a quick local test, add `SceneTokenManualInput` to a participant:

- `1`: Report
- `2`: Instruction
- `3`: Warning
- `4`: Emergency
- `5`: Confirmation
- `0`: Silent
- `Left Shift` with an intent key: Interrupting speech

## Token Fields

| Category | Fields |
| --- | --- |
| Spatial information | speakerId, direction, distance |
| Semantic information | intent, urgency, targetObjectId, speechState |
| Reconstruction control | priority, sourcePosition, targetPosition |

Recommended token model fields:

| Token element | Typical fields |
| --- | --- |
| Speaker | `speakerId`, `speakerRole` |
| Spatial | `direction`, `distance`, `sourcePosition` |
| Object Relation | `targetObjectId`, `targetPosition`, `relation` |
| Collaborative Intent | `intent`, `intentLabel` |
| Priority | `priority`, `urgency` |
| Conversation / Event | `speechState`, `timestamp`, `startTime`, `endTime` |

## Rendering Policy and Audio Strategy

`SemanticSpatialAudioReconstructor` first derives an `AudioStrategy` from a `RenderingPolicy`, then maps it to audio parameters.

| Token field | Policy effect | Audio mapping |
| --- | --- | --- |
| speaker | choose voice identity | source / voice profile |
| direction | set localization target | HRTF / azimuth / elevation |
| distance | set attenuation | volume attenuation |
| intent | choose cue style | clip / cue / clarity |
| urgency | choose priority level | gain / pitch boost |
| targetObject | choose attention target | target localization |
| speechState | choose onset / offset control | play / suppress / fade |

Example policy:

```text
IF priority = critical
THEN ducking = ON, gain = +6 dB, interrupt = ON
```

```text
IF intent = warning
THEN cue = warning_cue
```

```text
IF direction is available
THEN apply HRTF localization
```

## Rule-Based Reconstruction

`SemanticSpatialAudioReconstructor` maps Scene Token fields to audio parameters.

| Token field | Audio mapping |
| --- | --- |
| direction | source localization |
| distance | source distance and attenuation |
| intent | clip, volume, playback priority |
| urgency | volume and pitch boost |
| targetObject | instruction/warning localization |
| speechState | play or suppress audio cue |

Recommended intent mapping:

- `Report`: speaker direction, normal volume
- `Instruction`: target object direction, slightly emphasized
- `Warning`: target object direction, high priority
- `Emergency`: highest priority, strong volume boost
- `Confirmation`: low priority, reduced emphasis

## Evaluation Notes

For evaluation, keep the scenario simple and scenario-based:

- assign ground truth from the task scenario
- use rule-based intent and priority labels
- evaluate token generation, spatial audio reconstruction, and task performance separately

Suggested evaluation flow:

1. baseline spatial audio
2. full Scene Token rendering
3. optional ablation without priority

This keeps the prototype practical for a master's thesis while preserving the core research contribution.

## Evaluation Metrics

The runtime fields of `SceneTokenSelector` and `SceneTokenLoopbackTransport` can be logged or read from the Inspector.

- generated token count
- selected token count
- dropped token count
- important token send rate
- payload reduction rate
- generated/selected/sent kbps
- sent payload bytes

Add `SceneTokenDebugHud` to show these values during Play Mode.

Add `SceneTokenMetricsLogger` to write them to:

`Application.persistentDataPath/scene_token_logs`
