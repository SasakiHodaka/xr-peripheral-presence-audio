# Next Steps

This document lists the next tasks in priority order.

## Priority 1: Confirm the Current Prototype

Goal:

```text
Obtain one complete logged run across the three main conditions.
```

Status:

- Completed for prototype validation on 2026-07-06.
- Representative run: `Runs/run_20260706_230957`.
- The run includes direction and speaker responses for:
  - `C1_TRADITIONAL` in current naming, `TRADITIONAL` in the representative log
  - `C2_DIRECTION_DISTANCE` in current naming, `DIRECTION_DISTANCE` in the representative log
  - `C3_FULL_SCENE_TOKEN` in current naming, `FULL_SCENE_TOKEN` in the representative log

Tasks:

1. Open `Assets/Scenes/SceneTokenMock.unity`.
2. Run `Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check`.
3. Run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.
4. Press Play.
5. Start an experiment session with `Space`.
6. Run all three main conditions:
   - `C1_TRADITIONAL`
   - `C2_DIRECTION_DISTANCE`
   - `C3_FULL_SCENE_TOKEN`
7. During each condition, enter at least one direction response and one speaker
   response:
   - arrow keys for `FRONT`, `RIGHT`, `BACK`, `LEFT`
   - HUD buttons for all eight directions
   - `J`, `K`, `L` for speaker `A`, `B`, `C`
8. Confirm that token, event, and metrics CSV files are generated.
9. Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
python Tools/analyze_scene_packet_logs.py <scene_packets_csv_or_log_directory>
python Tools/analyze_token_logs.py <scene_tokens_csv_or_log_directory> token_summary.csv
python Tools/analyze_event_logs.py <scene_token_events_csv_or_log_directory> event_summary.csv
python Tools/summarize_experiment_run.py <log_directory> summary.md
```

10. Save one representative summary for the next research note.

Deliverable:

- one valid token log
- one valid event log
- one valid metrics log
- one valid response/event summary
- one Markdown experiment summary
- one short summary of what each main condition produced

Current status:

- token, semantic, turn-state, overlap, and metrics logging are working
- response logging has been implemented in the HUD and keyboard controls
- `response_direction` and `response_speaker` now appear in the event log for
  all three main conditions
- the run still contains ambiguous responses, so the formal pilot should make
  response timing clearer

## Priority 2: Reduce Ambiguous Responses Before Pilot

Goal:

```text
Make participant response timing clear enough for a small pilot.
```

Tasks:

1. Add or refine a HUD prompt that shows when a response should be made.
   - Implemented: `RESPOND NOW` / `WAIT` cue in the debug HUD.
2. Consider logging a `response_window_start` event when the scripted
   conversation has a clear active speaker.
   - Implemented: `response_window_start` and `response_window_end` events.
3. Keep ambiguous responses in the log, but make the pilot instruction avoid
   no-speaker and overlap moments unless explicitly tested.

Deliverable:

- one pilot-ready response workflow with fewer ambiguous responses

Next validation:

- Run one short session and confirm that the event log includes
  `response_window_start` and `response_window_end`.
- Confirm that responding only while `RESPOND NOW` is visible reduces
  `ambiguousResponses`.

## Priority 3: Strengthen the Research Definition

Goal:

```text
Make Scene Token defensible against "Is this just metadata?" questions.
```

Tasks:

1. Keep `docs/SCENE_TOKEN_SPEC.md` as the formal definition.
2. Explain that MASA/IVAS are rendering-oriented, while Scene Token is
   conversation-understanding-oriented.
3. Avoid claiming proven bandwidth reduction as the main contribution.
4. Keep the main claim focused on conversation understanding.
5. Prepare a one-slide version of:

```text
Speech + Position -> Spatial Audio
Speech + Position + Conversation State -> Scene Token
```

Deliverable:

- one slide or diagram showing the research gap and proposed pipeline

## Priority 4: Fix Evaluation Data Specification Before Scenario Runs

Goal:

```text
Lock the data contract before collecting pilot data.
```

Status:

- Representative metrics and RQ mapping are defined in
  `docs/EVALUATION_HYPOTHESES.md` and `docs/EVALUATION_DATA_SPEC.md`.

Tasks:

1. Define Scenario A/B/C as utterance-level ground-truth rows.
2. Confirm every scored response has an `utteranceId` or response-window target.
3. Confirm token, event, metrics, and packet logs contain every field required
   for scoring.
4. Add missing Unity log fields before running a participant pilot.
5. Treat the current design phase as closed after Scenario A/B/C and the
   required CSV fields are fixed.

Deliverable:

- one fixed scenario and evaluation CSV specification

## Priority 5: Clean Up the Demo Scenario

Goal:

```text
Make the scripted three-person conversation suitable for explanation and evaluation.
```

Tasks:

1. Tune the scripted conversation sequence.
2. Make sure it includes:
   - question
   - answer
   - instruction
   - warning
   - short overlap
3. Replace fallback tones with clearer voice clips when possible.
4. Keep semantic labels scripted/manual until the baseline is stable.

Deliverable:

- one repeatable 30-second to 60-second three-speaker scenario

## Priority 6: Prepare Evaluation Metrics

Goal:

```text
Decide exactly what will be measured before running participants.
```

Candidate objective metrics:

- speaker localization accuracy
- speaker identification time
- active speaker recognition accuracy
- turn/overlap recognition accuracy
- conversation comprehension score

Candidate subjective metrics:

- ease of identifying the speaker
- ease of following the conversation
- naturalness
- workload, such as NASA-TLX
- usefulness of semantic emphasis

Deliverable:

- experiment sheet or questionnaire draft

## Priority 7: Extend Analysis Tools

Goal:

```text
Analyze token-level behavior beyond the current summary script.
```

Implemented:

1. `Tools/analyze_token_logs.py` can export a condition summary CSV.
2. `Tools/analyze_token_logs.py` also writes a condition-by-speaker CSV.
3. `Tools/analyze_token_logs.py` checks whether all scripted semantic labels appeared.
4. `Tools/analyze_event_logs.py` can export a response summary CSV.
5. `Tools/summarize_experiment_run.py` includes quality checks, speaker summaries,
   communication metrics, and a weekly-report draft.
6. `Tools/analyze_scene_packet_logs.py` can summarize packet count, bytes,
   token selection, and important-token retention by condition.

Next validation:

1. Run the tools against one real Unity log directory.
2. Confirm that `QUESTION`, `ANSWER`, `INSTRUCTION`, `WARNING`, and `AGREEMENT`
   appear in the token log.
3. Confirm that `scene_packets_*.csv` is copied into `Runs/latest_run` and
   appears in the generated `Scene Packet Metrics` summary.
4. Copy the generated weekly-report draft into the research note and edit the
   result paragraph using the actual numbers.

Deliverable:

- token-level analysis output that can be copied into a weekly report

## Priority 8: Improve Engineering Quality

Tasks:

1. Run the editor analyzer self check for direction, distance, speaking, and turn-state parsing.
2. Add tests for CSV escaping.
   - Implemented in `SceneTokenAnalyzerSelfCheck`.
3. Add tests for metrics byte estimates.
   - Implemented in `SceneTokenAnalyzerSelfCheck`.
4. Expand scene validation if the scene wiring changes often.

Deliverable:

- repeatable validation that token generation is stable

## Priority 9: Future Research Extensions

Do only after the controlled baseline is stable:

- ASR-based transcription
- LLM-based semantic-token classification
- addressee estimation
- gaze/gesture integration
- networked multi-user VR version
- compact binary Scene Token transmission
- neural audio token or generative audio integration

## Recommended Immediate Weekly Report

Use this structure:

```text
今週は、Scene Tokenを用いた意味的空間音声コミュニケーション手法の研究定義を整理し、GitHub上に研究概要、Scene Token仕様、関連研究との比較、想定質疑応答をまとめた。

実装面では、3人の話者を対象としたUnityデモにおいて、方向、距離、発話状態、会話役割、意味ラベルをScene Tokenとして生成し、5条件で比較できる構成を整理した。また、Tokenログ、Eventログ、Metricsログを集計するPython解析ツールを整備し、条件ごとの発話状態、意味ラベル、回答正答率、反応時間、通信量指標を確認できるようにした。

今後は、Unity Editor上で5条件すべての実ログを取得し、Scene Tokenの追加情報によって話者把握、方向把握、会話理解にどのような差が出るかを評価するための実験設計を進める。
```
