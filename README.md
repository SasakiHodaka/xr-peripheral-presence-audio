# Semantic Spatial Audio / Scene Token VR

This repository contains a Unity research prototype for semantic spatial voice
communication in VR.

研究テーマ:

```text
VR空間におけるScene Tokenを用いた意味的空間音声コミュニケーション手法の提案
```

## Research Summary

Existing immersive voice communication technologies such as IVAS, MASA, and
object-based audio mainly focus on how to transmit and render spatial audio:

```text
Speech + Position / Direction -> Spatial Audio Rendering
```

This project extends that idea by adding conversation-state information:

```text
Speech Object + Position + Conversation State -> Scene Token -> Spatial Audio Rendering
```

The goal is not only to reproduce where a voice is heard from, but also to help
the listener understand:

- who is speaking
- where the speaker is located
- whether the speaker is currently holding the conversational turn
- what kind of utterance is being made, such as a question, answer, instruction, or warning

In short:

```text
MASA / IVAS: How should the sound be reproduced?
Scene Token: What is happening in the communication scene?
```

## Current Prototype

The current Unity mock scene implements a three-speaker VR conversation demo.

It supports:

- three avatar speakers
- 8-direction scene tokens
- 3-level distance tokens
- speaking-state tokens
- simple turn-state tokens
- manual or scripted semantic labels
- token-based AudioSource position, volume, and pitch reconstruction
- deterministic scripted conversation playback
- CSV token logging
- event logging for experiment sessions
- participant response logging and correctness scoring for direction and speaker guesses
- response latency logging for objective identification-time analysis
- communication-volume metrics

Current token example:

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER",
  "semanticToken": "QUESTION",
  "timestamp": 12.50
}
```

## Why Scene Token?

Spatial audio alone can tell users where a sound is coming from. However, in a
multi-speaker VR meeting, users may still have difficulty understanding who is
speaking, who is responding, whether speakers are overlapping, or whether an
utterance is a warning or instruction.

Scene Token is proposed as a discrete representation that integrates:

- spatial information: direction and distance
- conversation information: speaking state and turn state
- semantic information: utterance labels such as question, answer, instruction, and warning

This makes the research target semantic spatial audio communication rather than
only spatial audio reproduction.

## Related Work Position

This repository is organized around the following research gap:

| Area | Main contribution | Limitation for this project |
| --- | --- | --- |
| IVAS | Immersive voice/audio coding standard | Focuses on codec and spatial audio transmission |
| MASA | Metadata-assisted spatial audio | Represents spatial rendering metadata, not conversation meaning |
| Object-Based Audio | Treats each speaker as an audio object | Preserves speaker object and position, but not turn role or utterance meaning |
| Turn Taking | Models speaker/listener roles | Usually does not integrate spatial audio rendering |
| Semantic Communication | Transmits meaning rather than raw signal | Usually not specialized for VR spatial voice scenes |
| Scene Token | Integrates spatial state and conversation state | Proposed and evaluated in this project |

## Evaluation Conditions

The prototype compares five rendering conditions:

1. `TRADITIONAL`
   - Uses original object positions.
2. `DIRECTION_ONLY`
   - Uses quantized direction only.
3. `DIRECTION_DISTANCE`
   - Uses quantized direction and distance.
4. `DIRECTION_DISTANCE_SPEAKING`
   - Adds speaking-state gating.
5. `FULL_SCENE_TOKEN`
   - Adds turn-state and semantic-token modulation.

Planned evaluation metrics:

- speaker localization accuracy
- speaker identification time
- conversation understanding
- overlap/turn tracking accuracy
- NASA-TLX or other workload measures
- communication-volume comparison between object metadata and compact scene tokens

## Quick Start

1. Open this repository with Unity 2022.3.62f3 or another Unity 2022.3 LTS editor.
2. Open `Assets/Scenes/SceneTokenMock.unity`.
3. Press Play.
4. Use `A`, `B`, or `C` to toggle each avatar's speaking state.
5. Use `Q`, `W`, or `E` to cycle each avatar's semantic token.
6. Use `1`-`5` to switch evaluation conditions.
7. Use `Space` to start or stop an experiment session.
8. Use `N` to advance to the next condition, or wait for the timer.
9. Use `T` to start or stop the scripted conversation sequence.
10. Use `Y` to stop the scripted conversation sequence.
11. Use numpad `7/8/9/4/6/1/2/3` for direction responses.
12. Use `F1/F2/F3` for speaker responses.

## Documentation

Start here:

- `docs/README.md`: documentation index and recommended reading order

Research documents:

- `docs/RESEARCH_OVERVIEW.md`: research background, problem, purpose, novelty, and evaluation plan
- `docs/SCENE_TOKEN_SPEC.md`: formal Scene Token definition, fields, generation, and rendering rules
- `docs/RELATED_WORK_QA.md`: related-work comparison and expected Q&A for presentations

Implementation documents:

- `docs/PROJECT_STATUS.md`: current state, validation result, and known issues
- `docs/EXPERIMENT_PROTOCOL.md`: how to run a trial and collect logs
- `docs/ARCHITECTURE.md`: script responsibilities and data flow
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
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
python Tools/analyze_token_logs.py <scene_tokens_csv_or_log_directory>
python Tools/analyze_event_logs.py <scene_token_events_csv_or_log_directory>
python Tools/summarize_experiment_run.py <log_directory> [summary.md]
```

The metrics script summarizes:

- tokens per second
- JSON-like bytes per second
- compact scene token bytes per second
- object metadata bytes per second
- compact savings ratio

The token script summarizes:

- condition-level token rows
- session, participant, and trial IDs
- speaking and semantic-token ratios
- turn-holder and overlap rows
- observed speakers, directions, distances, turns, and semantic labels
- basic data quality checks

The event script summarizes:

- session/trial events
- direction response counts
- speaker response counts
- direction and speaker accuracy when a non-ambiguous target exists
- average direction and speaker response latency
- response-log quality checks

The summary script generates a Markdown report containing:

- data quality checks
- token summary
- response accuracy and latency summary
- communication metrics
- a short weekly-report draft

## Research Claim to Keep Clear

The current main claim is:

```text
Scene Token integrates spatial information and conversation-state information
into a discrete representation for VR spatial voice communication. Compared
with spatial metadata alone, it can support not only sound reproduction but also
conversation understanding.
```

Communication-volume reduction is treated as a secondary analysis until enough
experimental evidence is collected.
