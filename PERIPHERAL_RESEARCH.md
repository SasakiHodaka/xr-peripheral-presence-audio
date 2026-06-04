# Peripheral Research Notes

## Demo Flow

1. Open `Assets/Scenes/SampleScene.unity`.
2. Run `Tools > Peripheral Research > Create Demo Hierarchy`.
3. Select `PeripheralSystem`.
4. Assign `Main Camera` or `XR Origin/Main Camera` to `PeripheralStateDetector.userHead` if it is empty.
5. Set `PeripheralStateLogger.participantId`, `conditionLabel`, and `trialId` in the Inspector.
6. Set `PeripheralCueExperimentController.cueCandidate` to the sound candidate for the trial.
7. Set `PeripheralTrialController.trialDurationSeconds` if the trial length should differ from the default.
8. Enter Play Mode.
9. Confirm that the Game view shows `Peripheral Debug`.
10. Confirm that Unity Console prints `Peripheral CSV created: ...`.

CSV files are written to Unity's `Application.persistentDataPath`.
In the current Windows Editor setup this is typically:

```text
C:\Users\acd-pc67\AppData\LocalLow\DefaultCompany\My project
```

The logger uses timestamped filenames by default:

```text
peripheral_state_log_P001_demo_T001_yyyyMMdd_HHmmss.csv
```

`PeripheralStateLogger.includeExperimentMetadataInFileName` controls whether `participantId`, `conditionLabel`, and `trialId` are included in the file name.
The analysis script also uses this filename pattern as a fallback when older CSV rows do not contain metadata columns.

## CSV Columns

- `participantId`: Participant identifier set on `PeripheralStateLogger`.
- `conditionLabel`: Experimental condition label set on `PeripheralStateLogger`.
- `trialId`: Trial identifier set on `PeripheralStateLogger`.
- `cueCondition`: Cue comparison mode from `PeripheralCueModel`: `NoCue`, `FixedCue`, `StateBasedCue`, or `EnvironmentAdaptiveCue`.
- `roomScale`: Current `EnvironmentAcousticProfile.roomScale`.
- `materialClass`: Current `EnvironmentAcousticProfile.materialClass`.
- `environmentReverbAmount`: Current manual environment reverb value.
- `environmentOcclusionStrength`: Current manual environment occlusion strength.
- `environmentDistanceAttenuation`: Current manual environment distance attenuation.
- `environmentRt60`: Current manual RT60 value.
- `environmentDrr`: Current manual DRR value.
- `time`: Unity play time in seconds.
- `trialElapsed`: Elapsed trial time from `PeripheralTrialController`.
- `trialDuration`: Expected trial length from `PeripheralTrialController`.
- `targetId`: Target identifier, such as `Target_Approach`.
- `state`: Combined peripheral state flags.
- `outOfView`: Target is outside the configured field of view.
- `approaching`: Target is moving toward the user.
- `speaking`: Target is marked as speaking.
- `gazing`: Target is facing the user within the gaze threshold.
- `near`: Target is inside the near-distance threshold.
- `crossing`: Target is crossing in front of the user.
- `distance`: Distance from user head to target in meters.
- `viewAngle`: Angle between user forward direction and target direction.
- `radialSpeed`: Positive value means the target is approaching.
- `lateralSpeed`: Sideways movement speed in user-local space.
- `localX`, `localY`, `localZ`: Target position in user-head local coordinates.
- `expectedDirection`: Direction label derived from `localX` and `localZ`: `Left`, `Right`, `Front`, or `Rear`.
- `cueType`: Predicted cue category from `PeripheralCueModel`.
- `presenceScore`: Predicted peripheral presence strength.
- `volumeGain`: Predicted audio gain to use when the cue is played.
- `cueLowPassHz`: Low-pass cutoff predicted by `PeripheralCueModel` and `EnvironmentAcousticProfile`.
- `cueReverbAmount`: Reverb amount predicted by `PeripheralCueModel` and `EnvironmentAcousticProfile`.
- `cueOcclusionGain`: Occlusion gain predicted by `PeripheralCueModel` and `EnvironmentAcousticProfile`.
- `cueCandidate`: Sound candidate presented in the current trial.
- `responseGiven`: Whether the participant pressed the detection response key.
- `reactionTime`: Trial elapsed time when the detection response was first pressed.
- `responseKey`: Detection key name, usually `Space`.
- `directionResponse`: Direction key response, such as `Left`, `Right`, `Front`, or `Rear`.
- `directionCorrect`: Whether `directionResponse` matches `expectedDirection`.
- `subjectiveRating`: Numeric rating entered with keys `1` to `5`.
- `playbackCue`: Cue candidate currently controlled by `PeripheralCueAudioEmitter`.
- `playbackActive`: Whether `PeripheralCueAudioEmitter` currently considers the cue audible.
- `playbackVolume`: Actual target output volume after base gain scaling.
- `playbackLowPassHz`: Current low-pass cutoff used for spatial/rear/far cue filtering.
- `playbackReverbAmount`: Current reverb amount used for cue playback.
- `footstepInterval`: Current interval used for footstep one-shot playback.

## Cue Candidate Trials

`PeripheralCueExperimentController` controls the sound candidate and participant response fields.

Initial cue candidates:

- `NoCue`
- `PredictedCue`
- `Footstep`
- `Breathing`
- `ClothRustle`
- `Voice`
- `AmbientPresence`
- `MixedCue`

Default response keys:

- `Space`: detection response.
- `LeftArrow`, `RightArrow`, `UpArrow`, `DownArrow`: direction response.
- `1` to `5`: subjective rating.

Use one cue candidate per trial when collecting labels. For example:

```text
BackApproach + Footstep
BackApproach + Breathing
BackApproach + ClothRustle
BackApproach + AmbientPresence
```

`PeripheralCueAudioEmitter` plays the selected candidate as spatial audio at the currently most salient peripheral target.

`PeripheralCueTrialSequencer` can cycle through condition and cue-candidate combinations.

Default sequencer keys:

- `N`: next condition/cue trial.
- `B`: previous condition/cue trial.
- `R`: restart the current trial.

Useful sequencer settings:

- `repeatsPerCombination`: number of repeated trials for each condition/cue pair.
- `randomizeOrder`: shuffles the trial order using `randomSeed`.
- `updateLoggerMetadata`: writes condition/cue labels and automatic trial IDs to `PeripheralStateLogger`.

Automatic trial IDs use this form:

```text
T001_R01
T002_R01
...
```

The default sequence covers:

```text
BackApproach
Approach
Crossing
Speaking
```

combined with:

```text
NoCue
Footstep
Breathing
ClothRustle
Voice
AmbientPresence
MixedCue
```

## Initial Metrics To Inspect

- How often `outOfView` is true while `approaching` is also true.
- Time from first `approaching` detection to `near`.
- Whether `crossing` appears during `Target_Crossing` movement.
- Whether `speaking` appears only for speaking targets.
- Whether `viewAngle` and `localX` match the user's intuitive left/right and front/back perception.

## Trial Timing

`PeripheralTrialController` is added to `PeripheralSystem` by `Create Demo Hierarchy`.
It tracks elapsed trial time and displays it in `Peripheral Debug`.

- `trialDurationSeconds`: expected trial length.
- `preTrialSeconds`: preparation time before logging and target movement start.
- `autoStopEditorPlayMode`: stops Play Mode automatically when the duration is reached. Keep this off unless you want automatic stopping during Unity Editor tests.
- `logTrialCompleted`: prints a Console message when the trial duration is reached.

During pre-trial time, demo target movement and CSV row logging are paused. The Game view shows `Pre-trial: ...s`.

## Trial Conditions

`PeripheralTrialConditionController` is added to `PeripheralSystem` by `Create Demo Hierarchy`.
Use it to move from the all-target demo toward one-condition-per-trial runs.

- `AllDemoTargets`: enables all demo targets.
- `Approach`: enables only `Target_Approach`.
- `BackApproach`: enables only `Target_Back`.
- `Crossing`: enables only `Target_Crossing`.
- `Speaking`: enables only `Target_Speaking`.
- `None`: disables all demo targets.

When `updateLoggerConditionLabel` is enabled, `PeripheralStateLogger.conditionLabel` is updated from the selected condition.

## CSV Analysis Script

Run this from the project root to analyze the newest CSV log:

```powershell
python Tools/analyze_peripheral_csv.py
```

This also writes a summary CSV next to the source log:

```text
peripheral_state_log_yyyyMMdd_HHmmss_summary.csv
```

To analyze a specific CSV file:

```powershell
python Tools/analyze_peripheral_csv.py "C:\Users\acd-pc67\AppData\LocalLow\DefaultCompany\My project\peripheral_state_log_yyyyMMdd_HHmmss.csv"
```

To print only without writing a summary CSV:

```powershell
python Tools/analyze_peripheral_csv.py --no-summary-csv
```

To summarize all source logs in the log directory:

```powershell
python Tools/analyze_peripheral_csv.py --batch
```

This writes:

```text
peripheral_batch_summary.csv
```

To generate a browser-readable HTML report:

```powershell
python Tools/analyze_peripheral_csv.py --html-report
```

This writes:

```text
peripheral_report.html
```

The batch CSV and HTML report include `demoCheck`. This is a quick demo-health check, not a final research metric:

- `Target_Approach`: expects `approaching` and `near`.
- `Target_Back`: expects `outOfView + approaching`.
- `Target_Crossing`: expects `crossing`.
- `Target_Speaking`: expects `speaking`.

Very short Play Mode sessions can show `Check approach/near` because the approach target may not have enough time to reach `near`.

The batch CSV and HTML report also include `durationCheck`. Sessions shorter than 10 seconds are marked as `Short (<10s)` because the approach target may not have enough time to reach the near-distance threshold.

Use `--min-duration` to change this threshold:

```powershell
python Tools/analyze_peripheral_csv.py --batch --min-duration 15
python Tools/analyze_peripheral_csv.py --html-report --min-duration 15
```

Use `--log-dir` when analyzing logs from another project or copied folder:

```powershell
python Tools/analyze_peripheral_csv.py --batch --log-dir "C:\path\to\logs"
python Tools/analyze_peripheral_csv.py --html-report --log-dir "C:\path\to\logs"
```

The script prints per-target row counts, state counts, first detection times, `outOfView + approaching` counts, and the time from first `approaching` to first `near`.

## AI Training Dataset

Build the first compact cue-control training dataset with:

```powershell
python Tools/build_cue_training_dataset.py --output Logs/cue_training_dataset.csv
```

This writes:

```text
cue_training_dataset.csv
```

The first model should predict `cueType`, `presenceScore`, `volumeGain`, `cueLowPassHz`, `cueReverbAmount`, and `cueOcclusionGain` from target state, distance, speed, and local position columns.

Use `cueCondition` to separate fixed, state-based, and environment-adaptive cue rows during baseline training and evaluation.

Train the lightweight Unity-readable cue model with:

```powershell
python Tools/train_cue_model.py --dataset Logs/cue_training_dataset.csv --model Assets/Models/cue_model_unity.json --predictions Logs/cue_training_predictions.csv
```

The generated model is consumed by `PeripheralCueModel` when `comparisonCondition` is set to `LearnedCue` and `learnedModelJson` points to `Assets/Models/cue_model_unity.json`.

## Cue Candidate Effectiveness

To compute cue candidate effectiveness and best-cue labels from a source CSV:

```powershell
python Tools/analyze_peripheral_csv.py --cue-effectiveness
```

This writes:

```text
peripheral_state_log_yyyyMMdd_HHmmss_cue_effectiveness.csv
```

The effectiveness score is:

```text
detectionSuccess
+ directionAccuracy
- normalizedReactionTime
+ normalizedRating
```

When older logs do not contain `directionCorrect`, the analysis script falls back to `directionResponseRate`.
Rows marked `isBestCue=True` are the current label candidates for `cueType`.

To export only the best cue labels with model targets:

```powershell
python Tools/analyze_peripheral_csv.py --label-dataset
```

This writes:

```text
peripheral_state_log_yyyyMMdd_HHmmss_cue_labels.csv
```

The label dataset includes:

- `cueType`
- `presenceScore`
- `volumeGain`
- source metrics used to compute the target values

## AUI Log Collection Controller

`PeripheralAuiLogCollectionController` automates AUI dataset collection.

It cycles through:

- target scenarios: `Approach`, `BackApproach`, `Crossing`, `Speaking`
- cue conditions: `NoCue`, `FixedCue`, `StateBasedCue`, `EnvironmentAdaptiveCue`
- environment presets: `Neutral`, `Reverberant`, `Occluded`

Use it by running:

```text
Tools > Peripheral Research > Create Demo Hierarchy
```

Then select `PeripheralSystem` and enable `PeripheralAuiLogCollectionController.autoAdvanceTrials` if you want one Play Mode session to advance across all configured trials. The logger writes the combined condition into `conditionLabel` and writes the cue condition and environment profile values into dedicated CSV columns.

## Current Scope

Older Presence and greeting-meeting assets remain in the project. Do not delete them yet; `PeripheralTarget` can still bridge to `PresenceTarget` and `GroupWorkPresenceAudio`.
