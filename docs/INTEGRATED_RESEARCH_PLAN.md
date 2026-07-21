# Integrated Research and Implementation Plan

Last updated: 2026-07-21

## Research Objective

Build and evaluate a two-user Meta Quest VR training system in which an expert
guides a learner through inspection or maintenance. The system uses collaboration
context and semantic information to select a spatial anchor and presentation style
for live voice, with optional adaptation based on observable guidance need.

## Research Contribution

The contribution is not the use of Meta XR, Photon Fusion, or Photon Voice. Those
packages provide infrastructure. The evaluated contribution is the mapping:

```text
collaboration evidence
  + spoken meaning and urgency
  + task state
  + current guidance need
  -> information selection
  -> spatial anchor and presentation parameters
```

## Target Architecture

| Layer | Technology | Responsibility |
| --- | --- | --- |
| Device | Meta Quest 3 | Tracking, microphone, interaction, display, and audio output |
| XR | OpenXR + Meta XR Interaction SDK | Local rig and task interaction |
| Multiplayer | Photon Fusion 2 | Session, network rig, object state, authority, semantic events |
| Voice | Photon Voice 2 | Live expert/learner voice streams |
| Audio | Meta XR Audio + Unity audio | Spatial rendering and controlled presentation |
| Research | Project-owned code | Acquisition, inference, selection, adaptation, anchors, and logging |

Fusion remains the only shared-state authority. Meta Multiplayer Building Blocks
may be used selectively but must not duplicate Fusion session, spawn, or authority
management.

## Minimum Study Scenario

1. The learner enters a shared maintenance workspace.
2. The expert issues a controlled instruction referring to a tool or equipment
   component.
3. The target may be in front, beside, or behind the learner.
4. The learner locates and manipulates the target.
5. The system records attention, interaction, errors, recovery, and task timing.
6. Critical warnings and correction instructions occur in predefined cases.

The first networked task uses one grabbable workpiece/tool, one correct zone, and
one incorrect/hazard zone before adding a larger maintenance assembly.

## Comparison Conditions

### Minimum three-condition study

1. **Standard Voice:** voice is listener-relative/non-spatial or fixed.
2. **Speaker Spatial Voice:** voice is anchored to the remote expert avatar.
3. **Semantic Spatial Audio:** voice anchor is selected from context and meaning.

### Optional fourth condition

4. **Adaptive Semantic Spatial Audio:** semantic anchoring plus continuous
   guidance-need adaptation.

If participant count or study duration is limited, compare conditions 1-3 and
evaluate adaptation in a separate pilot or follow-up study.

## Semantic Anchor Policy

| Situation | Preferred anchor | Fallback |
| --- | --- | --- |
| Ordinary conversation | Speaker | Listener front |
| Object explanation/instruction | Task object | Speaker |
| Hazard warning | Hazard, with controlled listener-front reinforcement | Speaker |
| Missing/despawned target | Speaker | Listener front |

Anchor transitions require hysteresis/minimum dwell time so that live speech does
not oscillate between positions.

## User Adaptation Policy

Guidance need is continuous and context-dependent. It is not a permanent novice or
expert label.

Evidence that may increase need:

- explicit help request;
- wrong object/tool/action;
- repeated instruction;
- long hesitation;
- failure to orient toward a relevant target.

Evidence that may decrease need:

- correct independent action;
- repeated successful completion;
- rapid correct target acquisition.

Every update must log the evidence, previous value, new value, and resulting
selection/presentation change.

## Data Flow

```text
Quest pose + interaction + task state + live speech
  -> Situation Evidence
  -> Meaning/Importance Inference
  -> SelectionResult
  -> SemanticPacket / Fusion transport
  -> PresentationDecision
  -> Photon Voice Speaker anchor and Meta XR Audio parameters
  -> Objective logs + participant questionnaires
```

## Evaluation Design

- within-participant comparison where practical;
- counterbalanced condition order;
- identical task behavior and controlled instruction content across conditions;
- direction included as an experimental factor;
- role recorded explicitly;
- training trials separated from measured trials;
- primary outcomes fixed before formal collection;
- pilot used to tune thresholds, not the formal-study data.

## Primary Outcomes

1. Instruction-to-correct-action time.
2. Wrong-target/tool/action count.
3. Task success.
4. NASA-TLX.

Secondary outcomes are specified in `docs/LITERATURE_SYNTHESIS.md` and
`docs/EVALUATION_DATA_SPEC.md`.

## Implementation Sequence

1. Resolve Meta XR Interaction SDK and confirm existing builds.
2. Import Fusion 2 and matching Photon Voice.
3. Establish two-Quest session and rig synchronization.
4. Synchronize one task object with deterministic authority.
5. Verify ordinary speaker-anchored Photon Voice.
6. Implement the Fusion adapter for the existing semantic transport contract.
7. Add collaboration-evidence fields and timestamps.
8. Implement anchor resolver and safe fallback behavior.
9. Implement the three comparison conditions.
10. Validate logging and run a two-user pilot.

The detailed engineering checklist and definitions of done are in `TODO.md`.

## Scope Control

Not required for the minimum study:

- neural FOA/audio-token codec;
- large language model as the only inference baseline;
- eye tracking requiring a different headset;
- more than two participants;
- industrial digital twin platform;
- production-scale matchmaking or authentication;
- shared spatial anchors unless the task becomes co-located MR.

## Reproducibility Requirements

- record Unity, Meta XR, Fusion, and Voice versions;
- record Quest model, OS/firmware, audio device, and network setup;
- version every condition and policy parameter;
- log the actual applied audio anchor and gain, not only the requested values;
- keep App IDs/credentials out of public history where appropriate;
- tag the final pilot and formal-study builds.
