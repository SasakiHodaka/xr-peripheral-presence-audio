# NAF Research Plan

## Working Direction

This project should not try to reproduce a full Neural Acoustic Field system first.
The practical goal is to extract useful NAF-inspired elements for real-time peripheral presence cues in Unity.

Core research framing:

```text
Conventional goal: physically correct sound
This project: sound that makes off-screen presence easier to perceive
```

## Most Relevant NAF Families

### Real-Time NAT / Interactive NAF

Use this as the performance direction.

- Compute only the parts needed for the current trial.
- Prefer lightweight models or rule-based approximations first.
- Treat near/off-screen events as higher priority than far or irrelevant events.
- Cache repeated spatial/acoustic states where possible.

Unity implication:

- Prioritize `OutOfView`, `Approaching`, `Crossing`, `Speaking`, and `Near`.
- Avoid expensive full-scene acoustic simulation for every frame.
- Start with simple cue rules before adding neural inference.

### Direction-Aware NAF / DANF

Use this as the spatial direction direction.

- Direction matters more than raw audibility in VR.
- Rear, diagonal rear, and side events need distinct cues.
- Ambisonics or direction-encoded audio can be added later.

Unity implication:

- Current CSV already logs `localX`, `localY`, `localZ`, and `viewAngle`.
- Add derived direction labels next if needed:
  - `front`
  - `left`
  - `right`
  - `rear`
  - `rearLeft`
  - `rearRight`

### Physics-Informed NAF

Use this as the plausibility direction, not as the first implementation target.

- Useful for wall occlusion, room reflection, corridor effects, and natural attenuation.
- Full wave-equation or Helmholtz modeling is out of scope for the current prototype.

Unity implication:

- Add simple interpretable acoustic parameters first:
  - occlusion amount
  - low-pass amount
  - reverb send
  - volume attenuation

### Context-Aware NAF

This is close to the project goal.

- Presence cues should depend on meeting context, not only geometry.
- Group work, focused speech, and background motion can change the cue design.

Unity implication:

- Current `conditionLabel` can represent context or trial condition.
- Future conditions can include:
  - `FocusedSpeech`
  - `SideConversation`
  - `BackgroundMovement`
  - `SilentPresence`

## Current Prototype Mapping

Current Unity conditions:

- `Approach`: forward approaching presence.
- `BackApproach`: rear off-screen approaching presence.
- `Crossing`: lateral front crossing.
- `Speaking`: stationary speaking presence.
- `None`: baseline without demo targets.

Current logs already support:

- participant and trial metadata
- condition labels
- pre-trial exclusion
- trial elapsed time
- target state flags
- distance and view angle
- local target position
- radial and lateral speed

## Paper-Grounded Interpretation

### Learning Neural Acoustic Fields

Core idea:

- Model sound propagation as a linear time-invariant system.
- Learn a continuous mapping from emitter/listener pose pairs to an impulse response.
- Apply the learned impulse response to arbitrary dry sounds by convolution.
- Predicting raw time-domain impulse responses is difficult, so the paper predicts time-frequency representations.
- Local geometric conditioning helps generalize to unseen emitter/listener combinations.

Relevant inputs in the paper:

- listener position
- listener orientation
- left/right ear
- emitter position
- time and frequency coordinates
- learned local geometric features

Useful lesson for this project:

- We should treat the current Unity target/user pair as the emitter/listener pair.
- We do not need to generate full impulse responses yet.
- We can first log and model compact perceptual parameters that approximate parts of an impulse response:
  - gain
  - low-pass cutoff
  - reverb amount
  - direction label
  - presence weight

### Direction-Aware NAF / DANF

Core idea:

- Binaural or monaural RIR metrics are not enough for VR.
- Directional sound fields need inter-channel directional information.
- DANF models first-order Ambisonic RIRs and uses a direction-aware intensity-vector loss.
- Moderate direction loss improves direction-of-arrival without completely damaging acoustic metrics.

Useful lesson for this project:

- Direction should be a first-class output, not just a side effect of volume.
- Our `localX`, `localZ`, `viewAngle`, and `lateralSpeed` logs are the right lightweight proxy for direction.
- The next cue model should explicitly output `directionLabel`.

Recommended direction labels:

- `front`
- `left`
- `right`
- `rear`
- `rearLeft`
- `rearRight`

### Physics-Informed DANF

Core idea:

- FOA channels are physically related.
- The W-channel corresponds to pressure.
- X/Y/Z channels correspond to directional particle velocity.
- Physics-informed priors connect these channels using momentum and continuity equations.

Useful lesson for this project:

- Physics-informed modeling matters when producing full spatial impulse responses.
- For the current Unity prototype, use simple physically plausible constraints instead:
  - farther targets should not get louder without a reason
  - rear/off-screen targets can be filtered differently
  - crossing should change direction smoothly
  - cue parameters should vary continuously, not jump between frames

### NeRAF

Core idea:

- Jointly learn visual radiance fields and acoustic fields.
- Use 3D scene priors from NeRF-derived grids to condition acoustic prediction.
- Acoustic modeling benefits from 3D geometry because sound propagates omnidirectionally and depends on full scene structure, not only visible pixels.

Useful lesson for this project:

- Full NeRF/NeRAF is too large for the current prototype.
- The concept maps well to future context features:
  - room type
  - wall/partition presence
  - open/closed space
  - meeting context
  - occlusion proxy

Current Unity replacement for a full 3D scene prior:

- condition labels
- local target coordinates
- out-of-view state
- distance
- near/crossing/approaching/speaking flags

## What This Project Should Not Do Yet

- Do not train a neural field.
- Do not synthesize full RIRs.
- Do not implement Ambisonics or FOA modeling yet.
- Do not attempt physics-informed PDE losses.
- Do not add NeRF or 3D scene reconstruction yet.

These are later-stage upgrades after the perceptual cue layer is validated.

## Next Implementation Target

The next useful step is an interpretable NAF-inspired cue layer.

Proposed component:

```text
PeripheralAcousticCueModel
```

Inputs:

- `PeripheralDetectionResult.state`
- `distance`
- `viewAngle`
- `radialSpeed`
- `lateralSpeed`
- `localX`, `localZ`
- `conditionLabel`

Outputs:

- `cueType`
- `presenceWeight`
- `directionLabel`
- `volumeGain`
- `lowPassHz`
- `reverbSend`
- `priority`
- `occlusionProxy`
- `directionConfidence`

This keeps the system real-time and explainable while leaving room for later neural replacement.

## Suggested Cue Rules

Initial rule-based model:

- `BackApproach + OutOfView + Approaching`
  - high presence weight
  - rear direction label
  - stronger low-pass or breathing/footstep cue
- `Crossing`
  - lateral movement cue
  - direction label based on `localX` sign and lateral speed
- `Speaking`
  - voice cue
  - moderate presence weight
- `Approach + Near`
  - high presence weight
  - lower need for off-screen enhancement
- `None`
  - baseline, no cue

## NAF Boundary

Do not implement neural acoustic fields yet.
First validate whether these NAF-inspired cue features improve detection or perceived presence.

Only consider a neural model after the rule-based cue layer has stable logs and evaluation targets.
