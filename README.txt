Unity Presence Audio + Help Intent Package
==========================================

Overview
--------
This package contains two research-oriented Unity modules:

1. Presence audio support
   - Estimates the presence of nearby collaborators.
   - Maps distance, approach speed, gaze attention, role, and speaking state to spatial audio cues.

2. Help request intent support
   - Records user behavior logs.
   - Extracts short-window features.
   - Estimates HelpRequestScore with rule-based logic.
   - Shows a help icon when a collaborator may need support.

3. Scene Token semantic spatial audio support
   - Defines Scene Token Model as an information model for collaborative speech events.
   - Generates Scene Tokens from speaker, direction, distance, intent, urgency, target object, and speech state.
   - Applies Rendering Policy rules to convert Scene Tokens into Audio Strategy.
   - Reconstructs spatial audio from Audio Strategy with rule-based mappings.
   - Focuses on situation awareness and task performance in VR collaborative work.

Included Files
--------------
Presence audio:

- Assets/Scripts/PresenceTarget.cs
- Assets/Scripts/PresenceAudioEmitter.cs
- Assets/Scripts/PresenceAudioManager.cs
- Assets/Scripts/ExampleVoiceState.cs

Help intent:

- Assets/Scripts/HelpIntent/BehaviorLogger.cs
- Assets/Scripts/HelpIntent/FeatureExtractor.cs
- Assets/Scripts/HelpIntent/HelpRequestEstimator.cs
- Assets/Scripts/HelpIntent/HelpRequestNotifier.cs
- Assets/Scripts/HelpIntent/HelpRequestEventLogger.cs
- Assets/Scripts/HelpIntent/ExperimentConditionManager.cs
- Assets/Scripts/HelpIntent/CsvLogWriter.cs
- Assets/Scripts/HelpIntent/HelpIntentParticipantConfig.cs
- Assets/Scripts/HelpIntent/HelpIntentManualInput.cs
- Assets/Scripts/HelpIntent/HelpIntentDebugHud.cs

Scene Token:

- Assets/Scripts/SceneTokens/SceneToken.cs
- Assets/Scripts/SceneTokens/SceneTokenEnums.cs
- Assets/Scripts/SceneTokens/SceneTokenParticipant.cs
- Assets/Scripts/SceneTokens/SceneTokenGenerator.cs
- Assets/Scripts/SceneTokens/SceneTokenSelector.cs
- Assets/Scripts/SceneTokens/SceneTokenLoopbackTransport.cs
- Assets/Scripts/SceneTokens/SemanticSpatialAudioReconstructor.cs
- Assets/Scripts/SceneTokens/SceneTokenManualInput.cs
- Assets/Scripts/SceneTokens/SceneTokenDebugHud.cs
- Assets/Scripts/SceneTokens/SceneTokenMetricsLogger.cs

Tools:

- Tools/build_training_dataset.py
- Tools/annotation_template.csv
- Tools/self_report_template.csv

Docs:

- Docs/SetupGuide.md
- Docs/HelpIntentSetupGuide.md
- Docs/SceneTokenSetupGuide.md

Editor menu:

- Tools/Help Intent/Create Local Test Rig
- Tools/Help Intent/Create Help Icon Prefab
- Tools/Help Intent/Create Experiment Controller

Basic Setup
-----------
Copy the `Assets` folder into a Unity project.

Fastest test path:

1. Open Unity.
2. Import or copy this package's `Assets` folder.
3. Select `Tools > Help Intent > Create Local Test Rig`.
4. Press Play.
5. Hold `V` to simulate speaking, press `R` to simulate repeated failed actions, press `H` to record a help event.

Remote avatar:

- Add `PresenceTarget`
- Add `PresenceAudioEmitter`
- Add `HelpIntent.HelpRequestNotifier` if help intent notification is used

Local player / XR Origin:

- Add `PresenceAudioManager`
- Add `HelpIntent.CsvLogWriter`
- Add `HelpIntent.BehaviorLogger`
- Add `HelpIntent.FeatureExtractor`
- Add `HelpIntent.HelpRequestEstimator`
- Add `HelpIntent.HelpRequestEventLogger`
- Add `HelpIntent.HelpIntentParticipantConfig`
- Add `HelpIntent.HelpIntentManualInput`
- Add `HelpIntent.HelpIntentDebugHud`

Experiment controller:

- Add `HelpIntent.ExperimentConditionManager`

Scene Token semantic spatial audio:

- Add `SceneTokens.SceneTokenParticipant` to remote collaborator avatars
- Add `SceneTokens.SceneTokenGenerator` to instantiate Scene Tokens from scene context
- Add `SceneTokens.SceneTokenSelector` if priority-based token selection is evaluated
- Add `SceneTokens.SceneTokenLoopbackTransport`
- Add `SceneTokens.SemanticSpatialAudioReconstructor` for Rendering Policy based reconstruction
- Add `SceneTokens.SceneTokenDebugHud` for runtime communication metrics
- Add `SceneTokens.SceneTokenMetricsLogger` to save communication metrics to CSV

Research summary:

- `Docs/ResearchSummary.md`

Experiment Conditions
---------------------
- `None`: no support notification
- `PresenceOnly`: presence awareness only
- `HelpRequest`: presence awareness plus help request notification

Output Logs
-----------
Help intent logs are written to:

Application.persistentDataPath/help_intent_logs

Generated files:

- behavior_log_*.csv
- feature_log_*.csv
- decision_log_*.csv
- event_log_*.csv

Training Dataset
----------------
Use `Tools/build_training_dataset.py` to merge feature logs, event logs, third-party annotations, and self reports into `training_dataset.csv`.

`labelLevel >= 2` is treated as `binaryHelpRequest = 1`.

Notes
-----
This package does not include audio clip assets.
Assign suitable clips to the AudioSource components.

If no help icon prefab is assigned, `HelpRequestNotifier` creates a simple yellow `?` icon at runtime.
