# Related Work Q&A

This document summarizes the minimum answers needed for discussion,
presentation, and thesis defense.

## One-Sentence Research Description

VR空間において，空間音声へScene Tokenを付加し，話者の位置だけでなく発話状態や会話役割まで提示することで，会話理解を支援する手法を提案する。

## Core Difference

```text
Existing spatial audio communication:
  speech + position -> spatial audio reproduction

Proposed Scene Token communication:
  speech + position + conversation state -> semantic spatial audio support
```

## IVAS

### What is IVAS?

IVAS is a 3GPP immersive voice and audio communication codec standard. It is
designed to support spatial and immersive audio communication, including
formats such as stereo, object-based audio, ambisonics, and metadata-assisted
spatial audio.

### What is its goal?

The main goal is efficient immersive audio communication: preserving spatial
audio experience while keeping bitrate and communication cost practical.

### How is this project different?

IVAS focuses on coding and transmitting immersive audio. This project focuses
on adding a discrete conversation-state representation that supports the
listener's understanding of a VR conversation.

Short answer:

```text
IVAS is a codec standard for immersive audio.
Scene Token is a semantic representation for VR conversation state.
```

## MASA

### What is MASA?

MASA means Metadata-Assisted Spatial Audio. It represents spatial audio by
combining audio with metadata such as direction and spatial characteristics.

### What does MASA mainly represent?

Typical metadata is related to how sound should be spatially reproduced:

- direction
- distance or position-related information
- diffuseness
- coherence
- spatial rendering properties

### What does MASA not represent?

MASA does not primarily represent conversation meaning, such as:

- who is asking a question
- who is answering
- who has the conversational turn
- whether an utterance is a warning or instruction

### How is Scene Token different from MASA?

```text
MASA:
  metadata for sound reproduction
  "How should this sound be rendered?"

Scene Token:
  metadata for scene and conversation understanding
  "What is happening in this communication scene?"
```

### If asked: "Isn't this just adding role metadata to MASA?"

Answer:

It is related, but the purpose is different. MASA is designed for efficient
spatial audio coding and rendering. Scene Token is designed as a conversation
scene representation that integrates spatial state and conversational state.
The proposed contribution is the definition and evaluation of that integrated
representation for VR communication support.

## Object-Based Audio

### What is Object-Based Audio?

Object-Based Audio treats each sound source as an object. For VR meetings, each
speaker voice can be treated as a separate object with position and rendering
parameters.

### Why is it related?

The current implementation follows this idea by treating each avatar as a
`SpeakerObject`.

### What is the limitation?

Object-Based Audio can preserve "who is where", but it does not necessarily
represent "what role that speaker has in the conversation".

Short answer:

```text
Object-Based Audio keeps speakers as sound objects.
Scene Token adds conversation-state information to those objects.
```

## Parametric Object Coding

### Why is it related?

Parametric object coding addresses how to efficiently code and reconstruct
multiple audio objects, such as several speakers, at low bitrates.

### What is the limitation for this project?

It is mainly about efficient object coding and reconstruction, not semantic
conversation understanding.

Short answer:

```text
Parametric object coding reduces audio-object communication cost.
Scene Token represents spatial conversation meaning.
```

## Semantic Communication

### What is Semantic Communication?

Semantic Communication is a communication approach that aims to transmit
meaning rather than only raw signals.

### Is this project Semantic Communication?

It is close, but the project should be described more specifically as semantic
spatial audio communication or semantic spatial voice communication.

Reason:

- It still uses spatial information.
- It still renders audio.
- It adds meaning and conversation state to spatial audio communication.

Short answer:

```text
This project is not pure semantic communication. It is semantic spatial audio
communication because it combines spatial information and conversation meaning.
```

## Audio Token

### What is an Audio Token?

An audio token is a discrete representation of an audio signal, often produced
by neural audio codecs or vector quantization.

### How is Scene Token different?

```text
Audio Token:
  compresses or represents the sound signal itself.

Scene Token:
  represents spatial and conversational meaning around the sound.
```

Short answer:

```text
Audio Token is signal-oriented.
Scene Token is scene- and communication-oriented.
```

## Turn Taking

### What is Turn Taking?

Turn Taking concerns how participants alternate speaking roles in conversation.

### How is it used here?

The current prototype uses a minimal turn-state representation:

- `TURN_HOLDER`: the active speaker when only one speaker is speaking
- `LISTENER`: a non-speaking participant
- `OVERLAPPER`: a speaker during overlapping speech

### Why not classify question and answer automatically now?

Automatic question/answer/instruction classification requires ASR and semantic
classification. For the first prototype, manual and scripted labels are more
controlled and easier to evaluate. Automatic classification is a future
extension.

## Scene Token

### What is Scene Token?

Scene Token is a discrete representation that integrates:

- speaker identity
- direction
- distance
- speaking state
- turn state
- optional semantic label

### Why is it needed?

In a multi-speaker VR conversation, spatial audio alone tells where a sound
comes from. It does not explicitly tell who has the turn, whether multiple
people overlap, or what kind of utterance is being made. Scene Token adds this
conversation-state layer.

### What is the current minimum token?

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER"
}
```

### What is the current extended token?

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

## Dangerous Questions

### Q1. Is Scene Token just metadata?

Answer:

Yes, it is a kind of structured metadata, but the research contribution is the
definition and use of metadata for conversation understanding, not only spatial
audio rendering. Existing spatial metadata mainly supports reproduction of
sound. Scene Token supports interpretation of the communication scene.

### Q2. Why not just use MASA?

Answer:

MASA is appropriate for efficient spatial audio representation. However, the
research target here is not only whether the sound is spatially reproduced, but
whether the listener can understand a multi-speaker VR conversation. For that
purpose, speaking state and turn state need to be represented explicitly.

### Q3. Does this reduce communication traffic?

Answer:

The prototype logs compact token traffic and object metadata traffic, so
communication cost can be analyzed. However, the main claim should currently be
conversation understanding support. Traffic reduction should be treated as a
secondary analysis until the data is strong.

### Q4. Why are semantic labels manual?

Answer:

The first step is to evaluate whether adding semantic and turn-state information
to spatial audio is useful. Manual/scripted labels make the experiment
controllable. After this baseline is validated, ASR and LLM-based automatic
semantic classification can be added.

### Q5. Why use only 8 directions and 3 distances?

Answer:

The goal of the first evaluation is not high-precision acoustic reconstruction,
but a stable and understandable token representation for conversation support.
8 directions and 3 distances are coarse enough for users to interpret and
simple enough for controlled comparison.

### Q6. What is the novelty?

Answer:

The novelty is integrating spatial metadata and conversation-state metadata into
a discrete Scene Token and using it to reconstruct/support spatial audio
communication in VR.

## Phrases to Memorize

```text
既存研究は「音をどう再現するか」を扱う。本研究は「誰が・どこで・どのような役割で話しているか」をScene Tokenとして表現し，VR空間での会話理解を支援する。
```

```text
MASA is rendering-oriented metadata. Scene Token is communication-understanding-oriented metadata.
```

```text
Audio Token represents the signal. Scene Token represents the communication scene.
```

```text
The current prototype uses manual semantic labels to establish a controlled baseline before introducing ASR or LLM-based automatic classification.
```
