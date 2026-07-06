# Scene Token One-Slide Explanation

## Slide Title

Scene Token: Semantic Spatial Audio for Multi-Speaker VR Conversation

## Core Message

Conventional spatial audio mainly answers:

```text
Where should the sound be reproduced?
```

Scene Token additionally answers:

```text
What is happening in the conversation scene?
```

## Research Gap Diagram

```text
Existing Spatial Audio

Speech Object
  + Position / Direction / Distance
  -> Spatial Audio Rendering
  -> Listener hears where the sound is

Limitation:
  The system does not explicitly represent who is speaking,
  whether speech overlaps, or whether the utterance is a question,
  answer, instruction, or warning.
```

```text
Proposed Scene Token

Speech Object
  + Position / Direction / Distance
  + Speaking State
  + Turn State
  + Semantic Token
  -> Scene Token
  -> Token-Based Spatial Audio Rendering
  -> Listener understands where the speaker is and what is happening
```

## One-Line Comparison

```text
MASA / IVAS: How should the sound be reproduced?
Scene Token: What is happening in the communication scene?
```

## Evaluation Message

Compare three conditions:

| Condition | Role |
| --- | --- |
| `TRADITIONAL` | baseline spatial audio |
| `DIRECTION_DISTANCE` | spatial metadata only |
| `FULL_SCENE_TOKEN` | spatial metadata + speaking/turn/semantic state |

Measure:

- direction response accuracy and latency
- speaker response accuracy and latency
- conversation comprehension
- subjective ease of following the conversation

## Speaking Script

This research targets multi-speaker conversation in VR. It defines Scene Token as an intermediate representation that integrates speaker identity, relative direction, distance, speaking state, turn state, and semantic utterance labels. Existing spatial audio metadata mainly supports sound reproduction. Scene Token instead represents the conversation scene itself, such as who is speaking, whether speech overlaps, and whether the utterance is a question, answer, instruction, or warning. The evaluation asks whether this representation can support conversation understanding through token-based spatial audio rendering.
