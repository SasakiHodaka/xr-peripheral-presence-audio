# Research Design

## Working Title

Adaptive Spatial Audio Cues for Peripheral Human Presence Awareness in XR

## Core Problem

In XR, users can miss people who are outside the central visual field, behind them, or partially occluded. Visual indicators can solve awareness, but they also add visual clutter and can reduce immersion. Spatial audio is a promising alternative because it can signal direction, motion, and social presence without occupying the visual channel.

The research problem is:

```text
How should an XR system adapt spatial audio cues so that users notice peripheral human presence without reducing naturalness or immersion?
```

## Research Gap

Existing work covers several related areas, but they are usually studied separately:

- Peripheral awareness in VR often uses visual indicators, halos, arrows, or simple spatial cues.
- Spatial audio research often focuses on localization, navigation, or acoustic realism.
- Audio-visual environment acoustics work estimates room acoustics or impulse responses, but does not usually connect those estimates to human-presence cue design.
- Human-robot interaction and proxemic sound work studies functional sound for social awareness, but not specifically XR peripheral presence with adaptive acoustic control.

This project connects these areas through a practical cue-control loop:

```text
peripheral human state
+ environment acoustic profile
-> adaptive spatial audio cue
-> awareness / localization / naturalness / immersion outcomes
```

## Research Question

Primary research question:

```text
RQ1: Do adaptive spatial audio cues improve awareness of peripheral human presence in XR compared with no cue, fixed cue, and state-based cue conditions?
```

Secondary research questions:

```text
RQ2: Does environment-adaptive cue control improve perceived naturalness and immersion compared with state-based cue control?

RQ3: Which target states benefit most from adaptive cueing: approaching, behind-user approach, crossing, or speaking?

RQ4: Can compact logged state and cue parameters provide a useful dataset for training a cue-control model?
```

## Hypotheses

```text
H1: State-based spatial audio cues will reduce detection time and missed detections compared with NoCue.

H2: EnvironmentAdaptiveCue will improve perceived naturalness and immersion compared with FixedCue and StateBasedCue.

H3: BackApproach and OutOfView+Approaching cases will show the largest benefit from adaptive spatial audio cues.

H4: A compact cue-control model can reproduce rule-based cue parameters from logged Unity data with low prediction error, enabling later replacement by learned estimators.
```

## Proposed Contribution

The project should claim three contributions:

1. An XR prototype that detects peripheral human-presence states and maps them to interpretable spatial audio cues.
2. A comparison framework for `NoCue`, `FixedCue`, `StateBasedCue`, and `EnvironmentAdaptiveCue` conditions.
3. A bridge from environment acoustics estimation to cue control through a compact `EnvironmentAcousticProfile`, rather than trying to render full acoustic fields first.

The third contribution is the long-term AI link. The first paper or thesis chapter should not depend on a full neural acoustic field. It should validate the cue-control structure first.

## System Model

### Input State

The Unity prototype logs:

- target identity
- target state flags: `OutOfView`, `Approaching`, `Speaking`, `Gazing`, `Near`, `Crossing`
- distance
- view angle
- radial speed
- lateral speed
- target position in user-local coordinates

### Cue Prediction

The cue model outputs:

- `cueType`: `None`, `Footstep`, `Voice`, or `AmbientPresence`
- `presenceScore`
- `volumeGain`
- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

### Environment Profile

The compact profile should be treated as the Unity-facing output of later AI estimation:

- `roomScale`
- `materialClass`
- `reverbAmount`
- `occlusionStrength`
- `distanceAttenuation`
- `rt60`
- `drr`

This is intentionally lower-dimensional than a full room impulse response. It is easier to interpret, easier to validate in Unity, and enough for early cue adaptation.

## Experimental Conditions

Use a two-axis design:

### Target Scenario

- `Approach`: target approaches from front or front-peripheral area.
- `BackApproach`: target approaches from behind.
- `Crossing`: target crosses in front of the user.
- `Speaking`: target speaks while near or peripheral.
- `None`: no target, used to measure false positives and annoyance.

### Cue Condition

- `NoCue`: no audio cue.
- `FixedCue`: same cue intensity/filtering regardless of state or environment.
- `StateBasedCue`: cue type and strength depend on target state, distance, and speed.
- `EnvironmentAdaptiveCue`: state-based cueing plus environment acoustic profile adaptation.

Recommended first experiment:

```text
4 target scenarios x 4 cue conditions
```

Exclude `None` from the full factorial if the session becomes too long; use it as a short catch-trial condition.

## Dependent Measures

### Objective Measures

- detection time: time from first relevant target state to participant response
- miss rate: target event not reported within a time limit
- localization error: angular or categorical direction error
- false positive rate: response when no target event exists
- head-turn latency: time until user turns toward the target

### Subjective Measures

Use short post-condition ratings:

- awareness support
- naturalness
- immersion
- annoyance
- discomfort
- confidence in target direction

Recommended scale:

```text
7-point Likert scale
```

### System Measures

- cue type distribution
- cue volume distribution
- low-pass cutoff distribution
- reverb amount distribution
- occlusion gain distribution
- playback-active ratio

These measures are important because they explain why one condition works better than another.

## Minimum Viable Study

For a first defensible study, keep the design simple:

```text
Participants: 12-20
Design: within-subject
Target scenarios: BackApproach, Crossing, Speaking
Cue conditions: NoCue, FixedCue, StateBasedCue, EnvironmentAdaptiveCue
Primary metric: detection time
Secondary metrics: localization accuracy, naturalness, immersion, annoyance
```

The first study should focus on cue-control effectiveness, not on proving the full AI estimator.

## Data Logging Plan

Each trial should log:

- participant ID
- target scenario condition
- cue condition
- trial ID
- target state and kinematics
- predicted cue parameters
- actual playback parameters
- participant response timestamp
- participant response direction
- post-trial or post-condition ratings

Current implementation already logs most system-side values. The missing pieces for the user study are participant response and subjective ratings.

## Analysis Plan

### Main Comparisons

Compare cue conditions on:

- detection time
- miss rate
- localization error
- naturalness
- immersion
- annoyance

Expected analysis:

```text
within-subject repeated-measures comparison
```

If sample size is small, report effect sizes and confidence intervals rather than relying only on p-values.

### Important Interaction

The most important interaction is:

```text
target scenario x cue condition
```

BackApproach should benefit more from spatial audio than front Approach. Speaking may already be salient if voice is present, so its improvement may be smaller or mainly subjective.

## AI Extension Plan

The AI component should be introduced in stages:

1. Train a tabular baseline from Unity logs to reproduce rule-based cue outputs.
2. Add manually controlled `EnvironmentAcousticProfile` labels.
3. Generate SoundSpaces-style simulated audio-visual samples.
4. Train an environment estimator that predicts compact acoustic profile values.
5. Feed estimated profile values back into Unity cue control.

This staged plan keeps the research defensible even before the full audio-visual estimator is complete.

## Boundary Decisions

Do not frame the first study as:

```text
Learning a full neural acoustic field for XR
```

Frame it as:

```text
Adaptive cue control for peripheral human-presence awareness, with an extensible bridge to learned environment acoustics.
```

This is narrower, testable, and better aligned with the current Unity prototype.

## Next Required Implementation

Before running participants, add:

- participant response input
- response timestamp logging
- response direction logging
- trial randomization
- per-condition summary export
- simple questionnaire capture or external form mapping

