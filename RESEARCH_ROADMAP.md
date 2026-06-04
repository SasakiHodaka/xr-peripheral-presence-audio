# Research Roadmap

## Fixed Direction

This project should use human-labeled peripheral cue learning as the primary research pipeline:

```text
Unity situation generation
-> multiple peripheral cue candidates
-> human-subject evaluation
-> best cue label per situation
-> mathematical PresenceScore / volumeGain targets
-> neural model training
-> unknown-situation evaluation
```

The immediate implementation target is not a full neural acoustic field or a physically complete acoustic simulator. The first target is an explainable Unity experiment system that can present multiple peripheral presence cues, measure human recognition performance, convert the best cue into labels, and train a model to predict cue type and cue strength for unknown situations.

Environment acoustics estimation remains a later extension:

```text
human-labeled cue learning
-> environment-adaptive cue parameters
-> audio-visual / acoustic simulation support
```

The research should be presented as a hybrid human-machine approach:

```text
human perception experiment
-> cue labels and perceptual targets
-> machine learning for prediction and generalization
-> simulation-based environment estimation for acoustic adaptation
```

Human participants provide the perceptual ground truth. The model provides scalable prediction for situations that were not directly tested.

## Primary Human-Labeled Cue Learning Pipeline

The core experiment flow is:

1. Generate peripheral-person situation patterns in Unity.
2. Present multiple cue candidates for each situation.
3. Measure recognition performance in a human-subject experiment.
4. Select the most effective cue as the target label.
5. Define `PresenceScore` and `volumeGain` with a mathematical model.
6. Train a neural model using the labeled dataset.
7. Evaluate prediction performance on unknown situations.

Recommended initial cue candidates:

- `NoCue`
- `Footstep`
- `Breathing`
- `ClothRustle`
- `Voice`
- `AmbientPresence`
- `MixedCue`

Recommended objective metrics:

- detection success
- reaction time
- direction judgment accuracy
- missed detections
- false responses

Recommended subjective metrics:

- awareness
- naturalness
- annoyance
- discomfort
- immersion

Label generation should use a combined effectiveness score instead of reaction time alone:

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

The concrete study design is defined in `RESEARCH_DESIGN.md`. Implementation work should support that document's research question, cue conditions, target scenarios, and dependent measures before expanding the AI scope.

The follow-on project is defined in `SECOND_PROJECT_RESEARCH_DESIGN.md`. It should validate multimodal situation inference and adaptive feedback before expanding into full multimodal CPS.

## Layer 1: Environment Acoustics Estimation

Goal:

```text
RGB-D + echo + pose + source/listener position
-> RIR / RT60 / DRR / occlusion / material / room scale
-> environmentAcousticProfile
```

Most relevant reference:

- Few-Shot Audio-Visual Learning of Environment Acoustics

Why it matters:

- It directly matches the idea of estimating environment acoustics from limited observations.
- It uses RGB-D, echo, and pose information.
- It predicts RIR for arbitrary source-receiver pairs.
- It supports the few-shot unknown-environment framing.

Use in this project:

- Treat `environmentAcousticProfile` as the Unity-facing output.
- Keep the profile compact and controllable first:
  - `roomScale`
  - `materialClass`
  - `reverbAmount`
  - `occlusionStrength`
  - `distanceAttenuation`
  - `rt60`
  - `drr`

NAF-related work is useful as a reference concept for source/listener acoustic fields, but it is not the selected first AI method. The first AI method should follow Few-Shot Audio-Visual Learning of Environment Acoustics, with SoundSpaces 2.0 used to generate the required audio-visual training data.

## Layer 2: Audio-Visual Learning

Goal:

```text
3D scenes + random source/listener pairs
-> dry audio + wet audio + RGB-D + pose + RIR + metadata
-> self-supervised pre-training
```

Most relevant references:

- SoundSpaces
- SoundSpaces 2.0
- SoundSpaces: Audio-Visual Navigation in 3D Environments

Use in this project:

- Generate many unlabeled samples.
- Randomize source and listener locations.
- Save dry audio and rendered wet audio.
- Save RGB-D if available.
- Save room size, material, distance, occlusion, and source/listener pose.

Candidate self-supervised tasks:

- Predict wet audio from dry audio and source/listener pose.
- Predict RIR from RGB-D, pose, and source/listener position.
- Predict RT60 or DRR from audio-visual observations.
- Align visual embeddings and acoustic embeddings from the same environment.
- Learn common environment embeddings from different source/listener observations.

Candidate model structure:

- Audio encoder: Audio Spectrogram Transformer or CNN/ResNet.
- Visual encoder: CNN, ViT, or RGB-D encoder.
- Position encoder: sinusoidal positional encoding.
- Fusion: Transformer encoder.
- Outputs: RIR, RT60, DRR, wet spectrogram, or acoustic embedding.

## Layer 3: Unity Implementation Connection

Current Unity cue flow:

```text
state + distance + speed
-> cueType, presenceScore, volumeGain
```

Target Unity cue flow:

```text
state + distance + speed + environmentAcousticProfile
-> cueType, presenceScore, volumeGain, reverbAmount, occlusionGain
```

Current implemented components:

- `PeripheralStateDetector`
- `PeripheralCueModel`
- `PeripheralStateLogger`
- `PeripheralDebugUI`
- `PeripheralTrialController`
- `PeripheralTrialConditionController`

Next components:

- `PeripheralCueAudioEmitter`
- `EnvironmentAcousticProfile`
- playback-aware logger fields
- cue comparison condition controller

## Related Work Mapping

| Project element | Main reference |
| --- | --- |
| Large-scale simulation data generation | SoundSpaces / SoundSpaces 2.0 |
| RGB-D, echo, pose based acoustic estimation | Few-Shot Audio-Visual Learning of Environment Acoustics |
| Source-receiver RIR prediction | Few-Shot RIR / NAF |
| Audio-visual spatial understanding | SoundSpaces audio-visual navigation |
| Peripheral awareness through spatial audio | VRBubble |
| Functional sound for human-robot awareness | Consequential and Functional Sound in HRI |
| Spatial computing / HRC framing | Spatial Computing and Intuitive Interaction |
| Speaking-condition presence enhancement | Audio2Face |
| Unity peripheral cue control | Current `PeripheralCueModel` and future `EnvironmentAcousticProfile` |

## Audio2Face Position

Audio2Face is not the main environment acoustics method.

It should be treated as an optional later extension for the `Speaking` condition:

- voice and lip-sync alignment
- facial expression synchronization
- stronger speaking-person presence cues
- multimodal cue presentation

## References

### Core References

- Sagnik Majumder, Changan Chen, Ziad Al-Halah, Kristen Grauman. **Few-Shot Audio-Visual Learning of Environment Acoustics**. NeurIPS 2022.  
  https://proceedings.neurips.cc/paper_files/paper/2022/hash/113ae3a9762ca2168f860a8501d6ae25-Abstract-Conference.html

- Changan Chen, Carl Schissler, Sanchit Garg, Philip Kobernik, Alexander Clegg, Paul Calamia, Dhruv Batra, Philip W. Robinson, Kristen Grauman. **SoundSpaces 2.0: A Simulation Platform for Visual-Acoustic Learning**. NeurIPS 2022 Datasets and Benchmarks.  
  https://arxiv.org/abs/2206.08312

- Changan Chen, Unnat Jain, Carl Schissler, Sebastia Vicenc Amengual Gari, Ziad Al-Halah, Vamsi Krishna Ithapu, Philip Robinson, Kristen Grauman. **SoundSpaces: Audio-Visual Navigation in 3D Environments**. ECCV 2020 / arXiv.  
  https://arxiv.org/abs/1912.11474

### Conceptual References

- Andrew Luo, Yilun Du, Michael J. Tarr, Joshua B. Tenenbaum, Antonio Torralba, Vincent Sitzmann. **Learning Neural Acoustic Fields**. NeurIPS 2022.  
  https://arxiv.org/abs/2204.00628

- NVIDIA. **Omniverse Audio2Face Documentation**.  
  https://docs.omniverse.nvidia.com/audio2face/latest/overview_external.html

## Development Steps

### Step 0: Selected AI method

Use:

```text
SoundSpaces 2.0 dataset generation
-> Few-Shot-style environment acoustics estimator
-> compact EnvironmentAcousticProfile
-> Unity cue control
```

Do not start with NAF. Do not train a full neural acoustic field until the compact estimator has been validated.

### Step 1: Add cue-candidate experiment trials

Implement:

- cue-candidate trial selection
- `cueCandidate` metadata
- response and reaction-time logging
- subjective rating logging

Target flow:

```text
BackApproach situation
-> Footstep / Breathing / ClothRustle / Voice / AmbientPresence candidates
-> participant response
-> CSV log
```

### Step 2: Finish the Unity cue playback layer

Implement:

- `PeripheralCueAudioEmitter`
- cue-type audio playback
- playback logging
- debug display of playback state

Target flow:

```text
person approaches from behind
-> OutOfView + Approaching
-> Footstep cue
-> 3D audio playback
-> CSV log
```

### Step 3: Generate human labels

Implement:

- per-condition aggregation
- cue effectiveness scoring
- best-cue label selection
- labeled dataset export

Targets:

- `cueType`
- `presenceScore`
- `volumeGain`

### Step 4: Train the cue prediction model

Start with:

- rule-based baseline
- logistic regression or random forest baseline
- small MLP

Evaluate:

- cue type accuracy
- macro F1
- `PresenceScore` MAE
- `volumeGain` MAE
- held-out situation performance
- held-out participant performance

### Step 5: Add manual environment adaptation

Implement:

- `EnvironmentAcousticProfile`
- manual inspector-controlled profile values
- cue model input extension
- reverb and occlusion parameters

This allows testing the environment-adaptive structure before training a model.

### Step 6: Add experimental comparison conditions

Conditions:

- `NoCue`
- `FixedCue`
- `StateBasedCue`
- `LearnedCue`
- `EnvironmentAdaptiveCue`

Purpose:

- compare whether adaptive cues improve awareness, localization, naturalness, and immersion.

Current Unity status:

- `PeripheralCueModel.comparisonCondition` switches these cue-control modes.
- `cueCondition` is written to CSV and included in the first cue-training dataset.

### Step 7: Define simulation dataset schema

Save each generated sample with:

- `sample_id`
- `dry_audio_path`
- `wet_audio_path`
- `source_position`
- `listener_position`
- `listener_rotation`
- `rgb_image_path`
- `depth_image_path`
- `room_size`
- `material_type`
- `distance`
- `occlusion`
- `rir_path`
- `rt60`
- `drr`

### Step 8: Train self-supervised encoders

Start with small data and simple tasks:

- wet spectrogram prediction
- RT60 / DRR regression
- environment embedding alignment

Then scale toward RIR prediction.

### Step 9: Fine-tune environment estimator

Labels:

- wide / narrow
- strong / weak reverberation
- wood / concrete / glass / carpet
- open / closed
- occluded / not occluded

Output:

```text
environmentAcousticProfile
```

### Step 10: Integrate model output into Unity

Bridge:

```text
trained estimator
-> environmentAcousticProfile
-> PeripheralCueModel
-> PeripheralCueAudioEmitter
```

### Step 11: Evaluate

Model evaluation:

- RIR error
- RT60 error
- DRR error
- environment classification accuracy

VR experience evaluation:

- awareness of peripheral people
- reaction time
- localization accuracy
- perceived naturalness
- discomfort
- immersion
