# AI Training Schema

## Selected Method

The first AI target follows this practical pipeline:

```text
Unity / SoundSpaces-style samples
-> few-shot-style environment acoustics estimator
-> compact EnvironmentAcousticProfile
-> PeripheralCueModel / PeripheralCueAudioEmitter
```

NAF remains a background reference. The first trainable model should predict compact cue-control parameters, not full impulse responses.

## First Dataset

The first dataset is built from Unity CSV logs with:

```powershell
python Tools/build_cue_training_dataset.py
```

Default input directory:

```text
C:\Users\acd-pc67\AppData\LocalLow\DefaultCompany\My project
```

Default output:

```text
cue_training_dataset.csv
```

## Feature Columns

- `conditionLabel`
- `cueCondition`
- `roomScale`
- `materialClass`
- `environmentReverbAmount`
- `environmentOcclusionStrength`
- `environmentDistanceAttenuation`
- `environmentRt60`
- `environmentDrr`
- `targetId`
- `outOfView`
- `approaching`
- `speaking`
- `gazing`
- `near`
- `crossing`
- `distance`
- `viewAngle`
- `radialSpeed`
- `lateralSpeed`
- `localX`
- `localY`
- `localZ`

Later SoundSpaces-style samples should add:

- `roomScale`
- `materialClass`
- `rt60`
- `drr`
- `sourcePosition`
- `listenerPosition`
- `listenerRotation`
- `rgbImagePath`
- `depthImagePath`
- `rirPath`

## Target Columns

The first model should predict:

- `cueType`
- `presenceScore`
- `volumeGain`
- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

These are compact, Unity-facing targets. They can later be replaced or conditioned by a model trained on SoundSpaces 2.0 or few-shot environment acoustics data.

## First Model Baselines

Start with small supervised baselines:

- classifier for `cueType`
- regressor for `presenceScore`
- regressor for `volumeGain`
- regressor for `cueLowPassHz`
- regressor for `cueReverbAmount`
- regressor for `cueOcclusionGain`

Recommended order:

1. RandomForest or Gradient Boosting for fast tabular baselines.
2. Small MLP after the schema stabilizes.
3. Audio-visual model only after simulated acoustic samples exist.

## Current Training Script

The current dependency-free baseline is:

```powershell
python Tools/train_cue_model.py --epochs 40
```

It trains a lightweight cue-control model from `cue_training_dataset.csv` and writes:

- `Models/cue_model.json`
- `cue_training_predictions.csv`

Use `AUI_TRAINING_REPORT.md` for the latest training metrics and interpretation.
