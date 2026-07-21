# Technology Stack and Integration Boundaries

Last updated: 2026-07-21

## Decision

The multiplayer implementation will start from Photon Fusion Multiplayer VR
Training and XRShared, with Photon Fusion 2 as the only shared-state authority
and Photon Voice 2 as the voice transport. Meta XR features will be added for
Quest-specific interaction and audio rendering without creating a second
network ownership system.

This decision keeps the research contribution separate from infrastructure:

```text
Quest tracking and interaction
  -> observable user/task state
  -> semantic inference and selection
  -> Scene Token + presentation decision
  -> Fusion state/event transport
  -> Photon Voice audio transport
  -> semantic spatial audio rendering
```

## Responsibility Matrix

### Meta Quest 3

- Runs the standalone experiment application.
- Provides head/controller/hand pose and microphone input.
- Must not perform authoritative research inference independently on both peers
  when that could produce conflicting results.

### Unity OpenXR

- Provides the current Quest runtime and controller path.
- Remains the minimum fallback interaction path while Meta-specific packages are
  introduced.

### Meta XR Interaction SDK

- Package v85 is installed; integration into the controlled task scene remains pending.
- Provides local hand/controller interaction, grab, poke, ray, and locomotion
  components where they improve the Quest experience.
- Emits task observations such as grab, release, target contact, and pointing.
- Does not synchronize objects by itself in this architecture.

### Meta XR Multiplayer Building Blocks

- May provide selected Quest-specific setup, avatar, or platform helpers.
- Must not create a second session, player-spawn, or object-authority path beside
  Photon Fusion.
- Each block must be tested in an isolated integration scene before entering the
  experiment scene.

### Photon Fusion 2

- Owns session membership, network player spawning, shared object state, and
  authority transfer.
- Synchronizes persistent facts as networked state, including task step, object
  owner, warning state, and active experiment condition.
- Sends transient semantic events only when late joiners do not need their
  history. Persistent consequences must be represented as state rather than only
  as RPCs.

### Photon Voice 2

- Captures and transports live speech with `Recorder` and `Speaker` components.
- Follows Fusion room membership through the Fusion/Voice integration.
- Supplies received voice to a Unity audio source whose position and presentation
  parameters are controlled by the research layer.
- Does not transport task state or Scene Tokens.

### Fusion Multiplayer VR Training and XRShared

- Provides the initial two-user training scene and networked XR rig architecture.
- Supplies reference implementations for rig synchronization, grabbing,
  teleportation, connection, and reconnection.
- Training-specific sample logic must remain separable from the research policy so
  that experiment conditions use the same task behavior.

### Meta XR Audio SDK and Unity audio

- Render the received voice and generated cues spatially.
- Apply the selected source anchor, gain, distance behavior, and other approved
  presentation parameters.
- Must expose the actual applied parameters to experiment logging.

### Project-owned research layer

The repository owns and evaluates:

1. Situation acquisition from speech, pose, interaction, and task state.
2. Meaning inference: speaker, intent, target, action, urgency, and relevance.
3. Information selection and suppression.
4. Continuous user adaptation based on observable evidence.
5. Scene Token and Semantic Packet representation.
6. Selection of the spatial source anchor and presentation parameters.
7. Reproducible experiment logging and comparison conditions.

## Required Data Contracts

The integration must preserve these logical messages regardless of SDK-specific
components:

```text
ParticipantState
  participantId, role, headPose, leftHandPose, rightHandPose, guidanceNeed

TaskObjectState
  objectId, pose, state, ownerParticipantId, taskRelevance

SemanticEvent
  eventId, speakerId, intent, targetId, action, urgency, timestamp

PresentationDecision
  condition, selected, anchorType, anchorId, gain, duration, reason
```

`SemanticPacket`, `PresentationPolicy`, `PrioritySelectionPolicy`, and
`UserAdaptationController` remain the current local prototype implementation of
the research layer. SDK adapters must call into this layer instead of duplicating
its policy.

## Implementation Order

1. Preserve the current desktop and Quest single-user baseline.
2. Import and run the unmodified Photon Fusion VR Training sample separately.
3. Establish a two-Quest Fusion session and synchronize head/hands.
4. Add Photon Voice and verify unmodified speaker-attached spatial voice.
5. Synchronize one grabbable task object and its authority.
6. Connect interaction events to the existing situation/adaptation pipeline.
7. Send Semantic Events/Packets through a Fusion adapter.
8. Redirect received voice between speaker, task-object, and listener-relative
   anchors according to `PresentationDecision`.
9. Add selected Meta Interaction SDK and Building Block features one at a time.
10. Validate latency, disconnect behavior, logging, and all experiment conditions
    on two Quest devices.

The transport-neutral boundary for steps 6-7 now exists in
`Assets/Scripts/Collaboration`. Run
`Tools > Semantic Spatial Audio > Run Semantic Transport Self Check` to verify
envelope round-trip, schema rejection, and disconnected behavior before adding an
SDK-specific adapter.

## Acceptance Criteria for the First Multiplayer Increment

- Two Quest users join the same named experiment session.
- Both users see synchronized head and hand/controller poses.
- Voice is associated with the correct remote participant.
- One task object has deterministic grab authority and synchronized placement.
- The same interaction event enters the existing policy and CSV logging path on
  the authoritative peer.
- Disconnect and reconnect do not duplicate a participant or task object.
- The desktop comparison demo remains runnable.

## External Setup Still Required

- Import compatible Photon Fusion 2, Photon Voice 2, Fusion VR Training/XRShared,
  and Meta XR Interaction SDK versions.
- Create separate Photon Fusion and Voice App IDs and keep them out of source
  control when they are credentials or environment-specific configuration.
- Confirm SDK and Unity 2022.3 compatibility in a disposable integration branch or
  scene before modifying the controlled experiment scene.
- Record exact package/sample versions after the compatibility check succeeds.
