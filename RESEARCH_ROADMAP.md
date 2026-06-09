# Research Roadmap

## Fixed Direction

This project should be developed as a three-layer system:

```text
situation simulation
-> simulation-based cue-label generation
-> optional human feedback calibration
-> Unity peripheral cue-control learning
```

The immediate implementation target is not a full neural acoustic field and not only an environment acoustics estimator. The first target is a defensible cue-label pipeline: generate virtual human-presence situations, derive cue labels from objective simulation parameters and evaluation metrics, and use the result as training data.

Do not use the researcher's subjective cue decisions as final ground truth. Existing hand-written cue rules are only a prototype baseline for checking the Unity pipeline.

The concrete study design is defined in `RESEARCH_DESIGN.md`. Implementation work should support that document's research question, cue conditions, target scenarios, and dependent measures before expanding the AI scope.

The follow-on project is defined in `SECOND_PROJECT_RESEARCH_DESIGN.md`. It should validate multimodal situation inference and adaptive feedback before expanding into full multimodal CPS.

## Layer 1: Situation Simulation

Goal:

```text
distance + direction + approach speed + speaking + crossing + view state
-> many virtual human-presence situations
-> candidate cue playback and logging
```

Why it matters:

- It replaces ad hoc manual examples with a controlled condition space.
- It makes the input parameters explicit.
- It allows systematic coverage of behind-user approach, crossing, speaking, and no-target cases.
- It separates situation generation from the later question of which cue should be treated as correct.

Initial condition factors:

- distance: near, middle, far
- direction: front, rear, left, right
- approach speed: none, slow, fast
- speaking: false, true
- crossing: false, true
- view state: in view, peripheral, out of view

## Layer 2: Cue Evaluation and Label Generation

Goal:

```text
generated situation
-> objective simulation score
-> optional human feedback score
-> cueType, presenceScore, volumeGain labels
```

Why it matters:

- The correct cue should not be derived from the developer's preference.
- The first training labels should be generated from reproducible simulation parameters such as distance, direction, relative speed, view state, speaking state, and crossing state.
- Human feedback should be used later for calibration and validation, not as the only source of labels.

Use in this project:

- Generate many situations automatically in Unity.
- Compute baseline labels from objective situation metrics.
- Train a cue-control model from the generated dataset.
- Evaluate the learned model against held-out simulation scenarios.
- Add human feedback after the simulation pipeline works to calibrate clarity, naturalness, discomfort, and annoyance.

Initial objective label model:

```text
presenceScore =
  a * distanceUrgency
  + b * outOfViewNeed
  + c * approachUrgency
  + d * speakingImportance
  + e * crossingNeed
  + f * gazeRelevance
```

The weights should first be defined as a transparent baseline from objective situation factors and later calibrated from user feedback.

Human feedback calibration model:

```text
calibratedScore =
  objectiveScore
  + g * clarityRating
  + h * naturalnessRating
  - i * discomfortRating
  - j * annoyanceRating
```

This preserves a non-subjective simulation-first baseline while allowing the system to become closer to human perception.

## Layer 3: Cue-Control Learning

Goal:

```text
situation parameters
-> cueType + presenceScore + volumeGain
-> Unity playback
```

Initial models:

- RandomForest or Gradient Boosting for explainable tabular baselines.
- Small MLP after the dataset and labels stabilize.
- Multi-output model with cue classification and score regression.

Evaluation:

- cueType accuracy and F1-score against evaluated labels
- presenceScore MAE/RMSE
- volumeGain MAE/RMSE
- real-time Unity playback behavior
- final user-study measures: detection time, localization accuracy, naturalness, immersion, discomfort

## Simulation-Based Learning References

Most relevant references for the data-generation idea:

- Meta Audio Simulator
- SoundSpaces
- SoundSpaces 2.0
- self-supervised learning

Use in this project:

- These works support the idea of generating large datasets from virtual scenes.
- In SoundSpaces-style work, the label can be an RIR because acoustic propagation is physically simulated.
- In this project, the label is a cue choice for human awareness, so evaluation is required.

Environment acoustics remains a later extension:

```text
RGB-D + echo + pose + source/listener position
-> RIR / RT60 / DRR / occlusion / material / room scale
-> environmentAcousticProfile
-> cue playback conditioning
```

Neural Acoustic Fields are out of scope for the current implementation plan.

## Unity Implementation Connection

Current Unity cue flow:

```text
state + distance + speed
-> cueType, presenceScore, volumeGain
```

Target Unity cue flow:

```text
state + distance + speed + evaluated cue-label model
-> cueType, presenceScore, volumeGain
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
- simulation condition generator
- participant response logger
- evaluation-to-label dataset builder

## Related Work Mapping

| Project element | Main reference |
| --- | --- |
| Large-scale simulation data generation | Meta Audio Simulator / SoundSpaces / SoundSpaces 2.0 |
| Human-aware cue label generation | This project's evaluation pipeline |
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

- NVIDIA. **Omniverse Audio2Face Documentation**.  
  https://docs.omniverse.nvidia.com/audio2face/latest/overview_external.html

## Development Steps

### Step 0: Selected AI method

Use:

```text
Unity situation generation
-> cue candidate evaluation
-> evaluated cue-label dataset
-> cue-control model
-> Unity cue control
```

Do not start with NAF. Do not treat the current subjective cue labels as final ground truth. Use them only as an initial prototype baseline until evaluation-based labels exist.

### Step 0.5: Lock the simulation-first learning pipeline

Target pipeline:

```text
Unity automatic scenario generator
-> objective label generation
-> CSV dataset
-> Python model training
-> Unity learned cue playback
-> human feedback calibration and evaluation
```

The first implementation should prioritize reproducible generated datasets over manual demonstrations.

### Step 1: Finish the Unity cue layer

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

### Step 2: Add manual environment adaptation

Implement:

- `EnvironmentAcousticProfile`
- manual inspector-controlled profile values
- cue model input extension
- reverb and occlusion parameters

This allows testing the environment-adaptive structure before training a model.

### Step 3: Add experimental comparison conditions

Conditions:

- `NoCue`
- `FixedCue`
- `StateBasedCue`
- `EnvironmentAdaptiveCue`

Purpose:

- compare whether adaptive cues improve awareness, localization, naturalness, and immersion.

Current Unity status:

- `PeripheralCueModel.comparisonCondition` switches these cue-control modes.
- `cueCondition` is written to CSV and included in the first cue-training dataset.

### Step 4: Define cue-label simulation dataset schema

Save each generated sample with:

- `sample_id`
- `distance`
- `direction`
- `view_state`
- `approach_speed`
- `speaking`
- `crossing`
- `candidate_cue`
- `candidate_volume`
- `localization_accuracy`
- `reaction_time`
- `approach_recognition`
- `clarity_rating`
- `naturalness_rating`
- `discomfort_rating`
- `selected_cue_label`
- `presence_score`
- `volume_gain`

### Step 5: Run cue evaluation and create labels

For each generated situation:

- play multiple cue candidates
- measure objective and subjective responses
- select the best-performing cue
- compute `presenceScore` and `volumeGain`

### Step 6: Train the cue-control model

Inputs:

- distance
- direction
- view state
- approach speed
- speaking
- crossing

Outputs:

- `cueType`
- `presenceScore`
- `volumeGain`

Start with tabular baselines, then move to a small MLP or NN once the dataset is reliable.

### Step 7: Integrate learned cue model into Unity

Bridge:

```text
trained cue-control model
-> cueType / presenceScore / volumeGain
-> PeripheralCueModel
-> PeripheralCueAudioEmitter
```

### Step 8: Evaluate

Model evaluation:

- cueType accuracy / F1-score
- presenceScore error
- volumeGain error
- generalization to unseen generated situations

VR experience evaluation:

- awareness of peripheral people
- reaction time
- localization accuracy
- perceived naturalness
- discomfort
- immersion
