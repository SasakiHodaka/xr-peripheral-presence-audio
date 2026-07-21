# Semantic Spatial Audio — Master TODO

Last updated: 2026-07-21

This is the implementation and research checklist for the expert-to-learner
inspection/maintenance VR study. Check an item only when its evidence and stated
acceptance criteria exist. A package name in documentation does not mean that the
package is installed.

## P0 — Unblock SDK Integration

- [ ] Close every Unity process using this project.
- [ ] Reopen with Unity `2022.3.62f3` and allow Package Manager resolution.
- [x] Confirm `com.meta.xr.sdk.interaction` v85 appears in `packages-lock.json`.
- [x] Confirm there are no C# compilation errors after Meta package resolution.
- [ ] Run the existing Scene Token analyzer self-check.
- [ ] Run `SemanticTransportSelfCheck`.
- [ ] Download the compatible Photon Fusion 2 `.unitypackage` from the official account.
- [ ] Record the downloaded Fusion version and release date.
- [ ] Import Fusion without overwriting unrelated project assets.
- [ ] Install the matching Photon Voice integration from Fusion Hub.
- [ ] Record the installed Voice version.
- [ ] Create development-only Fusion and Voice App IDs.
- [ ] Keep environment-specific App IDs and credentials out of commits where appropriate.
- [ ] Verify Android/Quest support and IL2CPP configuration.
- [ ] Commit a reproducible package/version inventory.

### P0 definition of done

- Unity compiles with Meta Interaction, Fusion, and Voice installed.
- The current desktop demo still runs.
- The current Quest APK build path still succeeds.
- No App ID or credential is unintentionally exposed.

## P1 — Multiplayer Foundation

- [ ] Import the Fusion Multiplayer VR Training/XRShared sample into an isolated scene.
- [ ] Select and document Fusion topology: Shared Mode or Host Mode.
- [ ] Create a connection/bootstrap component for named experiment sessions.
- [ ] Assign stable participant IDs.
- [ ] Assign participant roles: expert and learner.
- [ ] Spawn exactly one network rig per participant.
- [ ] Synchronize head pose.
- [ ] Synchronize left-hand/controller pose.
- [ ] Synchronize right-hand/controller pose.
- [ ] Synchronize avatar representation.
- [ ] Display connection, role, and authority state in a development HUD.
- [ ] Handle participant join and leave.
- [ ] Handle reconnect without duplicate rigs.
- [ ] Define session shutdown and experiment reset behavior.

### P1 definition of done

- Two Quest devices join the same session.
- Each participant sees the other participant's head and hands.
- Join, leave, reconnect, and reset are reproducible.

## P2 — Shared Training Interaction

- [ ] Choose the first inspection/maintenance work object.
- [ ] Define stable IDs and semantic metadata for every task object.
- [ ] Add one networked grabbable tool or workpiece.
- [ ] Define state authority before, during, and after grab.
- [ ] Prevent simultaneous ownership by expert and learner.
- [ ] Synchronize object position and rotation.
- [ ] Synchronize grab and release state.
- [ ] Synchronize valid placement.
- [ ] Synchronize wrong placement and warning state.
- [ ] Synchronize task completion.
- [ ] Restore object authority after disconnect.
- [ ] Connect Meta Interaction SDK events to the existing task-event path.
- [ ] Preserve XR Interaction Toolkit as a fallback until Meta integration is validated.
- [ ] Test controller input on Quest 3.
- [ ] Decide whether hand tracking is required for the formal experiment.
- [ ] If required, implement and test hand-tracking interaction separately.

### P2 definition of done

- Both participants see the same object and task state.
- Grab authority is deterministic under simultaneous attempts.
- Existing error, success, and adaptation logs are generated from networked actions.

## P3 — Semantic Packet over Photon Fusion

- [x] Define `SemanticPacket`.
- [x] Define `SemanticPacketEnvelope`.
- [x] Define `ISemanticPacketTransport`.
- [x] Implement local loopback transport.
- [x] Connect optional transport publishing to `ScenarioPlayer`.
- [x] Add envelope round-trip and invalid-schema self-checks.
- [ ] Implement `FusionSemanticPacketTransport`.
- [ ] Decide which fields are persistent network state.
- [ ] Decide which fields are transient RPC events.
- [ ] Preserve event IDs and participant IDs across transport.
- [ ] Reject unsupported schema versions.
- [ ] Reject expired semantic events.
- [ ] Prevent duplicate event presentation.
- [ ] Support late join for persistent task state.
- [ ] Measure serialized byte count on the actual transport.
- [ ] Measure end-to-end semantic-event latency.
- [ ] Record send, receive, suppress, expire, and duplicate decisions.
- [ ] Add simulated delay/loss checks where practical.

### P3 definition of done

- A semantic event produced on one client is received once on the other client.
- The received event retains identity, meaning, target, and timing information.
- Persistent task consequences survive late join or reconnect.

## P4 — Photon Voice

- [ ] Configure `FusionVoiceClient` with the Fusion runner.
- [ ] Add one local `Recorder` per participant.
- [ ] Add a remote `Speaker` prefab.
- [ ] Add `VoiceNetworkObject` to the network player prefab.
- [ ] Associate incoming streams with the correct Fusion player object.
- [ ] Request and validate Quest microphone permission.
- [ ] Verify microphone capture on both Quest devices.
- [ ] Verify bidirectional expert/learner voice.
- [ ] Verify mute and disconnect behavior.
- [ ] Select sample rate, frame duration, bitrate, and reliability settings.
- [ ] Evaluate echo cancellation and noise suppression.
- [ ] Record Voice configuration used in each study build.
- [ ] Measure voice onset-to-playback delay.
- [ ] Add a development-only echo/debug mode.

### P4 definition of done

- Expert and learner can speak bidirectionally on two Quest devices.
- Every remote voice is attached to the correct participant.
- Voice survives normal room transitions without duplicate Speakers.

## P5 — Semantic Spatial Audio Rendering

- [ ] Define supported anchor types: speaker, task object, hazard, listener front.
- [ ] Add stable anchor IDs to presentation decisions.
- [ ] Implement a runtime anchor resolver.
- [ ] Route each remote Voice `Speaker` through the semantic presentation controller.
- [ ] Preserve intelligibility while moving an active voice source.
- [ ] Define transition duration between anchors.
- [ ] Prevent rapid anchor oscillation.
- [ ] Define gain limits and hearing-safe defaults.
- [ ] Define distance attenuation.
- [ ] Define spatial blend and near-field behavior.
- [ ] Decide whether occlusion and room acoustics are experimental variables or fixed controls.
- [ ] Implement critical-warning background attenuation if included in the study.
- [ ] Log requested and actually applied anchor, position, gain, and duration.
- [ ] Verify left/right/front/back localization on Quest 3.
- [ ] Verify behavior when the referenced task object is missing or despawned.

### P5 definition of done

- The same live voice can be reproducibly presented from each supported anchor.
- Applied rendering parameters are logged.
- Missing anchors fail safely to the speaker position.

## P6 — Situation Acquisition

- [x] Acquire scripted speaker, target, priority, and timing data.
- [x] Acquire local grab, release, wrong-placement, and success events.
- [x] Acquire explicit help requests.
- [ ] Acquire authoritative network participant role.
- [ ] Acquire remote head and hand/controller pose.
- [ ] Acquire current held object and owner.
- [ ] Acquire current task step.
- [ ] Acquire expected next action.
- [ ] Acquire listener-to-target distance.
- [ ] Acquire whether the listener is oriented toward the target.
- [ ] Define gaze approximation when eye tracking is unavailable.
- [ ] Acquire hesitation and repeated-error evidence.
- [ ] Add timestamps from a consistent clock domain.
- [ ] Define missing-data and stale-data behavior.
- [ ] Log the exact evidence used for every adaptation.
- [ ] Add instruction, head-turn onset, target-facing, target-selection, and correct-action timestamps.
- [ ] Record speaking time, turn count, and participation balance.
- [ ] Record shared-attention duration where the available device permits a valid measure.

## P7 — Meaning and Importance Inference

- [x] Implement deterministic direction and distance labels.
- [x] Implement rule-based priority and relevance scoring.
- [x] Preserve critical-event override.
- [ ] Add live speech-to-text input.
- [ ] Decide whether speech recognition runs on Quest, PC, or a service.
- [ ] Extract speaker identity.
- [ ] Classify intent: instruction, explanation, warning, confirmation, conversation.
- [ ] Resolve spoken target names to task-object IDs.
- [ ] Extract requested action and direction.
- [ ] Estimate urgency independently of preassigned priority.
- [ ] Add confidence values and an uncertain/fallback state.
- [ ] Define inference timeout behavior.
- [ ] Preserve a deterministic rule baseline for comparison.
- [ ] Compare learned/LLM inference against the rule baseline before formal use.
- [ ] Avoid sending identifiable speech to external services without protocol approval.

## P8 — User Adaptation

- [x] Implement continuous `guidanceNeed` rather than fixed user categories.
- [x] Increase guidance need after help and errors.
- [x] Decrease guidance need after successful correction.
- [x] Adapt selection, message detail, cue size, duration, and alert gain.
- [ ] Add network-authoritative adaptation state.
- [ ] Add hesitation evidence.
- [ ] Add repeated-success evidence.
- [ ] Add target-specific knowledge state.
- [ ] Define decay or reset between task sections.
- [ ] Clamp adaptation rate to prevent abrupt presentation changes.
- [ ] Expose adaptation evidence in the experiment log.
- [ ] Validate adaptation parameters in a pilot.
- [ ] Freeze parameters before the formal study.

## P9 — Experimental Conditions

- [ ] Freeze the baseline task and object layout.
- [ ] Freeze expert instruction wording or define controlled variation.
- [ ] Condition A: non-spatial/standard voice chat.
- [ ] Condition B: speaker-position spatial voice.
- [ ] Condition C: semantic anchor selection without user adaptation.
- [ ] Condition D: semantic anchor selection with user adaptation, if sample size permits.
- [ ] Ensure identical task logic across conditions.
- [ ] Include target direction as an experimental factor instead of averaging all directions.
- [ ] Counterbalance condition order.
- [ ] Define training trials separately from measured trials.
- [ ] Define failure, timeout, withdrawal, and equipment-error handling.
- [ ] Prevent debug HUD information from leaking condition logic to participants.
- [ ] Record software version, device ID, condition, seed, and configuration.

## P10 — Measures and Logging

- [x] Log packet size and selection decisions locally.
- [x] Log interaction, adaptation, and pilot-run summaries.
- [ ] Log task completion time.
- [ ] Log instruction-to-correct-action time.
- [ ] Log target acquisition time.
- [ ] Log incorrect tool selections.
- [ ] Log wrong operations and recovery time.
- [ ] Log missed or repeated instructions.
- [ ] Log network round-trip/one-way timing assumptions.
- [ ] Log voice latency measurement method.
- [ ] Add NASA-TLX or selected workload measure.
- [ ] Add intelligibility and localization ratings.
- [ ] Add presence, usability, and preference measures where justified.
- [ ] Define primary and secondary outcomes before data collection.
- [ ] Validate CSV schema and escaping.
- [ ] Add participant pseudonymization and data-retention rules.
- [ ] Add an automated per-session completeness report.

## P11 — Validation and Study Readiness

- [ ] Run all editor self-checks with zero failures.
- [ ] Run scene validation with zero failures.
- [ ] Build the Windows comparison demo.
- [ ] Build the Quest APK.
- [ ] Test on two matching Quest 3 devices.
- [ ] Test with the final audio output device.
- [ ] Test microphone permission from a clean install.
- [ ] Test weak Wi-Fi, packet loss, and reconnect behavior.
- [ ] Test a complete experiment without Editor involvement.
- [ ] Complete at least one researcher dry run.
- [ ] Complete a small pilot with representative users.
- [ ] Analyze pilot logs and revise thresholds.
- [ ] Freeze the study build and tag the commit.
- [ ] Archive package versions and build configuration.
- [ ] Obtain required ethics/research approval before formal participant collection.
- [ ] Run the formal study.
- [ ] Analyze results against predefined hypotheses.
- [ ] Document limitations, failures, and reproducibility information.

## Deferred Until the Baseline Works

- [ ] Learned or LLM-based end-to-end policy.
- [ ] Eye tracking that requires a different Quest model.
- [ ] Large industrial digital twin integration.
- [ ] More than two simultaneous participants.
- [ ] Shared Spatial Anchors for co-located MR.
- [ ] Complex room acoustics and dynamic occlusion.
- [ ] Production authentication, matchmaking UI, and deployment infrastructure.

These items must not delay the first two-user voice-and-object synchronization
baseline unless the research protocol is explicitly changed.
