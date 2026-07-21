# SemanticSpatialAudio

Unity 2022.3 LTS project for a semantic spatial audio research prototype.

## Active Working Project

The active Unity project for current implementation and validation work is:

```text
C:\Users\acd-pc67\SemanticSpatialAudio
```

Older worktree copies, such as `xr-peripheral-presence-audio/.worktrees/scene-token-prototype`,
should be treated as reference copies only unless explicitly synchronized.

The older peripheral presence audio prototype has been replaced in this
repository. The current research target is spatial conversation tokenization:

`Speech Object + Position + Meaning/Turn State -> Scene Token -> Spatial Audio Rendering`

# TODO — Multiplayer Semantic Spatial Audio

> **Current priority:** close Unity, resolve Meta XR Interaction SDK v85, import
> Photon Fusion 2, then establish a two-Quest session before expanding AI logic.

- [x] Implement the local Semantic Packet, selection, adaptation, and presentation prototype.
- [x] Add the Meta Quest OpenXR build path and minimum XR grab task.
- [x] Define an SDK-independent semantic transport contract and loopback check.
- [x] Resolve and compile Meta XR Interaction SDK v85.
- [ ] Import Photon Fusion 2 and record the exact installed version.
- [ ] Install Photon Voice 2 through Fusion Hub and record its exact version.
- [ ] Import or reproduce the Fusion Multiplayer VR Training/XRShared baseline.
- [ ] Connect two Quest devices to the same Fusion session.
- [ ] Synchronize head, hands/controllers, avatar, and one grabbable task object.
- [ ] Implement deterministic grab authority and reconnect behavior.
- [ ] Connect `SemanticPacketTransportRouter` to a real Fusion transport adapter.
- [ ] Add `FusionVoiceClient`, `Recorder`, `Speaker`, and `VoiceNetworkObject`.
- [ ] Associate every received voice stream with the correct remote participant.
- [ ] Switch voice anchoring between speaker, task object, hazard, and listener front.
- [ ] Acquire live situation data from speech, pose, interaction, and task progress.
- [ ] Implement and validate intent, target, action, urgency, and relevance inference.
- [ ] Finalize adaptive presentation rules without fixed novice/expert labels.
- [ ] Log network delay, packet loss, voice delay, applied audio parameters, and task events.
- [ ] Freeze the expert-to-learner inspection/maintenance task and error cases.
- [ ] Implement all experimental conditions with identical task behavior.
- [ ] Complete a two-user pilot on Quest 3.
- [ ] Revise the protocol, obtain required research approval, and run the formal study.

The expanded checklist, dependencies, acceptance criteria, and definition of done
are maintained in [`TODO.md`](TODO.md).

## Adopted Technology Stack

The target implementation is a two-user Meta Quest VR training environment in
which an expert guides a learner through an inspection or maintenance task.

| Layer | Adopted technology | Responsibility | Repository status |
| --- | --- | --- | --- |
| Target device | Meta Quest 3 | Standalone HMD, tracking, microphone, and experiment runtime | Minimal APK path implemented |
| XR runtime | Unity OpenXR | Quest runtime and controller input | Installed and configured |
| Local interaction | Meta XR Interaction SDK | Quest hand/controller interaction and task-object manipulation | v85 installed; task-scene integration pending |
| Meta scene helpers | Meta XR Multiplayer Building Blocks | Selected Quest-specific room/avatar/setup helpers only | Evaluation required; not the network authority |
| Shared state | Photon Fusion 2 | Session, network rig, object state, authority, and semantic-event synchronization | Planned |
| Voice transport | Photon Voice 2 | Low-latency expert/learner microphone streaming | Planned |
| Training foundation | Photon Fusion Multiplayer VR Training / XRShared | Two-user training, rig synchronization, grabbing, teleport, and reconnection baseline | Planned as the primary multiplayer base |
| Spatial rendering | Meta XR Audio SDK and Unity AudioSource | Runtime spatialization and semantic presentation control | Meta XR Audio installed; adaptive cues implemented |
| Research layer | Project-owned code | Situation acquisition, meaning inference, information selection, user adaptation, Scene Tokens, and spatial audio policy | Prototype implemented; network integration pending |

Fusion is the single authority for multiplayer state. Photon Voice transports
audio but does not own task state. Meta Multiplayer Building Blocks will be used
selectively where they do not duplicate Fusion session, spawn, or authority
management. See [`docs/TECHNOLOGY_STACK.md`](docs/TECHNOLOGY_STACK.md) for the
integration boundaries and implementation order.

## Quick Start

1. Open this repository with Unity 2022.3.62f3 or another Unity 2022.3 LTS editor.
2. Open `Assets/Scenes/SceneTokenMock.unity`.
3. Run `Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check`.
4. Run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.
5. Press Play.
6. Use `Space` to start or stop an experiment session.
7. Use `N` to advance to the next condition, or wait for the timer.
8. Use `1`-`5` to switch evaluation conditions manually.
9. Use `T` to start or stop the scripted conversation sequence.
10. Use `Y` to stop the scripted conversation sequence.
11. Use `A`, `B`, or `C` to toggle each avatar's speaking state manually.
12. Use `Q`, `W`, or `E` to cycle each avatar's semantic token manually.
13. Record participant responses from the HUD buttons, or use:
    - arrow keys for `FRONT`, `RIGHT`, `BACK`, `LEFT`
    - `J`, `K`, `L` for speaker `A`, `B`, `C`

## Selection Comparison Demo

The Ground Truth playback includes an in-game comparison HUD and spatial cues.
Open either scene and press Play, then use:

- `F6`: Full Transmission (all four S3 events are sent)
- `F7`: Priority-only Baseline (three events are sent)
- `F8`: Context + User State demo policy (two events are sent)
- `F4`: set current guidance need to `0.8` for comparison
- `F5`: set current guidance need to `0.2` for comparison
- `[` / `]`: decrease or increase current guidance need by `0.1`
- `H`: request help and increase current guidance need
- `X`: simulate an observed operation error and increase current guidance need
- `R`: replay the current mode and reset its counters
- `F9`: replay the automated object-transfer scenario

The HUD shows every send/suppress decision, its reason, packet bytes, and cumulative
event count. It also keeps summaries of the last four completed runs so that event and
byte reductions remain visible after switching modes or profiles. A green sphere
represents transmitted information; a gray cube represents
suppressed information. `AudioAndToken` decisions also produce a short spatial alert.

The Context + User State mode is a deterministic, explainable scoring demonstrator.
It combines urgency, task relevance, novelty, and the receiver's current guidance
need. Critical events are always sent. Higher current need retains more procedural
guidance; lower current need suppresses routine progress while preserving exceptions
and outcomes. The HUD displays the total score and all four components.

Presentation is also state-adaptive. Higher-need cues include the action, target, and
direction and remain larger and visible for longer. Lower-need cues use a compact
action-and-target message with a smaller, shorter alert. The exact need value, message,
cue scale, duration, and audio gain are recorded in the experiment CSV. `F4` and `F5`
are only reproducible presets; they are not labels assigned to a person.

The minimal S4 task starts with three fixed events: one new task step, the same step
repeated, and one unrelated update. The user then left-drags the orange cube. Placing
it on red adds a wrong-placement warning; placing it on green adds the completion
result. Following the instructed red-then-green path produces five events:

```text
Full Transmission:          5 / 5 events
Priority-only:              4 / 5 events
Proposed, need 0.8:         3 / 5 events
Proposed, need 0.2:         2 / 5 events
```

The critical warning and result are transmitted at every guidance-need setting.

Mouse task controls:

1. Hold the left mouse button on the orange cube.
2. Drag it onto the red zone and release to create an observed error.
3. Drag it again onto the green zone and release to complete the task.
4. Use `F9` to restart.

Grab, release, outside-zone, wrong-placement, and correct-placement events are written
to `interaction_events_*.csv` with task elapsed time and object position.

The demo is rendered directly by Unity using the workpiece, target zones, spatial
cues, guidance text, and alert sound. It does not depend on a pre-rendered video.

To create a runnable Windows demo, use:

```text
Tools > Semantic Spatial Audio > Build Minimal Windows Demo
```

The executable is written to `Builds/MinimalAdaptiveDemo/MinimalAdaptiveDemo.exe`.

Meta Quest 3 support uses OpenXR and XR Interaction Toolkit. The orange workpiece has
both mouse drag and `XRGrabInteractable`; Quest grip select events call the same grab,
release, wrong-zone, and correct-zone logic. Build the APK with:

```text
Tools > Semantic Spatial Audio > Build Meta Quest APK
```

The APK is written to `Builds/Quest/MinimalAdaptiveDemo.apk`.

The demo also updates guidance need from minimal observable evidence. Help requests
and errors increase need, while the successful correction event decreases it. The HUD
shows the current value and event counts. A separate `adaptation_events_*.csv` records
the reason and before/after value for every update.

Each completed five-event run also writes one row to `pilot_runs_*.csv`, including
condition, initial/final need, duration, selected/suppressed events, packet bytes, and
help/error/success counts. The completed-run summary appears on screen immediately.

## Current Prototype

The mock scene currently supports:

- 8-direction scene tokens
- 3-level distance tokens
- speaking state
- simple turn state
- manual semantic labels
- visible avatar state labels
- CSV token logging
- event logging for experiment sessions
- direction and speaker response logging
- deterministic scripted conversation playback
- communication volume metrics
- HUD participant/session fields and completion feedback
- token and metric logging gated by experiment session state
- token-based AudioSource position, volume, and pitch reconstruction
- generated fallback tone when no recorded speaking clip is assigned

## Documentation

- `TODO.md`: master implementation and study-readiness checklist
- `docs/INTEGRATED_RESEARCH_PLAN.md`: consolidated research question, system, conditions, and execution plan
- `docs/LITERATURE_SYNTHESIS.md`: mapping from the local paper corpus to design, measures, and implementation
- `docs/PROJECT_STATUS.md`: current state, validation result, known issues
- `docs/RESEARCH_STORY_FORMAT.md`: basic thesis and presentation story format
- `docs/RESEARCH_STORY_BRIEF.md`: short research story for slide planning
- `docs/EXPERIMENT_PROTOCOL.md`: how to run a trial and collect logs
- `docs/EVALUATION_DATA_SPEC.md`: scenario ground truth and evaluation CSV contract
- `docs/ARCHITECTURE.md`: script responsibilities and data flow
- `Docs/IMPLEMENTATION_FLOW.md`: detailed runtime implementation flow
- `docs/SCENE_TOKEN_SPEC.md`: research definition, token fields, and design rationale
- `docs/RELATED_WORK_QA.md`: concise related-work comparison and defense Q&A
- `docs/NEXT_STEPS.md`: recommended next development tasks
- `Assets/Scripts/SceneToken/README_SceneTokens.md`: implementation-level notes

## Repository Layout

- `Assets/Editor`: Unity editor tooling and scene wizard
- `Assets/Scenes`: Unity scenes
- `Assets/Scripts/SceneToken`: token model, manager, logger, decoder, metrics
- `Assets/Scripts/UI`: debug labels and UI helpers
- `Assets/Audio`: optional voice clips
- `Assets/Data`: sample metadata and analysis data
- `Assets/Prefabs`: reusable scene objects
- `Packages`: Unity package manifest and lock file
- `ProjectSettings`: Unity project settings
- `Tools`: analysis scripts

## Log Analysis

Metric logs are written to Unity's `Application.persistentDataPath`.

Run:

```bash
python Tools/check_latest_response_run.py <unity_log_directory>
python Tools/collect_latest_scene_token_run.py <unity_log_directory> Runs/latest_run
python Tools/analyze_scene_token_logs.py Runs/latest_run
python Tools/analyze_scene_packet_logs.py Runs/latest_run
python Tools/analyze_token_logs.py Runs/latest_run token_summary.csv
python Tools/analyze_event_logs.py Runs/latest_run event_summary.csv
python Tools/summarize_experiment_run.py Runs/latest_run summary.md
```

## Scene Validation

In Unity, run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.

Batch mode:

```bash
Unity.exe -batchmode -quit -projectPath <project> -executeMethod SceneTokenSceneValidator.ValidateSceneForBatch
```
