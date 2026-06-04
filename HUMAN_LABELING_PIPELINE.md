# Human-Labeled Peripheral Cue Learning Pipeline

This document defines the primary research pipeline for the current project.

The core goal is not to reproduce physically correct sound first. The first research target is to learn which peripheral presence cue helps users notice and understand off-screen people in VR.

## Human-Machine Research Balance

This project should be framed as a hybrid human-machine cue learning system.

Human evaluation defines what the system should optimize:

- which cue is easiest to notice
- which cue feels natural
- which cue is not annoying
- which cue helps direction judgment
- how strongly peripheral presence should be expressed

Machine learning handles scale and generalization:

- generating many controlled Unity situations
- extracting distance, angle, speed, and visibility features
- predicting `cueType`, `PresenceScore`, and `volumeGain`
- estimating environment context later
- generalizing to unknown situations and held-out participants

The intended balance is:

```text
human perception labels
-> mathematical cue targets
-> neural cue prediction
-> environment-adaptive machine estimation
```

In short, humans define the meaning of the correct cue, and the machine learns how to apply it in new situations.

## Core Pipeline

```text
Unity situation generation
-> multiple peripheral cue candidates
-> human-subject evaluation
-> best cue label per situation
-> mathematical PresenceScore / volumeGain targets
-> neural model training
-> unknown-situation evaluation
```

## Step 1: Generate Situation Patterns In Unity

Use Unity to generate controlled peripheral-person situations.

Initial conditions:

- `Approach`
- `BackApproach`
- `Crossing`
- `Speaking`
- `None`

Later conditions:

- `RearLeftApproach`
- `RearRightApproach`
- `SidePassing`
- `NearSilentPresence`
- `OccludedSpeaking`

Input features to log:

- state flags: `outOfView`, `approaching`, `speaking`, `gazing`, `near`, `crossing`
- geometry: `distance`, `viewAngle`, `localX`, `localY`, `localZ`
- motion: `radialSpeed`, `lateralSpeed`
- trial metadata: `participantId`, `conditionLabel`, `trialId`, `cueCandidate`

## Step 2: Present Multiple Cue Candidates

For each situation, present multiple sound candidates.

Initial cue candidates:

- `NoCue`
- `Footstep`
- `Breathing`
- `ClothRustle`
- `Voice`
- `AmbientPresence`
- `MixedCue`

The experiment should avoid always making the loudest or most intrusive cue win. Cue volume and timing should be controlled enough that differences reflect cue suitability, not only salience.

## Step 3: Measure Recognition Performance

Objective metrics:

- detection success
- reaction time
- direction judgment accuracy
- missed detections
- false responses

Subjective metrics:

- awareness
- naturalness
- annoyance
- discomfort
- immersion
- confidence

## Step 4: Convert Results Into Cue Labels

For each situation pattern, select the best cue candidate as the target label.

Recommended label score:

```text
cueEffectiveness =
  detectionSuccess
  + directionAccuracy
  - normalizedReactionTime
  + awarenessRating
  + naturalnessRating
  - annoyanceRating
  - discomfortRating
```

The cue with the highest `cueEffectiveness` becomes the target `cueType` label for that situation.

Keep the raw metrics even after label generation. They are needed to explain why a label was selected.

## Step 5: Define PresenceScore And VolumeGain

`PresenceScore` should represent how strongly the system should express peripheral presence.

Candidate formula:

```text
presenceScore =
  detectionNeed
  * cognitiveBenefit
  * comfortFactor
```

`volumeGain` should represent the recommended playback gain after distance, situation importance, and annoyance control.

Candidate formula:

```text
volumeGain =
  baseCueGain
  * distanceCompensation
  * situationPriority
  * comfortLimiter
```

These targets should be computed from experiment results first, then used as regression labels for the model.

## Step 6: Train A Neural Model

Input:

- peripheral state flags
- distance and view angle
- user-local position
- radial and lateral speed
- condition label
- optional environment profile

Output:

- `cueType`
- `presenceScore`
- `volumeGain`

Later outputs:

- `lowPassHz`
- `reverbAmount`
- `occlusionGain`
- `directionLabel`

Use a simple baseline first:

- rule-based model
- logistic regression or random forest
- small MLP

Only move to larger audio or multimodal networks after the labeled dataset is stable.

## Step 7: Evaluate Unknown-Situation Performance

Model evaluation:

- `cueType` accuracy
- macro F1 for cue classes
- `presenceScore` MAE
- `volumeGain` MAE
- performance on held-out situation types
- performance on held-out participants

VR experience evaluation:

- learned cue vs `NoCue`
- learned cue vs `FixedCue`
- learned cue vs `StateBasedCue`
- reaction time improvement
- direction accuracy improvement
- naturalness and annoyance tradeoff

## GitHub References To Reuse

- SoundSpaces: audio-visual simulation and source/listener sampling  
  https://github.com/facebookresearch/sound-spaces

- Few-Shot Audio-Visual Learning of Environment Acoustics: RIR prediction and sparse environment observation structure  
  https://github.com/SAGNIKMJR/few-shot-rir

- Learning Neural Acoustic Fields: source-listener acoustic field modeling reference  
  https://github.com/aluo-x/Learning_Neural_Acoustic_Fields

- Audio Spectrogram Transformer: audio encoder baseline if raw cue audio features are used later  
  https://github.com/YuanGongND/ast

- Microsoft spatialaudio-unity: Unity spatializer implementation reference  
  https://github.com/microsoft/spatialaudio-unity

- Steam Audio Unity Integration: occlusion, reflections, and HRTF-based spatial audio reference  
  https://valvesoftware.github.io/steam-audio/doc/unity/guide.html

## Immediate Unity Tasks

1. Add cue-candidate trial support.
2. Add `cueCandidate`, response, reaction time, and rating fields to CSV.
3. Implement real 3D cue playback for each candidate.
4. Add a trial sequencer for condition/cue combinations.
5. Build an analysis script that computes `cueEffectiveness`.
6. Export a labeled dataset for model training.

Implemented local commands:

```powershell
python Tools/analyze_peripheral_csv.py --cue-effectiveness
python Tools/analyze_peripheral_csv.py --label-dataset
```
