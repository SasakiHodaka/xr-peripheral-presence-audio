# Literature Synthesis for Semantic Spatial Audio

Last updated: 2026-07-21

## Scope

This document maps the local literature corpus to the design and evaluation of
an expert-to-learner VR inspection/maintenance system. The papers themselves are
not copied into this repository. Local filenames are recorded only for
traceability.

The proposed research combines three established ideas:

```text
Workspace awareness
  -> observable collaboration context
  -> goal-oriented semantic selection
  -> user-adaptive spatial audio presentation
  -> task, perception, workload, and communication evaluation
```

## Primary Literature

### Gutwin and Greenberg: Workspace Awareness

Local source: `要求条件/2002-DescriptiveFramework.JCSCW.pdf`

The workspace-awareness framework defines awareness as an up-to-the-moment
understanding of another person's interaction with a shared workspace. Its core
design questions are what collaborators need to know, how they obtain that
knowledge, and how it supports coordination.

Use in this research:

- justify acquisition of participant location, action, task object, ownership,
  current step, and anticipated next action;
- define Semantic Spatial Audio as awareness support rather than only an audio
  effect;
- explain why voice content and speaker coordinates alone are insufficient for
  remote training;
- evaluate whether the presentation improves coordination and opportunities for
  assistance.

Required implementation fields derived from this framework:

```text
participantId, role, pose, speakingState, currentAction,
heldObjectId, gazeOrAttentionTargetId, currentTaskStep,
nextExpectedAction, helpState, errorState
```

### Chandio et al.: What Sensors See, What People Feel

Local source: `要求条件/2504.16373v3.pdf`

This work separates three complementary nodes: subjective collaboration
experience, sensor-based behavior, and task performance. It cautions against
treating sensor traces as a complete description of how collaboration was
experienced.

Use in this research:

- acquire speaking time, turn count, participation variance, shared attention,
  participant distance, interaction balance, revisions, and completion time;
- pair behavioral logs with subjective measures rather than inferring user state
  from sensors alone;
- treat adaptation evidence as uncertain observations, not fixed novice/expert
  labels;
- evaluate contribution awareness, conversational support, shared attention
  awareness, cognitive load, and group collaboration.

### Zaman et al.: MRMAC

Local source: `要求条件/ismar2023-zaman.pdf`

MRMAC provides an implementation and evaluation example for asymmetric remote
collaboration. It compares conventional communication with progressively richer
immersive collaboration support and evaluates both task outcomes and experience.

Use in this research:

- support the asymmetric expert/learner role design;
- use controlled conditions with the same underlying task;
- measure task success, completion time, NASA-TLX, SUS, spatial presence, social
  presence, co-presence, mutual attention, mutual understanding, preference, and
  qualitative feedback;
- counterbalance condition order and report role effects separately.

### Harada and Ohyama: 360-degree Visual Guidance

Local source: `要求条件/Quantitative_evaluation_of_visual_guidance_effects.pdf`

This work demonstrates that guidance effectiveness depends on target direction
and distinguishes guidance-recognition time from target-search time. It also uses
direction-specific analysis rather than a single overall response time.

Use in this research:

- evaluate front, side, and rear targets separately;
- record cue onset, head-turn onset, target-facing time, target selection, and
  correct-action completion;
- retain the current eight-direction Scene Token representation;
- distinguish rapid cue recognition from successful task-object acquisition;
- consider directional interaction effects in the statistical model.

### Wang et al.: Goal-oriented Semantic Communication for the Metaverse

Local source: `Goal-oriented_Semantic_Communication_for_the_Metaverse_Application.pdf`

This paper frames semantic communication around transmitting information needed
for a goal rather than reproducing source data exactly. Its concrete method is
visual/NeRF-oriented, so the algorithm is not transferred directly.

Use in this research:

- define the goal as correct, timely completion of a collaborative maintenance
  task;
- justify selection and suppression of task-relevant semantic events;
- compare transmitted event count and bytes alongside task effectiveness;
- distinguish semantic success from waveform or coordinate fidelity.

### Sudarsanam et al.: FOA Tokenizer

Local source: `2510.22241v1.pdf`

FOA Tokenizer demonstrates a discrete low-bitrate representation of first-order
ambisonic audio with a spatial-consistency objective. It is relevant to future
generative XR communication but is outside the minimum study implementation.

Use in this research:

- motivate future integration of Audio Tokens and Spatial Perception Tokens;
- provide a future direction for preserving directional cues in compressed audio;
- do not make neural FOA coding a dependency of the first two-user study.

## Supporting Literature

### Spatial Perception Token research draft

Local source: `生成型XR通信に向けたVR会議における空間知覚トークンの設計と評価.pdf`

The draft organizes spatial information into physical, perceptual, and social
layers. It directly informs candidate fields but should not be treated as strong
independent empirical evidence.

Candidate fields:

```text
Physical: direction, distance, motion
Perceptual: outOfView, occluded, approaching, receding
Social: speakerRole, turnState, attentionState, urgency
```

Candidate ablation:

1. coordinate-based spatial audio;
2. physical tokens only;
3. physical and perceptual tokens;
4. full physical, perceptual, and social tokens.

### Chang et al.: 6G-enabled Edge AI for Metaverse

Local source: `2204.06192v1.pdf`

Use this survey only for deployment discussion: Quest performs sensing and
rendering, while heavier speech/meaning inference may run on a nearby PC or edge
service. The study must measure the resulting latency rather than assume an ideal
6G environment.

### SC-GIR

Local source: `SC-GIR Goal-oriented Semantic Communication via Invariant Representation Learning for Image Transmission.pdf`

This paper supports the general principle of compact task-useful representation,
but its image and machine-to-machine method is not a direct implementation target.

## Low-priority or Out-of-scope Sources

| Local file | Decision |
| --- | --- |
| `3415190.pdf` | Politeness and cultural language adaptation are possible future work, not a current spatial-audio variable. |
| `switch2_低遅延chat_260707_news015.pdf` | Useful engineering analogy for separating realtime media and service state, but not primary scholarly evidence. |
| `Event-Triggered_Output_Feedback_...pdf` | Control-theory topic is outside the current research question. |
| `Massive_MIMO_With_Cauchy_Noise_...pdf` | Physical-layer channel estimation is outside the Unity/Photon application-layer study. |

## Resulting Research Gap

Prior work separately provides:

- principles for workspace awareness;
- sensor and task indicators for collaboration;
- immersive remote-collaboration systems and measures;
- directional guidance evaluation;
- goal-oriented semantic selection; and
- discrete spatial-audio representation.

The target gap is the combination of these elements in live expert-to-learner VR
training: selecting task-relevant spoken information from collaboration context
and adaptively choosing where and how that live voice should be spatially
presented.

## Resulting Research Questions

- **RQ1:** Does semantic anchor selection improve task-object acquisition and
  correct action compared with standard voice and speaker-position spatial voice?
- **RQ2:** Which context elements—physical, perceptual, social, and task state—are
  necessary for effective spatial presentation?
- **RQ3:** Does adaptation based on observable guidance need reduce errors and
  workload without reducing awareness or perceived naturalness?
- **RQ4:** Can selection reduce transmitted semantic events/bytes while preserving
  or improving task outcomes?

## Resulting Hypotheses

- **H1:** Semantic anchoring reduces instruction-to-target and
  instruction-to-correct-action time, especially for out-of-view targets.
- **H2:** Semantic anchoring reduces wrong-target and wrong-tool actions.
- **H3:** User-adaptive presentation reduces NASA-TLX relative to a non-adaptive
  semantic condition while preserving intelligibility and presence.
- **H4:** Goal-oriented selection reduces semantic-event count and bytes without a
  material reduction in task success or instruction comprehension.

## Measures to Implement

### Primary objective measures

- instruction-to-correct-action time;
- target acquisition time;
- wrong-target, wrong-tool, and wrong-operation count;
- task completion time and success;
- missed/repeated instruction count.

### Secondary objective measures

- cue-to-head-turn onset;
- cue-to-target-facing time by direction;
- speaking time, turn count, and participation balance;
- shared-attention duration;
- help requests, hesitation, revisions, and recovery time;
- semantic events and bytes sent/suppressed;
- semantic and voice transport latency.

### Subjective measures

- NASA-TLX;
- SUS;
- intelligibility and localization confidence;
- spatial/social presence and co-presence;
- mutual attention and mutual understanding;
- perceived contribution and conversational support;
- preference and qualitative feedback.

## Immediate Implementation Consequences

1. Extend the shared state with current action, held object, task step, expected
   action, attention target, help, and error evidence.
2. Add timestamps for instruction, head-turn onset, target facing, selection, and
   correct action.
3. Implement speaker, target, hazard, and listener-front audio anchors.
4. Preserve standard voice and speaker-position voice as baselines.
5. Log objective behavior and collect subjective measures separately.
6. Analyze direction as a factor rather than averaging all target locations.
7. Keep neural audio coding, politeness adaptation, and large-scale networking out
   of the minimum study.
