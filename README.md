# XR Peripheral Presence Audio Research

Unity prototype and research notes for adaptive peripheral presence audio in VR/XR.

The project starts from a practical Unity system that detects off-screen or peripheral people and maps those states to interpretable audio cues. The longer-term research direction is to replace subjective cue labels with labels built from simulation and evaluation: generate many virtual human-presence situations, test candidate cues, convert the best-performing cue into a training label, and train a cue-control model from that dataset.

## Current Research Position

The current implementation proves that the Unity logging and cue-control learning pipeline can run. It does not yet prove that the generated cue labels are generally correct for users.

Current status:

- Unity can detect peripheral human-presence states and log cue parameters.
- A first training dataset can be generated from objective simulation parameters on the PC.
- Unity logs can still be used for prototype and human-feedback datasets.
- A lightweight cue-control model can predict `cueType`, `presenceScore`, and playback parameters.
- The current simulation labels are reproducible baseline labels. Human feedback is used later for calibration and validation.
- The next research step is to build evaluation-derived labels from generated situations and candidate cue performance.
- Evaluation logs can also be converted with `Tools/analyze_peripheral_csv.py --label-dataset --objective-only` so the first label pass depends only on detection, direction, and reaction time.
- All source logs in a directory can be combined with `Tools/analyze_peripheral_csv.py --batch-label-dataset --objective-only`.
- The full evaluation-to-training pipeline can be run with `Tools/train_from_evaluation_logs.py`.
- If the current logs do not contain cue-candidate records yet, that script falls back to the existing simulation dataset so training still completes.

Important boundary:

```text
Current dataset:
developer/rule-based prototype labels

Target dataset:
simulation-generated situations
-> multiple cue candidates
-> evaluation by localization, reaction time, clarity, naturalness, and discomfort
-> final cue labels
```

## Current Prototype

The current Unity prototype detects peripheral target states and predicts lightweight cue parameters.

Implemented components:

- `PeripheralStateDetector`: detects target state flags such as `OutOfView`, `Approaching`, `Speaking`, `Gazing`, `Near`, and `Crossing`.
- `EnvironmentAcousticProfile`: stores compact room/acoustic parameters that can later be predicted by an AI estimator.
- `PeripheralCueModel`: predicts `cueType`, `presenceScore`, and `volumeGain` from each detection result.
- `PeripheralCueAudioEmitter`: plays predicted cue types as target-attached 3D audio.
- `PeripheralStateLogger`: writes target state and cue predictions to CSV.
- `PeripheralDebugUI`: shows target state and cue predictions in Play Mode.
- `PeripheralTrialController`: controls pre-trial and trial timing.
- `PeripheralTrialConditionController`: switches demo conditions such as approach, back approach, crossing, speaking, and none.
- `PeripheralAuiLogCollectionController`: cycles target scenarios, cue conditions, and environment presets for AUI training logs.
- `PeripheralSimulationDatasetGenerator`: generates objective simulation-label CSV data from Unity.
- `PeripheralCueModel.comparisonCondition`: switches `NoCue`, `FixedCue`, `StateBasedCue`, and `EnvironmentAdaptiveCue` cue-control modes.

PC-only simulation and training:

```powershell
python Tools/generate_simulation_dataset.py --mode grid --output cue_training_dataset.csv
python Tools/train_cue_model.py --dataset cue_training_dataset.csv --classifier linear --classifier-epochs 220 --epochs 80
```

This writes `Assets/Models/cue_model_unity.json`, which can be assigned to `PeripheralCueModel.learnedModelJson` and used with `LearnedCue`.

Current cue flow:

```text
target state + distance + speed
-> cueType, presenceScore, volumeGain, lowPassHz, reverbAmount, occlusionGain
-> 3D audio playback + CSV + debug UI
```

Next Unity target:

```text
target state + distance + speed + evaluated cue-label model
-> cueType, presenceScore, volumeGain
-> 3D audio playback + CSV + participant/evaluation logs
```

## Research Structure

The method is organized into three layers.

### 1. Situation Simulation and Cue-Label Generation

Goal: generate many virtual situations and convert cue evaluation results into training labels.

Core flow:

```text
distance + direction + approach speed + speaking + crossing + view state
-> candidate cues such as footstep, voice, ambient presence, clothing rustle, breathing, or none
-> evaluation by localization accuracy, reaction time, clarity, naturalness, and discomfort
-> cueType, presenceScore, and volumeGain labels
```

This is the main answer to the data-reliability problem. The current dataset is useful as an initial subjective prototype, but it should not be treated as the final ground truth. Final cue labels should come from simulation plus evaluation, not only from the developer's preference.

### 2. Audio-Visual Simulation and Acoustic References

Goal: use simulation-based audio research as the data-generation model, while keeping the target label problem clear.

Main references:

- Meta Audio Simulator
- SoundSpaces
- SoundSpaces 2.0
- Learning Neural Acoustic Fields
- self-supervised learning

Useful ideas:

- Generate many samples from virtual environments instead of manually labeling every sample.
- Randomly place sources, listeners, and moving targets in 3D spaces.
- Save state parameters, audio candidates, source/listener positions, and environment metadata.
- Use self-supervised or simulation-supervised learning where the label is physically computable.

Important boundary:

```text
SoundSpaces-style work can use simulated RIR as the ground truth because acoustics are physically computable.
This project cannot get the correct cueType from physics alone because cue usefulness depends on human perception.
Therefore, cue labels require human evaluation or an explicit evaluation model.
```

### 3. Unity Implementation Connection

Goal: use the learned or rule-based cue-control output in a real-time XR prototype.

Unity-side flow:

```text
PeripheralStateDetector
-> PeripheralCueModel or learned cue-control model
-> PeripheralCueAudioEmitter
-> CSV/debug/evaluation
```

Environment acoustics estimation remains a later extension. A compact `EnvironmentAcousticProfile` can condition cue playback after the cue-label dataset and evaluation pipeline are stable.

Audio2Face is treated as an optional extension for the `Speaking` condition, where voice, lip motion, facial expression, and presence cues may be synchronized. It is not the main cue-label generation method.

## Planned Data and Learning Pipeline

```text
1. Generate many Unity situations:
   distance, direction, approach speed, speaking, crossing, and view state.
2. Prepare several cue candidates for each situation:
   footstep, voice, ambient presence, clothing rustle, breathing, and none.
3. Evaluate cue candidates:
   localization accuracy, detection/reaction time, approach recognition, clarity, naturalness, and discomfort.
4. Build final labels:
   best cueType, presenceScore, and volumeGain.
5. Train a neural or tabular cue-control model:
   state parameters -> cueType + presenceScore + volumeGain.
6. Test on unseen situations and in Unity play-mode trials.
```

## Development Roadmap

1. Stabilize the current state detection and cue prediction layer.
2. Stabilize `PeripheralCueAudioEmitter` clip mapping and playback timing in trial scenes.
3. Define the simulation condition grid for distance, direction, approach speed, speaking, crossing, and view state.
4. Add candidate cue playback for footstep, voice, ambient presence, clothing rustle, breathing, and none.
5. Collect the first subjective prototype dataset and clearly mark it as non-final.
6. Run evaluation trials to measure localization accuracy, reaction time, clarity, naturalness, and discomfort.
7. Convert evaluation results into cue labels and presence/volume scores.
8. Train a cue-control model from the evaluated dataset.
9. Compare rule-based, subjective-label, and evaluation-label models.
10. Add environment-acoustic conditioning after the cue-label pipeline is reliable.

## Documentation

Primary research documents:

- `RESEARCH_DESIGN.md`: research question, hypotheses, experiment design, measures, and the cue-label training plan.
- `RESEARCH_ROADMAP.md`: implementation roadmap from situation simulation to evaluation-derived labels and cue-control learning.
- `AI_TRAINING_SCHEMA.md`: feature columns, target columns, current prototype dataset, label reliability issue, and next dataset target.
- `CURRENT_PROGRESS_REPORT.md`: current Japanese summary of the project status, limitation, and next plan.

Implementation documents:

- `PERIPHERAL_RESEARCH.md`: Unity demo flow, CSV format, trial conditions, and analysis script usage.
- `ENVIRONMENT.md`: local Unity and tooling setup.

Related work and extension documents:

- `LITERATURE_PRIORITIES.md`: prioritized related work across acoustics, SoundSpaces, XR, HRI, CPS, and multimodal learning.
- `NAF_RESEARCH_PLAN.md`: NAF-inspired interpretation and boundaries for this project.
- `SECOND_PROJECT_RESEARCH_DESIGN.md`: follow-on project design that bridges XR adaptive feedback to multimodal CPS.

Progress/archive documents:

- `AUI_TRAINING_REPORT.md`: latest cue-control learning pipeline result and next training steps.
- `PROGRESS_REPORT_AUI.md`: concise progress-report text for the current AUI learning implementation.
- `AUI_PROGRESS_PRESENTATION_DRAFT.md`: slide-by-slide draft and Q&A for progress reporting.

## References

- Majumder, S., Chen, C., Al-Halah, Z., & Grauman, K. **Few-Shot Audio-Visual Learning of Environment Acoustics**. NeurIPS 2022. https://proceedings.neurips.cc/paper_files/paper/2022/hash/113ae3a9762ca2168f860a8501d6ae25-Abstract-Conference.html
- Chen, C., Jain, U., Schissler, C., Gari, S. V. A., Al-Halah, Z., Ithapu, V. K., Robinson, P., & Grauman, K. **SoundSpaces: Audio-Visual Navigation in 3D Environments**. ECCV 2020 / arXiv. https://arxiv.org/abs/1912.11474
- Chen, C., Schissler, C., Garg, S., Kobernik, P., Clegg, A., Calamia, P., Batra, D., Robinson, P. W., & Grauman, K. **SoundSpaces 2.0: A Simulation Platform for Visual-Acoustic Learning**. NeurIPS 2022 Datasets and Benchmarks. https://arxiv.org/abs/2206.08312
- Luo, A., Du, Y., Tarr, M. J., Tenenbaum, J. B., Torralba, A., & Sitzmann, V. **Learning Neural Acoustic Fields**. NeurIPS 2022. https://arxiv.org/abs/2204.00628
- NVIDIA. **Omniverse Audio2Face Documentation**. https://docs.omniverse.nvidia.com/audio2face/latest/overview_external.html

## Compile Check

Use Unity batch mode as the authoritative compile check:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\acd-pc67\xr-peripheral-presence-audio" -logFile "C:\Users\acd-pc67\xr-peripheral-presence-audio\Logs\PeripheralCompileCheck.log"
```
