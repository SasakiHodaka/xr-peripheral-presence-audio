# Research Summary

## Title Direction

Scene Token-based semantic spatial audio for VR collaborative work.

## Core Problem

In VR collaborative work, users need to understand who is speaking, where the event is occurring, what object is involved, what the speaker intends, how urgent the event is, and whether the conversation is ongoing or overlapping.

Conventional audio communication and ordinary spatial audio expose sound and direction, but they do not explicitly preserve the collaborative context required for situation awareness.

## Contribution 1: Scene Token Model

The Scene Token Model is an information model for collaborative speech events.

It structures the following elements:

- Speaker
- Spatial
- Object Relation
- Collaborative Intent
- Priority
- Conversation / Event

The goal is to preserve situation awareness by encoding only the information required for collaborative work.

## Contribution 2: Semantic Spatial Audio Rendering

Semantic Spatial Audio Rendering converts Scene Tokens into spatial audio presentation.

The core mechanism is a Rendering Policy:

```text
R : T -> A
```

where:

- `T` is the Scene Token
- `A` is the Audio Strategy

The Audio Strategy is then mapped to concrete audio parameters such as source position, voice, cue, gain, timing, priority, ducking, and fade.

## System Flow

```text
Scene Context
↓
Scene Token Model
↓
Scene Token Generator
↓
Scene Token
↓
Rendering Policy
↓
Audio Strategy
↓
Audio Parameters
↓
Spatial Audio Renderer
```

## Design Roots

The design is grounded in:

- Task Analysis
- Design Requirements
- Modality Requirement Analysis

These are design foundations, not research contributions.

## Evaluation

The proposed system is evaluated at two levels:

1. Direct evaluation
   - speaker recognition
   - direction and distance recognition
   - object recognition
   - intent recognition
   - priority recognition
   - conversation state recognition

2. Final evaluation
   - task completion time
   - error count
   - reaction time
   - NASA-TLX
   - subjective usefulness

## Ground Truth

Ground truth is derived from the task scenario:

```text
Task Scenario
↓
Expected Situation
↓
Ground Truth
↓
Scene Token evaluation
```

This avoids circular evaluation.

## Implementation Scope

The prototype uses existing techniques for:

- speech event detection
- intent acquisition
- priority assignment
- spatial audio rendering

The research focus remains on:

- the Scene Token Model
- the Rendering Policy

