# XR Research Continuation Notes

Date: 2026-06-09

## Current Research Position

This project should stay focused on adaptive spatial audio cues for peripheral human-presence awareness in XR.

The strongest framing is:

```text
XR user attention is visually limited.
Peripheral or occluded people can be missed.
Spatial audio can provide fast directional and social-presence cues.
The cue should be adaptive, because the best cue depends on target state, environment, and task risk.
```

The current Unity prototype already supports the first technical claim:

```text
target state + local kinematics + cue condition + environment profile
-> cue type and playback parameters
-> 3D audio output and CSV log
```

The next research move is not to claim a full acoustic-field AI system. The next move is to make the cue labels defensible by connecting them to perception and task performance.

## Updated Related Work Signals

### Spatial Audio for Rapid XR Attention

Kim, Dey, and Kaufman, "Evaluating Spatialized Auditory Cues for Rapid Attention Capture in XR," arXiv 2026 / IEEE VRW 2026 author version:

- Directly supports the idea that brief spatial audio can guide attention without occupying visual bandwidth.
- Also warns that audio alone is coarse and can be insufficient for complex or high-risk tasks.
- Short calibration improved perception of audio signals.

Use in this project:

```text
Do not evaluate only whether a user hears the cue.
Evaluate whether the cue gives enough coarse direction for rapid attention shift.
Consider a short calibration or practice phase before the main trial.
Do not overclaim precise localization from audio alone.
```

Source: https://arxiv.org/abs/2601.21264

### Spatial Audio Placement in XR

Cho et al., "Auptimize: Optimal Placement of Spatial Audio Cues for Extended Reality," UIST 2024:

- Shows that XR spatial audio can guide users to events such as notifications and approaching avatars.
- Emphasizes human localization limitations, including angular error and front-back confusion.
- Proposes that the audio source location may be separated from the visual object's true location to reduce identification errors.

Use in this project:

```text
Do not assume cue source position must always equal target position.
For behind-user or front-back-confusable cases, test an adjusted cue position or enhanced lateralization.
Log both target position and emitted audio position if adjusted cue placement is introduced.
```

Source: https://arxiv.org/abs/2408.09320

### Functional Sound in HRI and Audio AR

Smith and Kennedy, "The Role of Consequential and Functional Sound in Human-Robot Interaction: Toward Audio Augmented Reality Interfaces," arXiv 2025, revised 2026:

- Treats robot sound and designed auditory cues as part of HRI.
- Reports a mixed-methods study with localization of augmented spatial audio and collaborative handover tasks.
- Suggests augmented spatial audio can carry task-relevant information and reduce discomfort in interaction.
- Notes that lateral cues were easier than frontal cues in their study.

Use in this project:

```text
Frame cue design as functional sound, not just decorative presence audio.
Separate lateral, front, and rear direction performance in analysis.
For CPS/HRC framing, connect peripheral human or robot awareness to collaborative safety and social acceptability.
```

Source: https://arxiv.org/abs/2511.15956

### MR Remote Collaboration

Nokia Bell Labs, "The Effect of Spatial Auditory and Visual Cues in Mixed Reality Remote Collaboration," 2020:

- Spatialized voice and auditory beacons improved spatial perception compared with non-spatialized audio.
- Hybrid audio and visual cues improved task performance, co-presence, and spatial awareness in an unmodified office.

Use in this project:

```text
Spatial audio should be compared against no cue and non-adaptive cue baselines.
Co-presence and spatial awareness are legitimate dependent measures, not only detection time.
The strongest future extension is audio + minimal visual confirmation, not audio-only maximalism.
```

Source: https://www.nokia.com/bell-labs/publications-and-media/publications/the-effect-of-spatial-auditory-and-visual-cues-in-mixed-reality-remote-collaboration/

### Peripheral Avatar Awareness

Ji, Cochran, and Zhao, "VRBubble: Enhancing Peripheral Awareness of Avatars for People with Visual Impairments in Social Virtual Reality," ASSETS 2022:

- Strong direct match to peripheral awareness of surrounding avatars.
- Uses spatial audio feedback based on proxemic distance zones.
- Shows improved avatar awareness, while also warning that audio feedback can become distracting in crowded environments.

Use in this project:

```text
Use distance zones as one interpretable cue-control baseline.
Measure annoyance and distraction, especially for multi-target or frequent-cue cases.
Do not optimize only for faster detection; naturalness and cognitive load matter.
```

Source: https://arxiv.org/abs/2208.11071

### Simulation-Based Audio-Visual Data

SoundSpaces and SoundSpaces 2.0:

- Provide a strong reference for simulation-based audio-visual data generation.
- SoundSpaces includes large-scale RIR-based audio renderings over Matterport3D and Replica scenes.
- SoundSpaces 2.0 adds real-time acoustic simulation, continuous spatial rendering, configurable microphones/materials, and generalization to arbitrary scenes.

Use in this project:

```text
Use SoundSpaces as the analogy for systematic condition generation.
Keep the distinction clear: RIR labels can be physically simulated, but cueType labels require human evaluation.
The project should generate situations first, then evaluate candidate cues, then train the cue-control model.
```

Source: https://soundspaces.org/

### Spatial Computing and HRC Framing

Delmerico et al., "Spatial Computing and Intuitive Interaction: Bringing Mixed Reality and Robotics Together," IEEE Robotics & Automation Magazine / arXiv 2022:

- Frames MR devices as egocentric sensing and spatial-computing interfaces for HRI.
- Supports the project's CPS/HRC context: XR systems can understand space and human action, then provide intuitive interaction support.

Use in this project:

```text
Position the Unity prototype as a spatial-computing interface for shared-space awareness.
The cue system is not only an audio effect; it is an attention and safety layer for human-aware XR/CPS.
```

Source: https://arxiv.org/abs/2202.01493

## Refined Research Gap

The gap should be stated as:

```text
Prior XR work shows spatial audio can support attention, localization, navigation, collaboration, and avatar awareness.
Prior audio-visual simulation work can generate acoustic labels such as RIR.
However, little work connects simulated peripheral human-presence situations to evaluated, adaptive sound-cue labels for XR awareness.
```

This project's contribution is the bridge:

```text
simulation-generated peripheral situations
+ candidate functional spatial audio cues
+ human/task evaluation
-> cue labels
-> adaptive Unity cue-control model
```

## Updated Hypotheses

Recommended hypotheses for the next study:

```text
H1: State-based and environment-adaptive spatial audio cues reduce detection time and missed detections compared with NoCue.

H2: Environment-adaptive cue control improves perceived naturalness and immersion compared with FixedCue when the acoustic profile changes.

H3: BackApproach and OutOfView+Approaching situations benefit more from adaptive cueing than front/peripheral Approach situations.

H4: Direction errors will be higher for front-back cases than lateral cases; adaptive cue placement or filtering should reduce this error.

H5: Evaluation-derived cue labels produce a more defensible cue-control model than developer/rule labels because labels are tied to detection, localization, naturalness, and discomfort.
```

## Experimental Design Update

### Minimum Study

Keep the first human study small and controlled:

```text
Design: within-subject
Participants: 12-20
Target scenarios: BackApproach, Crossing, Speaking
Cue conditions: NoCue, FixedCue, StateBasedCue, EnvironmentAdaptiveCue
Primary metric: detection time
Secondary metrics: localization direction, miss rate, naturalness, immersion, annoyance, confidence
```

### Add Direction-Specific Analysis

The literature makes direction-specific analysis important.

Add analysis groups:

```text
lateral: left/right
rear: behind or rear-peripheral
front: front or front-peripheral
crossing: lateral motion across field
```

Report:

```text
detection time by scenario x cue condition
direction error by direction group x cue condition
annoyance by cue condition
naturalness by cue condition
```

### Add Short Calibration

Before the main trial, add a short practice/calibration phase:

```text
left cue
right cue
rear cue
front cue
footstep cue
ambient presence cue
voice cue
```

Reason:

```text
Recent XR spatial-audio work indicates that short feedback training can improve users' interpretation of auditory direction.
```

## Cue Design Implications

### Candidate Cues

Keep the candidate set:

```text
Footstep
Voice
AmbientPresence
ClothingRustle
Breathing
None
```

Recommended initial mapping:

```text
BackApproach -> Footstep or AmbientPresence
Crossing -> Footstep with lateral motion
Speaking -> Voice or Voice + low AmbientPresence
Near but not visible -> Breathing or ClothingRustle
No target -> None
```

### Cue Placement

Add an optional future parameter:

```text
emittedLocalX
emittedLocalY
emittedLocalZ
cuePositionStrategy
```

Strategies:

```text
TargetPosition
LateralizedProxy
RearEnhancedProxy
UserRelativeWarningZone
```

This follows the Auptimize insight that the perceptually optimal audio position may not always be the physical target position.

Do not implement this before the current response logging is stable, but reserve it in the research plan.

## Immediate Implementation Priorities

The next implementation work should support evaluation, not add more AI complexity.

1. Add participant response logging:
   - response timestamp
   - response direction
   - confidence
   - false positive marker

2. Add calibration/practice trial support:
   - non-logged or separately marked practice trials
   - known cue direction
   - simple feedback

3. Add generated situation grid:
   - distance
   - direction group
   - approach speed
   - speaking
   - crossing
   - view state

4. Add candidate cue trial mode:
   - present multiple cue candidates for the same situation
   - log candidate cue type and playback parameters
   - collect rating values

5. Add evaluation-to-label export:
   - selectedCueLabel
   - computedPresenceScore
   - computedVolumeGain
   - directionError
   - reactionTime

## Next Paper/Thesis Framing

Recommended title:

```text
Adaptive Spatial Audio Cues for Peripheral Human-Presence Awareness in XR
```

Recommended contribution wording:

```text
We present a Unity-based XR prototype that detects peripheral human-presence states and adapts functional spatial audio cues.
We propose a simulation-and-evaluation pipeline that converts generated peripheral situations and candidate cue performance into cue-control labels.
We evaluate whether adaptive spatial audio improves detection time, localization, naturalness, and immersion compared with no cue, fixed cue, and state-based cue baselines.
```

Avoid this framing for the first study:

```text
We learn a full neural acoustic field for XR.
```

Use this instead:

```text
We learn and evaluate adaptive cue-control labels for functional spatial audio in XR.
```

## One-Sentence Research Direction

```text
This research investigates how XR systems can use adaptive functional spatial audio to make users aware of peripheral or occluded human presence while preserving naturalness and immersion.
```

