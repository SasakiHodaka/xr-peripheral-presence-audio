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

RQ5: Can simulation-generated situations and perceptual evaluation produce more reliable cue labels than labels selected only by the developer?
```

## Hypotheses

```text
H1: State-based spatial audio cues will reduce detection time and missed detections compared with NoCue.

H2: EnvironmentAdaptiveCue will improve perceived naturalness and immersion compared with FixedCue and StateBasedCue.

H3: BackApproach and OutOfView+Approaching cases will show the largest benefit from adaptive spatial audio cues.

H4: A compact cue-control model can learn evaluated cue labels from generated situation data with low prediction error.

H5: Evaluation-derived cue labels will provide stronger evidence than developer-selected subjective labels because they are tied to detection time, localization accuracy, clarity, naturalness, and discomfort.
```

## Proposed Contribution

The project should claim three contributions:

1. An XR prototype that detects peripheral human-presence states and maps them to interpretable spatial audio cues.
2. A simulation-and-evaluation pipeline that converts generated human-presence situations and candidate cue performance into cue labels.
3. A cue-control learning model that predicts `cueType`, `presenceScore`, and `volumeGain` from situation parameters.

Environment acoustics estimation and `EnvironmentAcousticProfile` remain long-term extensions. The first paper or thesis chapter should not depend on a full neural acoustic field. It should validate the cue-label generation and cue-control structure first.

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

Use a three-part design: generated target situation, cue candidate, and later cue condition.

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

### Cue Candidate for Label Generation

For cue-label generation, each generated situation should be paired with several candidate cues:

- `Footstep`
- `Voice`
- `AmbientPresence`
- `ClothingRustle`
- `Breathing`
- `None`

The cue candidate with the best combined evaluation result becomes the target label for that situation.

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

For cue-label generation, each sample should also log:

- generated situation parameters: distance, direction, view state, approach speed, speaking, crossing
- cue candidate type and playback parameters
- localization accuracy
- reaction time
- approach recognition
- clarity rating
- naturalness rating
- discomfort or annoyance rating
- selected cue label
- computed `presenceScore`
- computed `volumeGain`

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

## Cue-Label and AI Training Plan

The AI component should be introduced in stages:

1. Use the current developer-selected cue labels only as an initial prototype baseline.
2. Generate many Unity situations by varying distance, direction, approach speed, speaking, crossing, and view state.
3. Present multiple cue candidates for each situation.
4. Evaluate the candidates using detection time, localization accuracy, approach recognition, clarity, naturalness, and discomfort.
5. Convert the best-performing candidate into the `cueType` label and compute `presenceScore` and `volumeGain`.
6. Train a tabular baseline, then a small NN or MLP, to predict cue labels from situation parameters.
7. Compare the developer-label model, rule-based model, and evaluation-label model.
8. Add environment-acoustic conditioning only after the cue-label dataset is reliable.

This plan follows the data-generation idea behind SoundSpaces, SoundSpaces 2.0, Meta Audio Simulator, Neural Acoustic Fields, and self-supervised learning, but it keeps a key distinction clear: those systems can use physically simulated acoustic targets such as RIR, while this project must evaluate cue labels because cue usefulness depends on human perception.

This staged plan keeps the research defensible even before the full audio-visual estimator is complete.

## Boundary Decisions

Do not frame the first study as:

```text
Learning a full neural acoustic field for XR
```

Frame it as:

```text
Simulation and evaluation based cue-control learning for peripheral human-presence awareness in XR.
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
- generated situation condition grid
- candidate cue selection and playback logging
- evaluation-to-label dataset export
