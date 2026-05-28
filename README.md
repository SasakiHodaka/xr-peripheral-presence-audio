# XR Peripheral Presence Audio Research

Unity prototype and research notes for adaptive peripheral presence audio in VR.

The project starts from a practical Unity system that detects off-screen or peripheral people and maps those states to interpretable audio cues. The longer-term research direction is to connect those cues to environment acoustics estimated from audio-visual simulation and learning.

## Current Prototype

The current Unity prototype detects peripheral target states and predicts lightweight cue parameters.

Implemented components:

- `PeripheralStateDetector`: detects target state flags such as `OutOfView`, `Approaching`, `Speaking`, `Gazing`, `Near`, and `Crossing`.
- `PeripheralCueModel`: predicts `cueType`, `presenceScore`, and `volumeGain` from each detection result.
- `PeripheralStateLogger`: writes target state and cue predictions to CSV.
- `PeripheralDebugUI`: shows target state and cue predictions in Play Mode.
- `PeripheralTrialController`: controls pre-trial and trial timing.
- `PeripheralTrialConditionController`: switches demo conditions such as approach, back approach, crossing, speaking, and none.

Current cue flow:

```text
target state + distance + speed
-> cueType, presenceScore, volumeGain
-> CSV + debug UI
```

Next Unity target:

```text
target state + distance + speed + environmentAcousticProfile
-> cueType, presenceScore, volumeGain, reverbAmount, occlusionGain
-> 3D audio cue playback
```

## Research Structure

The method is organized into three layers.

### 1. Environment Acoustics Estimation

Goal: infer an `environmentAcousticProfile` that captures room-scale acoustic properties.

Main references:

- Few-Shot Audio-Visual Learning of Environment Acoustics
- Learning Neural Acoustic Fields, as a reference concept for source-receiver acoustic prediction

Useful ideas:

- Use RGB-D, echo, pose, and source/listener position.
- Predict RIR, RT60, DRR, occlusion, and compact acoustic profile values.
- Treat NAF-style acoustic fields as a later-stage concept, not the first implementation target.

### 2. Audio-Visual Learning

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

### 3. Unity Implementation Connection

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

1. Stabilize the current state detection and cue prediction layer.
2. Add `PeripheralCueAudioEmitter` to play predicted cue types as 3D audio.
3. Extend logs with actual playback parameters.
4. Add a manual `EnvironmentAcousticProfile` for early environment-adaptive cue tests.
5. Add comparison conditions: `NoCue`, `FixedCue`, `StateBasedCue`, and `EnvironmentAdaptiveCue`.
6. Build the simulation data schema for dry/wet audio, source/listener pose, RGB-D, RIR, and room metadata.
7. Pre-train audio-visual-position encoders with self-supervised tasks.
8. Fine-tune an environment estimator that outputs `environmentAcousticProfile`.
9. Connect the estimator output back into Unity cue control.
10. Evaluate both model accuracy and VR experience.

## Documentation

- `PERIPHERAL_RESEARCH.md`: Unity demo flow, CSV format, trial conditions, and analysis script usage.
- `NAF_RESEARCH_PLAN.md`: NAF-inspired interpretation and boundaries for this project.
- `RESEARCH_ROADMAP.md`: consolidated research plan and mapping to related work.
- `ENVIRONMENT.md`: local Unity and tooling setup.

## Compile Check

Use Unity batch mode as the authoritative compile check:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\acd-pc67\My project" -logFile "C:\Users\acd-pc67\My project\Logs\PeripheralCompileCheck.log"
```

