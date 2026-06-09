# AUI Training Report

## Current Status

The first lightweight AUI cue-control learning pipeline is running end to end.

This report describes an initial baseline. The current labels come from existing cue rules and developer-selected cue behavior, so they should be used to verify the training pipeline, not as final evidence that the cues are correct for users.

Current simulation-first pipeline:

```text
PC simulation dataset generation
-> cue_training_dataset.csv
-> lightweight cue-control model training
-> Assets/Models/cue_model_unity.json
-> cue_training_predictions.csv
```

## Commands

Generate the simulation dataset:

```powershell
python Tools/generate_simulation_dataset.py --mode grid --output cue_training_dataset.csv
```

Train and evaluate the model:

```powershell
python Tools/train_cue_model.py --dataset cue_training_dataset.csv --classifier linear --classifier-epochs 220 --epochs 80
```

## Latest Training Run

Dataset:

- Source: objective PC simulation grid
- Rows: 800

Train/test split:

- Train rows: 600
- Test rows: 200

Training class counts:

- `Footstep`: 129
- `Voice`: 297
- `None`: 24
- `AmbientPresence`: 150

Evaluation:

- Classifier: dependency-free linear multi-class classifier
- Split: random row split
- Added categorical features: `directionLabel`, `viewState`, `motionState`
- Train `cueType` accuracy: 0.9683
- Test `cueType` accuracy: 0.9600
- Test per-class accuracy:
  - `AmbientPresence`: 1.0000
  - `Footstep`: 1.0000
  - `None`: 0.3333
  - `Voice`: 1.0000
- Test `presenceScore` MAE: 0.0620
- Test `volumeGain` MAE: 0.0489
- Test `cueLowPassHz` MAE: 1009.3945
- Test `cueReverbAmount` MAE: 0.0255
- Test `cueOcclusionGain` MAE: 0.0238

Additional randomized simulation check with unknown condition groups:

- Rows: 5,000
- Split: group split by `directionLabel,motionState`
- Train rows: 3,749
- Test rows: 1,251
- Test `cueType` accuracy: 0.8066
- Test per-class accuracy:
  - `AmbientPresence`: 0.4190
  - `Footstep`: 1.0000
  - `None`: 0.9273
  - `Voice`: 0.9885
- Test `presenceScore` MAE: 0.0827
- Test `volumeGain` MAE: 0.0788

Class weighting check:

- Balanced class weighting improved `None` detection on the grid split but reduced overall accuracy and `AmbientPresence` accuracy.
- The default Unity model currently uses unweighted linear classification.
- `--class-weight balanced` remains available for analysis when false silence or missed no-cue cases become the main error.

Outputs:

- `Assets/Models/cue_model_unity.json`
- `cue_training_dataset.csv`
- `cue_training_predictions.csv`

Dataset distribution can be checked with:

```powershell
python Tools/summarize_cue_training_dataset.py
```

## Interpretation

This is a simulation-first baseline, not the final AUI model.

The current dataset is generated from objective situation parameters rather than the researcher's subjective cue choices. The labels are still baseline labels because the score weights are defined by an explicit objective model. Human feedback should later be used to calibrate those weights and validate whether the generated cues are perceptually appropriate.

The current model learns:

- cue type classification
- presence score regression
- volume gain regression
- low-pass / reverb / occlusion output reproduction

The current model is strong enough to verify the implementation pipeline: generated situation parameters can be converted into cue-control labels, learned on the PC, exported to Unity JSON, and loaded by `PeripheralCueModel` as `LearnedCue`.

The random row split is high, but the group split is much harder. Adding `viewState` and `motionState` improved unknown-condition accuracy from 0.7282 to 0.8066. `AmbientPresence` remains the weakest class, so the next technical challenge is to improve how the model separates ambient presence from silence and other active cues in unseen conditions.

The larger limitation is still label validity. Objective simulation labels reduce developer subjectivity, but they do not prove that the cues are perceptually optimal. Final cue labels should be calibrated and evaluated with human feedback.

## What Can Be Reported

The next progress report can accurately say:

```text
Objective simulation data were generated on the PC.
A lightweight AUI cue-control model was trained from the generated data.
The model currently predicts cue type and basic cue parameters from target state, distance, speed, direction, and local position.
The model was exported to a Unity-compatible JSON file.
The next step is to use human feedback to calibrate and validate the objective simulation labels.
```

## Next Steps

1. Define the generated situation grid:
   - distance
   - direction
   - view state
   - approach speed
   - speaking
   - crossing
2. Prepare cue candidates:
   - `Footstep`
   - `Voice`
   - `AmbientPresence`
   - `ClothingRustle`
   - `Breathing`
   - `None`
3. Record evaluation measures:
   - localization accuracy
   - reaction time
   - approach recognition
   - clarity
   - naturalness
   - discomfort
4. Convert the best-performing cue into `cueType`, `presenceScore`, and `volumeGain` labels.
5. Retrain the cue-control model and compare:
   - rule-based model
   - developer-label baseline
   - evaluation-label model
