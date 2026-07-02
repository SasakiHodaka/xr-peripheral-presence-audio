# Next Steps

This document lists the next tasks in priority order.

## Priority 1: Confirm the Current Prototype

Goal:

```text
Obtain one complete logged run across all five conditions.
```

Tasks:

1. Open `Assets/Scenes/SceneTokenMock.unity`.
2. Run `Tools > Semantic Spatial Audio > Run Scene Token Analyzer Self Check`.
3. Run `Tools > Semantic Spatial Audio > Validate Scene Token Mock Scene`.
4. Press Play.
5. Start an experiment session with `Space`.
6. Run all five conditions:
   - `TRADITIONAL`
   - `DIRECTION_ONLY`
   - `DIRECTION_DISTANCE`
   - `DIRECTION_DISTANCE_SPEAKING`
   - `FULL_SCENE_TOKEN`
7. During each condition, enter at least one direction response and one speaker
   response:
   - arrow keys for `FRONT`, `RIGHT`, `BACK`, `LEFT`
   - HUD buttons for all eight directions
   - `J`, `K`, `L` for speaker `A`, `B`, `C`
8. Confirm that token, event, and metrics CSV files are generated.
9. Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
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
- one short summary of what each condition produced

Current status:

- token, semantic, turn-state, overlap, and metrics logging are working
- response logging has been implemented in the HUD and keyboard controls
- the next Unity run should confirm that `response_direction` and
  `response_speaker` appear in the event log

## Priority 2: Strengthen the Research Definition

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

## Priority 3: Clean Up the Demo Scenario

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

## Priority 4: Prepare Evaluation Metrics

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

## Priority 5: Extend Analysis Tools

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

Next validation:

1. Run the tools against one real Unity log directory.
2. Confirm that `QUESTION`, `ANSWER`, `INSTRUCTION`, `WARNING`, and `AGREEMENT`
   appear in the token log.
3. Copy the generated weekly-report draft into the research note and edit the
   result paragraph using the actual numbers.

Deliverable:

- token-level analysis output that can be copied into a weekly report

## Priority 6: Improve Engineering Quality

Tasks:

1. Run the editor analyzer self check for direction, distance, speaking, and turn-state parsing.
2. Add tests for CSV escaping.
3. Add tests for metrics byte estimates.
4. Expand scene validation if the scene wiring changes often.

Deliverable:

- repeatable validation that token generation is stable

## Priority 7: Future Research Extensions

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
