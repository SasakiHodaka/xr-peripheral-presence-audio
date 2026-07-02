# Research Overview

## Title

VR空間におけるScene Tokenを用いた意味的空間音声コミュニケーション手法の提案

English working title:

```text
Semantic Spatial Voice Communication in VR Using Scene Tokens
```

## One-Sentence Summary

This research defines Scene Token as an intermediate representation that integrates speaker identity, spatial information, and conversation state in VR, and evaluates whether token-based spatial audio rendering supports multi-speaker conversation understanding.

## Background

VR and metaverse environments increasingly support multi-user conversation,
remote collaboration, and group work. In these environments, spatial audio is
important because it helps users perceive where other speakers are located.

Existing immersive voice communication research, including IVAS,
metadata-assisted spatial audio, and object-based audio, mainly focuses on the
efficient transmission and reconstruction of spatial audio. These approaches can
represent information such as direction, distance, object position, and spatial
audio rendering parameters.

However, in a multi-speaker conversation, users do not only need to know where a
voice is coming from. They also need to understand conversation-level
information:

- who is currently speaking
- who is responding
- whether multiple people are speaking at the same time
- whether the utterance is a question, answer, instruction, or warning
- who currently holds the conversational turn

Therefore, spatial audio metadata alone is not sufficient for supporting
conversation understanding in VR.

## Research Problem

The central problem is:

```text
How can a VR communication system represent and use not only speaker position
but also conversation state for spatial audio presentation?
```

More specifically:

1. Existing spatial audio methods represent sound position and rendering
   parameters, but they do not explicitly represent conversation meaning.
2. Multi-speaker VR conversations can become difficult to understand when
   speaker positions, speaking states, and conversational roles are unclear.
3. A structured representation is needed to integrate spatial information and
   conversation-state information.

## Purpose

The purpose of this research is to define and implement Scene Token, a discrete
representation that integrates:

- speaker identity
- direction
- distance
- speaking state
- turn state
- semantic utterance label

The system uses Scene Tokens to reconstruct or modulate spatial audio
presentation so that users can more easily understand who is speaking, where
they are, and what role they have in the conversation.

## Proposed Method

The proposed system follows this pipeline:

```text
VR Speaker Objects
  -> Scene Parsing
  -> spatial-state analysis
  -> conversation-state analysis
  -> Scene Token generation
  -> Scene Token logging / communication
  -> token-based spatial audio rendering
  -> listener
```

In this research, Scene Parsing means the process of analyzing the current VR
communication scene and converting speaker position, listener-relative
direction, distance, speaking state, turn state, and semantic label into Scene
Tokens.

For each avatar speaker, the system obtains:

- speaker ID
- speaker position
- listener position and head direction
- AudioSource playback state or scripted speaking state
- semantic label from manual/scripted annotation

The system then generates a Scene Token:

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER",
  "semanticToken": "QUESTION",
  "timestamp": 12.50
}
```

The token is then decoded by the renderer:

- `direction` controls audio source direction
- `distance` controls radius and volume
- `speakingState` controls whether the speaker is emphasized or suppressed
- `turnState` controls turn-related emphasis
- `semanticToken` controls additional modulation such as warning or instruction emphasis

## Novelty

The novelty is not simply using spatial audio in VR. The novelty is the
definition and use of a Scene Token that integrates spatial metadata and
conversation-state metadata.

Comparison:

| Method | Spatial information | Speaking state | Turn state | Semantic label | Main purpose |
| --- | --- | --- | --- | --- | --- |
| Traditional spatial audio | yes | weak or implicit | no | no | sound localization |
| MASA / IVAS-style metadata | yes | no | no | no | efficient spatial audio coding/rendering |
| Object-Based Audio | yes | object-dependent | no | no | speaker object rendering |
| Turn-taking research | no or limited | yes | yes | sometimes | conversation analysis |
| Proposed Scene Token | yes | yes | yes | yes | semantic spatial conversation support |

In short:

```text
Existing work: sound reproduction
Proposed work: conversation understanding support through spatial audio
```

## Current Implementation

The Unity prototype currently implements:

- a three-speaker mock VR scene
- speaker objects with ID, AudioSource, speaking state, and semantic label
- 8-direction tokenization
- 3-level distance tokenization
- speaking-state detection from scripted/manual state and AudioSource playback
- turn-state estimation with `LISTENER`, `TURN_HOLDER`, and `OVERLAPPER`
- semantic labels such as `QUESTION`, `ANSWER`, `INSTRUCTION`, and `WARNING`
- token-based AudioSource position, volume, and pitch reconstruction
- deterministic scripted conversation
- CSV token logs, event logs, and communication-volume metrics

Main implementation files:

- `Assets/Scripts/SceneToken/SceneToken.cs`
- `Assets/Scripts/SceneToken/SpeakerObject.cs`
- `Assets/Scripts/SceneToken/SceneTokenManager.cs`
- `Assets/Scripts/SceneToken/SceneTokenDecoderRenderer.cs`
- `Assets/Scripts/SceneToken/SceneTokenLogger.cs`
- `Assets/Scripts/SceneToken/SceneTokenMetrics.cs`
- `Assets/Scripts/SceneToken/SceneTokenExperimentSession.cs`

## Evaluation Plan

### Conditions

The main user study compares three rendering conditions:

1. `TRADITIONAL`
   - original speaker positions
2. `DIRECTION_DISTANCE`
   - direction and distance tokens
3. `FULL_SCENE_TOKEN`
   - direction, distance, speaking state, turn state, and semantic token

`DIRECTION_ONLY` and `DIRECTION_DISTANCE_SPEAKING` remain available as optional
ablation conditions, but they are not required for the main participant study.

### Objective Metrics

Candidate objective metrics:

- speaker localization accuracy
- speaker identification time
- turn tracking accuracy
- overlap detection accuracy
- task completion time in a conversation task
- communication-volume comparison

### Subjective Metrics

Candidate subjective metrics:

- conversation understanding
- ease of identifying the active speaker
- perceived naturalness
- perceived workload, such as NASA-TLX
- usefulness of semantic emphasis

## Hypotheses

H1:

```text
Adding direction and distance tokens improves speaker localization compared
with traditional rendering.
```

H2:

```text
Adding speaking-state and turn-state tokens improves active-speaker
identification and turn tracking.
```

H3:

```text
Full Scene Token rendering improves conversation understanding compared with
spatial metadata-only rendering.
```

H4:

```text
Scene Token representation can reduce or structure communication metadata
compared with richer object-level metadata, but this is a secondary analysis.
```

## Research Scope

### In Scope

- Scene Token definition
- Unity prototype implementation
- controlled three-speaker conversation demo
- manual and scripted semantic labels
- token logging and condition comparison
- analysis of speaker localization and conversation understanding

### Out of Scope for the First Prototype

- automatic speech recognition
- LLM-based semantic classification
- neural audio codec implementation
- full IVAS or MASA codec implementation
- real networked multi-user VR communication

These are treated as future extensions.

## Important Limitation

The current semantic labels are manual or scripted. This is intentional for the
first prototype because it keeps the experiment controlled. The first research
question is whether semantic and turn-state information is useful when added to
spatial audio. Automatic extraction using ASR or LLMs should be introduced only
after this controlled baseline is evaluated.

## Expected Contribution

This research aims to contribute:

1. A definition of Scene Token for VR spatial voice communication.
2. A Unity prototype that generates, logs, and renders Scene Tokens.
3. A comparison framework for spatial metadata and semantic scene metadata.
4. Experimental evidence on whether Scene Tokens support multi-speaker VR
   conversation understanding.

## Future Work

Future extensions include:

- automatic semantic-token generation using ASR and LLMs
- addressee estimation, such as who the speaker is talking to
- gaze and gesture integration
- richer turn-taking models
- real-time networked VR meetings
- compact binary Scene Token transmission
- integration with neural audio tokens and generative audio rendering
