# Second Project Research Design

## Positioning

This second project should be designed as the direct bridge between the current XR peripheral-presence audio project and the later multimodal CPS project.

The current XR audio project answers:

```text
Can adaptive spatial audio cues improve awareness of peripheral human presence in XR?
```

The second project should answer:

```text
Can a system infer human situation and interaction risk from multiple lightweight signals, then adapt feedback in real time?
```

The later multimodal CPS project can then expand this into:

```text
physical environment
+ human state
+ robot / agent state
+ multimodal sensing
+ real-time intervention
-> closed-loop cyber-physical interaction support
```

## Recommended Theme

Working title:

```text
Multimodal Situation Awareness and Adaptive Feedback for Human-Centered XR/CPS Interaction
```

Shorter title:

```text
Multimodal Adaptive Awareness System for XR-to-CPS Interaction
```

The second project should not jump immediately to a full CPS. It should build and validate the core loop:

```text
multimodal sensing
-> situation inference
-> adaptive feedback
-> human awareness / decision / safety outcome
```

## Core Research Problem

Human users in XR, remote collaboration, robot-assisted work, or smart environments often need to understand events outside their direct attention. A system can support this by sensing the situation and giving adaptive feedback, but poor feedback can become annoying, distracting, or untrustworthy.

The research problem is:

```text
How can a system combine multiple human/environment signals to infer attention-relevant situations and provide adaptive feedback without overloading the user?
```

## Why This Should Come Before Multimodal CPS

Full multimodal CPS requires many difficult pieces at once:

- multimodal sensing
- time synchronization
- situation recognition
- uncertainty handling
- feedback control
- human response measurement
- safety and reliability
- real-world deployment constraints

The second project should isolate the core scientific question before the full CPS:

```text
Does multimodal situation inference improve adaptive feedback compared with single-modal or rule-based feedback?
```

This makes the later CPS project stronger because the feedback and inference logic will already be validated.

## Research Gap

Existing work often treats these areas separately:

- XR awareness support focuses on visual or audio cue design.
- Human activity recognition focuses on classification accuracy, not feedback usefulness.
- CPS research focuses on control and infrastructure, often with limited human-subjective evaluation.
- Multimodal learning often improves perception models, but does not always close the loop with human response.

The gap is the closed human-centered loop:

```text
multimodal inference
-> adaptive cue / intervention
-> measured human awareness and behavior
```

## Research Questions

Primary research question:

```text
RQ1: Does multimodal situation inference improve adaptive feedback for human awareness compared with single-modal and rule-based feedback?
```

Secondary research questions:

```text
RQ2: Which modalities contribute most to situation awareness: spatial audio, gaze/head pose, body motion, proximity, speech, or environment context?

RQ3: How should feedback intensity be adapted under uncertainty to avoid missed events and excessive interruption?

RQ4: Can the system produce interpretable situation labels and feedback parameters suitable for later CPS control?
```

## Hypotheses

```text
H1: Multimodal inference will reduce missed events and response time compared with single-modal inference.

H2: Adaptive feedback intensity will improve awareness while reducing annoyance compared with fixed feedback.

H3: Interpretable intermediate state labels will make the system easier to evaluate and transfer into a CPS setting.

H4: The most valuable modality will depend on event type: gaze/head pose for attention, proximity/motion for collision or approach, speech/audio for social presence, and environment context for ambiguity reduction.
```

## Scope Boundary

The second project should include:

- multimodal sensing or simulated multimodal signals
- situation-state estimation
- adaptive feedback control
- human-centered evaluation

The second project should not yet require:

- real industrial CPS deployment
- robot actuation
- safety-certified control
- large-scale foundation models
- fully autonomous physical intervention

Those are better reserved for the later multimodal CPS project.

## Candidate Application Scenarios

Choose one main scenario first. Do not try to cover all of them in the first study.

### Scenario A: XR Collaborative Awareness

User works in XR while another person or agent moves, speaks, approaches, or needs attention.

Useful because it directly extends the current audio project.

Signals:

- head pose
- target position
- target motion
- speech activity
- gaze or facing direction
- audio cue state

Feedback:

- spatial audio
- visual minimal indicator
- haptic pulse if available

### Scenario B: Human-Robot Shared Workspace

User and robot share a workspace. The system detects approach, crossing, handover readiness, attention mismatch, or risk.

Useful because it connects more directly to CPS and HRI.

Signals:

- human pose
- robot pose
- object position
- gaze/head direction
- speech/activity
- distance/risk zone

Feedback:

- audio warning
- spatial cue
- robot motion modulation
- visual/haptic cue

### Scenario C: Smart Environment Abnormal Event Awareness

User receives feedback about abnormal events in the environment: device activity, human approach, blocked route, or hazard-like event.

Useful for CPS framing, but may be broader and harder to evaluate cleanly.

Signals:

- environment sensors
- camera/depth
- audio
- user location
- system state

Feedback:

- adaptive alerts
- location-aware cueing
- urgency control

Recommended first scenario:

```text
Scenario A first, then Scenario B as the CPS extension.
```

This preserves continuity with the XR peripheral audio project while preparing the CPS transition.

## System Architecture

### Layer 1: Multimodal Observation

Inputs:

- user head pose
- user gaze or facing direction
- target / agent position
- target / agent velocity
- speech activity
- environment context
- optional physiological or interaction signals

### Layer 2: Situation Inference

Intermediate labels:

- `userAttending`
- `targetApproaching`
- `targetBehind`
- `targetSpeaking`
- `targetCrossing`
- `attentionMismatch`
- `interactionRisk`
- `requiresFeedback`

The model should output interpretable states first, not only an end-to-end feedback command.

### Layer 3: Adaptive Feedback

Feedback parameters:

- modality: audio, visual, haptic, or combined
- urgency
- volume / intensity
- spatial direction
- duration
- repetition
- filtering / occlusion / reverb if audio is used

### Layer 4: Human Response

Measured outcomes:

- detection time
- response accuracy
- head-turn or gaze-shift latency
- task interruption
- subjective trust
- annoyance
- workload

## Experimental Conditions

Use this comparison structure:

```text
NoFeedback
FixedFeedback
SingleModalAdaptive
MultimodalAdaptive
```

Optional fifth condition:

```text
MultimodalAdaptiveWithUncertaintyControl
```

Only add the fifth condition if the experiment remains manageable.

## Evaluation Metrics

### Objective

- detection time
- missed event rate
- false alarm rate
- localization / event classification accuracy
- task performance impact
- response latency after cue onset

### Subjective

- awareness support
- annoyance
- trust
- workload
- naturalness
- perceived usefulness

Recommended questionnaires:

- NASA-TLX for workload if task load matters.
- Short custom 7-point ratings for awareness, annoyance, trust, and usefulness.

## Dataset Plan

Each sample or timestep should store:

- participant ID
- scenario ID
- feedback condition
- timestamp
- user pose
- target / agent pose
- target / agent velocity
- speech activity
- environment label
- inferred situation labels
- feedback modality
- feedback parameters
- user response
- response correctness
- subjective ratings

This dataset becomes the foundation for the later multimodal CPS project.

## Model Plan

Start simple:

1. Rule-based baseline.
2. Tabular model for situation labels.
3. Temporal model for event sequences.
4. Multimodal model after the logging schema is stable.

Candidate models:

- Random Forest / Gradient Boosting for initial tabular baselines.
- Temporal CNN, GRU, or Transformer encoder for time-series signals.
- Late fusion model for interpretable modality contribution.

Avoid starting with a large black-box multimodal model. It will make the research harder to explain and evaluate.

## Relationship To Current XR Audio Project

Current project:

```text
peripheral state
-> adaptive spatial audio cue
-> awareness outcome
```

Second project:

```text
multimodal state
-> situation inference
-> adaptive multimodal feedback
-> awareness / workload / trust outcome
```

Later multimodal CPS:

```text
multimodal sensing
-> situation inference
-> feedback + physical/system control
-> human-system co-adaptation
```

## Expected Contributions

The second project should contribute:

1. A multimodal situation-state schema that can bridge XR awareness support and CPS control.
2. A comparison of fixed, single-modal adaptive, and multimodal adaptive feedback.
3. Evidence about which modalities improve awareness without increasing annoyance or workload.
4. A logged dataset structure reusable for the later multimodal CPS project.

## Minimum Viable Study

Recommended first study:

```text
Participants: 12-20
Scenario: XR collaborative awareness
Events: approach, behind approach, crossing, speaking/requesting attention
Conditions: NoFeedback, FixedFeedback, SingleModalAdaptive, MultimodalAdaptive
Primary metrics: detection time, missed event rate
Secondary metrics: localization accuracy, workload, annoyance, trust
```

This study is feasible and directly extends the current Unity prototype.

## Path To Multimodal CPS

After this second project, extend in this order:

1. Replace XR-only targets with physical or robot-agent states.
2. Add external sensors or simulated CPS sensor streams.
3. Add system-side action, not only feedback.
4. Add uncertainty-aware intervention policy.
5. Evaluate human-system closed-loop performance.

The key transition is:

```text
from adaptive feedback
to adaptive feedback + cyber-physical action
```

