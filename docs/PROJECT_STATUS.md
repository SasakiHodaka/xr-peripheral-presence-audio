# Project Status

Last organized: 2026-06-25

## Repository

- GitHub repository: `SasakiHodaka/xr-peripheral-presence-audio`
- Current research direction: semantic spatial voice communication using Scene Tokens
- Previous peripheral-presence-audio direction has been replaced by the Scene Token VR prototype.

## Research Status

Current working title:

```text
VR空間におけるScene Tokenを用いた意味的空間音声コミュニケーション手法の提案
```

Current main claim:

```text
Scene Token integrates spatial information and conversation-state information
into a discrete representation for VR spatial voice communication. Compared
with spatial metadata alone, it can support not only sound reproduction but also
conversation understanding.
```

Current research focus:

- define Scene Token clearly
- position the work against IVAS, MASA, object-based audio, turn taking, and semantic communication
- evaluate whether adding conversation-state information improves VR conversation understanding

Current risk:

- semantic labels are manual/scripted, not automatically inferred yet
- communication-volume reduction is not yet proven
- user-study evidence has not been collected yet

## Unity Version

- Target editor: Unity 2022.3.62f3
- Render pipeline: Built-in 3D
- Audio package: Meta XR Audio SDK package is listed in `Packages/manifest.json`.

## Current Demo Scene

Primary scene:

```text
Assets/Scenes/SceneTokenMock.unity
```

The scene contains:

- listener camera
- three mock speaker avatars
- `SceneTokenSystem`
- token logger
- event logger
- metrics logger
- decoder renderer
- condition controller
- experiment session controller
- scripted conversation controller
- debug HUD

## Implemented Features

The current prototype supports:

- three avatar speakers
- speaker ID management
- 8-direction tokenization
- 3-level distance tokenization
- speaking-state tokenization
- simple turn-state tokenization
- manual semantic labels
- scripted semantic labels
- deterministic scripted conversation
- token-based AudioSource position reconstruction
- token-based volume modulation
- token-based pitch modulation
- token CSV logging
- event CSV logging
- participant response logging and correctness scoring for direction and speaker guesses
- response latency logging for direction and speaker guesses
- communication-volume metric CSV logging
- token and metric logging gated by active experiment session
- five rendering conditions for comparison
- token-level log analysis with `Tools/analyze_token_logs.py`
- event and response analysis with `Tools/analyze_event_logs.py`
- editor scene validation with `SceneTokenSceneValidator`

## Current Scene Token Fields

The implementation currently logs:

```text
timestamp,
sessionId,
participantId,
trialIndex,
trialElapsed,
speakerId,
azimuth,
range,
direction,
distance,
speakingState,
turnState,
semanticToken,
utteranceText,
semanticConfidence,
condition
```

## Validation Status

Confirmed on 2026-06-25:

- Unity Editor normal launch succeeded.
- Project import completed.
- Script compilation completed.
- No `error CS` or `CompilerError` entries were found in `Editor.log`.
- Unity automatically enabled:
  - Spatializer Plugin: `Meta XR Audio`
  - Ambisonic Decoder Plugin: `Meta XR Audio`

Known validation limitation:

- Unity batchmode validation returned code `199` because the Unity Licensing
  Client IPC channel timed out.
- This was a licensing service IPC issue in batchmode, not a script compilation
  error.
- Normal Unity Editor launch compiled the project successfully.

## Current Limitations

Research limitations:

- No user-study results yet.
- Semantic labels are controlled manually or by script.
- Automatic ASR/LLM semantic-token generation is future work.
- Bandwidth reduction is only estimated by metrics, not yet validated as a main contribution.

Implementation limitations:

- The prototype is a local Unity mock scene, not a networked multi-user VR system.
- It does not implement IVAS, MASA, or a real codec bitstream.
- Voice quality depends on assigned clips; fallback tones are only for testing.
- The current semantic modulation is simple volume/pitch control.
- Existing scenes created before the latest wizard update may rely on runtime
  AudioListener auto-addition unless regenerated.

## Documents to Read

Research:

- `docs/RESEARCH_OVERVIEW.md`
- `docs/SCENE_TOKEN_SPEC.md`
- `docs/RELATED_WORK_QA.md`

Prototype:

- `docs/ARCHITECTURE.md`
- `docs/EXPERIMENT_PROTOCOL.md`
- `docs/NEXT_STEPS.md`
