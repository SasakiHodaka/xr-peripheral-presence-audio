# XR Peripheral Presence Audio Research

Unity prototype and research notes for adaptive peripheral presence audio in VR.

The project starts from a practical Unity system that detects off-screen or peripheral people and maps those states to interpretable audio cues. The longer-term research direction is to connect those cues to environment acoustics estimated from audio-visual simulation and learning.

The research is framed as a hybrid human-machine system: human-subject experiments define which cues are perceptually effective, while machine learning generalizes cue selection and cue strength to unknown situations.

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
- `PeripheralCueModel.comparisonCondition`: switches `NoCue`, `FixedCue`, `StateBasedCue`, and `EnvironmentAdaptiveCue` cue-control modes.

Current cue flow:

```text
target state + distance + speed
-> cueType, presenceScore, volumeGain, lowPassHz, reverbAmount, occlusionGain
-> 3D audio playback + CSV + debug UI
```

Next Unity target:

```text
target state + distance + speed + environmentAcousticProfile
-> cueType, presenceScore, volumeGain, reverbAmount, occlusionGain
-> 3D audio cue playback
```

## Research Structure

The primary method is organized around human-labeled peripheral cue learning.

### 1. Human-Labeled Cue Learning

Goal: learn which sound cue best improves recognition of peripheral or off-screen people in VR.

Pipeline:

```text
Unity situation generation
-> multiple cue candidates
-> human-subject evaluation
-> best cue labels
-> PresenceScore / volumeGain targets
-> neural cue prediction
-> unknown-situation evaluation
```

Useful ideas:

- Generate controlled conditions such as approach, rear approach, crossing, speaking, and no target.
- Present multiple cue candidates for each situation.
- Measure reaction time, detection success, direction accuracy, naturalness, annoyance, and immersion.
- Use the best-performing cue as the supervised `cueType` label.
- Train a model to predict `cueType`, `presenceScore`, and `volumeGain`.

The longer-term method also keeps three extension layers.

### 2. Environment Acoustics Estimation

Goal: infer an `environmentAcousticProfile` that captures room-scale acoustic properties.

Main references:

- Few-Shot Audio-Visual Learning of Environment Acoustics
- Learning Neural Acoustic Fields, as a reference concept for source-receiver acoustic prediction

Useful ideas:

- Use RGB-D, echo, pose, and source/listener position.
- Predict RIR, RT60, DRR, occlusion, and compact acoustic profile values.
- Treat NAF-style acoustic fields as a later-stage concept, not the first implementation target.

### 3. Audio-Visual Learning

Goal: generate and learn from large unlabeled audio-visual datasets.

Main references:

- SoundSpaces
- SoundSpaces 2.0
- SoundSpaces: Audio-Visual Navigation in 3D Environments

Useful ideas:

- Randomly place sources and listeners in 3D spaces.
- Render dry and wet audio pairs.
- Store RGB-D, pose, source/listener positions, RIR, room size, material, distance, and occlusion.
- Pre-train audio, vision, and position encoders with self-supervised objectives.

### 4. Unity Implementation Connection

Goal: use the estimated acoustic profile to control peripheral audio cues in VR.

Unity-side flow:

```text
PeripheralStateDetector
-> PeripheralCueModel
-> PeripheralCueAudioEmitter
-> CSV/debug/evaluation
```

Audio2Face is treated as an optional extension for the `Speaking` condition, where voice, lip motion, facial expression, and presence cues may be synchronized. It is not the main environment acoustics method.

## Development Roadmap

1. Add cue-candidate trial support.
2. Add `PeripheralCueAudioEmitter` to play candidate cue types as 3D audio.
3. Extend logs with cue candidate, response, reaction time, rating, and playback parameters.
4. Run human-subject experiments to select best cue labels per situation.
5. Build `PresenceScore` and `volumeGain` targets from the experiment results.
6. Train a cue prediction model for `cueType`, `presenceScore`, and `volumeGain`.
7. Evaluate the model on unknown situations and held-out participants.
8. Add a manual `EnvironmentAcousticProfile` for early environment-adaptive cue tests.
9. Add comparison conditions: `NoCue`, `FixedCue`, `StateBasedCue`, `LearnedCue`, and `EnvironmentAdaptiveCue`.
10. Build the simulation data schema for dry/wet audio, source/listener pose, RGB-D, RIR, and room metadata.
11. Pre-train audio-visual-position encoders with self-supervised tasks.
12. Fine-tune an environment estimator that outputs `environmentAcousticProfile`.
13. Connect the estimator output back into Unity cue control.
14. Evaluate both model accuracy and VR experience.

## Documentation

- `PERIPHERAL_RESEARCH.md`: Unity demo flow, CSV format, trial conditions, and analysis script usage.
- `HUMAN_LABELING_PIPELINE.md`: main human-subject cue labeling and model-training pipeline.
- `RESEARCH_DESIGN.md`: research question, hypotheses, experimental conditions, metrics, and study plan.
- `SECOND_PROJECT_RESEARCH_DESIGN.md`: follow-on project design that bridges XR adaptive feedback to multimodal CPS.
- `AUI_TRAINING_REPORT.md`: latest cue-control learning pipeline result and next training steps.
- `PROGRESS_REPORT_AUI.md`: concise progress-report text for the current AUI learning implementation.
- `AUI_PROGRESS_PRESENTATION_DRAFT.md`: slide-by-slide draft and Q&A for the next progress report.
- `CURRENT_PROGRESS_REPORT.md`: integrated current progress report across research design, Unity implementation, and AUI learning.
- `NAF_RESEARCH_PLAN.md`: NAF-inspired interpretation and boundaries for this project.
- `RESEARCH_ROADMAP.md`: consolidated research plan and mapping to related work.
- `LITERATURE_PRIORITIES.md`: prioritized related work across acoustics, SoundSpaces, XR, HRI, CPS, and multimodal learning.
- `ENVIRONMENT.md`: local Unity and tooling setup.

## References

- Majumder, S., Chen, C., Al-Halah, Z., & Grauman, K. **Few-Shot Audio-Visual Learning of Environment Acoustics**. NeurIPS 2022. https://proceedings.neurips.cc/paper_files/paper/2022/hash/113ae3a9762ca2168f860a8501d6ae25-Abstract-Conference.html
- Chen, C., Jain, U., Schissler, C., Gari, S. V. A., Al-Halah, Z., Ithapu, V. K., Robinson, P., & Grauman, K. **SoundSpaces: Audio-Visual Navigation in 3D Environments**. ECCV 2020 / arXiv. https://arxiv.org/abs/1912.11474
- Chen, C., Schissler, C., Garg, S., Kobernik, P., Clegg, A., Calamia, P., Batra, D., Robinson, P. W., & Grauman, K. **SoundSpaces 2.0: A Simulation Platform for Visual-Acoustic Learning**. NeurIPS 2022 Datasets and Benchmarks. https://arxiv.org/abs/2206.08312
- Luo, A., Du, Y., Tarr, M. J., Tenenbaum, J. B., Torralba, A., & Sitzmann, V. **Learning Neural Acoustic Fields**. NeurIPS 2022. https://arxiv.org/abs/2204.00628
- NVIDIA. **Omniverse Audio2Face Documentation**. https://docs.omniverse.nvidia.com/audio2face/latest/overview_external.html

## Compile Check

Use Unity batch mode as the authoritative compile check:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\acd-pc67\My project" -logFile "C:\Users\acd-pc67\My project\Logs\PeripheralCompileCheck.log"
```
