# AUI Training Report

## Current Status

The first lightweight AUI cue-control learning pipeline is running end to end.

This report describes an initial baseline. The current labels come from existing cue rules and developer-selected cue behavior, so they should be used to verify the training pipeline, not as final evidence that the cues are correct for users.

Pipeline:

```text
Unity peripheral CSV logs
-> cue_training_dataset.csv
-> lightweight cue-control model training
-> Models/cue_model.json
-> cue_training_predictions.csv
```

## Commands

Build the training dataset:

```powershell
python Tools/build_cue_training_dataset.py --include-none
```

Train and evaluate the first model:

```powershell
python Tools/train_cue_model.py --epochs 40
```

## Latest Training Run

Dataset:

- Source CSV files: 18
- Rows: 17,562

Train/test split:

- Train rows: 13,172
- Test rows: 4,390

Training class counts:

- `Footstep`: 9,670
- `Voice`: 1,705
- `None`: 1,619
- `AmbientPresence`: 178

Evaluation:

- Train `cueType` accuracy: 0.8070
- Test `cueType` accuracy: 0.8091
- Test `presenceScore` MAE: 0.3074
- Test `volumeGain` MAE: 0.3074
- Test `cueLowPassHz` MAE: 0.0000
- Test `cueReverbAmount` MAE: 0.0000
- Test `cueOcclusionGain` MAE: 0.0000

Outputs:

- `Models/cue_model.json`
- `cue_training_dataset.csv`
- `cue_training_predictions.csv`

Dataset distribution can be checked with:

```powershell
python Tools/summarize_cue_training_dataset.py
```

## Interpretation

This is an initial baseline, not the final AUI model.

The current dataset is generated from existing Unity logs. Older logs did not contain cue target columns, so `Tools/build_cue_training_dataset.py` fills missing cue labels using the current rule-based cue policy. This allows model training to start before new experiment logs are collected.

The current model learns:

- cue type classification
- presence score regression
- volume gain regression
- low-pass / reverb / occlusion output reproduction

The zero MAE for low-pass, reverb, and occlusion means the current generated labels are constant for older logs. These targets will become meaningful only after new data contains real variation.

The larger limitation is label reliability. The current dataset can show that the model can reproduce prototype labels, but it cannot prove that those labels are perceptually appropriate. Final cue labels should be generated from simulated situations and evaluation results.

## What Can Be Reported

The next progress report can accurately say:

```text
Unity logs were converted into a cue-control training dataset.
An initial AUI learning pipeline was implemented.
A lightweight baseline model was trained and evaluated.
The model currently predicts cue type and basic cue parameters from target state, distance, speed, and local position.
The current labels are prototype labels, so the next step is to build evaluation-derived cue labels from generated situations and candidate cue performance.
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
