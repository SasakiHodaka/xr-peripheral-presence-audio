# Implementation Milestones

Last updated: 2026-07-21

This roadmap connects the implementation order to the research story:

```text
Situation acquisition
  -> situation understanding
  -> information selection
  -> user adaptation
  -> Scene Token representation
  -> communication
  -> spatial presentation
  -> evaluation
```

`Implemented` means that a deterministic prototype exists. It does not mean
that the research hypothesis has already been validated with participants.

## Current Position

The adopted runtime and multiplayer integration boundaries are documented in
`docs/TECHNOLOGY_STACK.md`. Photon Fusion is the single multiplayer state
authority; Photon Voice carries live voice; Meta XR packages provide selected
Quest interaction, platform, and spatial-audio capabilities.

| Milestone | Status | Current evidence | Exit condition |
| --- | --- | --- | --- |
| M0 Requirements and related work | In progress | Scene Token specification and evaluation documents exist | Fix the VR collaboration task, user roles, and required awareness information |
| M1 Minimum collaboration environment | Partial | Three-speaker controlled Unity scene and shared task objects exist | Two networked VR users can communicate and manipulate the same object |
| M2 Situation acquisition | Partial | Speaker identity, position, listener pose, speech state, task state, object, priority, and timestamps are acquired or scripted | Log all required fields from a repeatable VR task without manual repair |
| M3 Situation understanding | Partial | Rule-based direction, distance, speaking state, turn state, and semantic labels exist | Produce a reproducible situation model with documented input-to-label rules |
| M4-1 Rule-based information selection | Implemented | Full, priority-only, and context/user-state comparison policies; deterministic S4 transfer scenario | Automated policy checks and Unity scene validation pass |
| M4-2 Scored prioritization | Implemented prototype | Explainable urgency, relevance, novelty, and receiver-role score is logged and visualized | Add measured time criticality and validate weights in a pilot |
| M4-3 Learned or LLM inference | Future | No learned inference in the controlled baseline | Compare learned inference with the fixed rule baseline |
| M5 User adaptation | Implemented prototype | Continuous current guidance need and target knowledge change selection and presentation | Derive need dynamically from interaction evidence and validate adaptations with users |
| M6 Scene Token representation | Implemented v1 | Formal token specification, generator, selector, packet builder, and logs exist | Freeze the fields required by the evaluation data contract |
| M7 Communication optimization | Partial | Semantic packets, selection, byte metrics, SDK-independent transport contract, and loopback transport exist | Implement the Fusion adapter and measure delay, loss, and traffic over an actual network transport |
| M8 Spatial audio presentation | Partial | Token-driven spatial position, volume, pitch, and alert cues exist | Fix presentation rules and validate them in the target HMD environment |
| M9 Evaluation | Partial | Conditions, response windows, CSV logs, metrics, and analysis scripts exist | Complete a pilot, revise the protocol, and then run the formal study |

## Completed Increment: M4-1

The first closed implementation increment is the deterministic information
selection baseline.

Comparison modes:

1. `FullTransmission`: sends every event as audio and token.
2. `PriorityOnly`: selects only from the event priority threshold.
3. `ContextAndUserState`: always sends critical events, suppresses
   task-irrelevant events, and suppresses repeated target information already
   presented to the receiver.

The policy is demonstrated by the automated S4 object-transfer scenario and is
covered by `SceneTokenAnalyzerSelfCheck`. The checks cover:

- low-, normal-, and high-priority decisions;
- task relevance;
- repeated target suppression;
- critical-event override;
- receiver-state reset between runs.

Validation on 2026-07-21:

```text
[SceneTokenAnalyzerSelfCheck] Passed.
[SceneTokenValidation] Passed.
```

## Completed Increment: M4-2 Prototype

The proposed policy now converts the preassigned event urgency into a logged,
explainable score while preserving M4-1 as the comparison baseline.

Minimum scoring inputs:

```text
urgency + task relevance + novelty + receiver role
```

Required output for every event:

```text
component scores + total score + decision + reason
```

Implemented behavior:

1. The same input produces the same score and decision.
2. Critical events cannot be suppressed by novelty or role.
3. High-need and low-need states produce different routine-information decisions.
4. Scores, component values, current need, threshold, and reasons are written to the log.
5. Boundary cases are covered by the Unity self-check.

Remaining before M4-2 is research-complete:

- add a measured time-criticality component instead of assuming it from priority;
- expose component switches for formal ablation;
- determine or tune weights from a pilot rather than claiming optimal weights.

## Completed Increment: M5 Prototype

The receiver is not assigned a fixed novice or expert category. A continuous
`guidanceNeed` value from `0.0` to `1.0`, together with target-specific known
state, affects both information selection and presentation. The value represents
the person's current need in the current context and may change during a task.

| Current state | Routine information | Message | Visual/audio behavior |
| --- | --- | --- | --- |
| Higher guidance need | Retains new task-relevant procedure | Action + target + direction + explicit guidance | Larger and longer cue, stronger critical alert |
| Lower guidance need | Suppresses routine progress and keeps exceptions/outcomes | Compact action + target | Smaller and shorter cue |

Every presentation records its mode, message, cue scale, cue duration, and audio
gain. The Unity self-check verifies that high-need guidance contains direction and
is larger and longer than the low-need alert for the same critical event.

The current `0.8` and `0.2` settings are reproducible comparison presets, not
user classes. The next step is to update guidance need from observable evidence,
such as repeated errors, hesitation, successful repetitions, explicit help requests,
and current workload.

## Minimal Demo Baseline

Further work must preserve a small runnable baseline before adding new inputs.
The baseline uses one draggable object and a five-event red-then-green path:

1. new task-relevant step;
2. repeated information about the same target;
3. unrelated background update;
4. critical wrong-placement warning;
5. critical correction result.

Expected selected-event counts are `5`, `4`, `3`, and `2` for Full,
Priority-only, proposed need `0.8`, and proposed need `0.2`, respectively.
The first three events are fixed; wrong placement and correction are generated by
the user's mouse releases. New behavior should first be demonstrated in this scenario
and only then be added to a larger VR collaboration task.

The visible adaptation baseline runs directly in Unity. It uses scene objects,
spatial cues, guidance text, and alert sound rather than a pre-rendered video.
The Windows build has one fixed entry scene and a reproducible editor build command.

The same task also has a minimal Meta Quest 3 path using OpenXR, Oculus Touch
Controller Profile, XR Direct Interactors, and `XRGrabInteractable`. Desktop mouse
and Quest grip release share the same placement and adaptation logic.

## Automatic Need Update Baseline

The minimal user-state loop is implemented without a fixed user category:

```text
help request  -> guidanceNeed +0.25
observed error -> guidanceNeed +0.20
task success  -> guidanceNeed -0.15
```

Values are clamped to `0.0` through `1.0`. Updates do not erase target-specific
known state. Each update logs its event type, reason, value before/after, and
cumulative help/error/success counts to `adaptation_events_*.csv`.

These deltas are deterministic prototype parameters, not validated optimal values.

## Pilot Run Summary

Every completed minimal scenario writes one `pilot_runs_*.csv` row with:

- comparison mode;
- initial and final guidance need;
- run duration;
- total, selected, and suppressed event counts;
- transmitted packet bytes;
- help, error, and success events during the run.

This is the minimum objective data contract for the first visual pilot. It does not
replace participant feedback or task-comprehension measures.
