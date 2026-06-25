# Next Steps

This document lists the next tasks in priority order.

## Priority 1: Confirm the Current Prototype

Goal:

```text
Obtain one complete logged run across all five conditions.
```

Tasks:

1. Open `Assets/Scenes/SceneTokenMock.unity`.
2. Press Play.
3. Start an experiment session with `Space`.
4. Run all five conditions:
   - `TRADITIONAL`
   - `DIRECTION_ONLY`
   - `DIRECTION_DISTANCE`
   - `DIRECTION_DISTANCE_SPEAKING`
   - `FULL_SCENE_TOKEN`
5. Confirm that token, event, and metrics CSV files are generated.
6. Run:

```bash
python Tools/analyze_scene_token_logs.py <metrics_csv_or_log_directory>
python Tools/analyze_token_logs.py <scene_tokens_csv_or_log_directory>
python Tools/analyze_event_logs.py <scene_token_events_csv_or_log_directory>
python Tools/summarize_experiment_run.py <log_directory> summary.md
```

7. Save one representative summary for the next research note.

Deliverable:

- one valid token log
- one valid event log
- one valid metrics log
- one valid response/event summary
- one Markdown experiment summary
- one short summary of what each condition produced

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

Tasks:

1. Extend `Tools/analyze_token_logs.py` to export a summary CSV file.
2. Extend `Tools/analyze_event_logs.py` to export a response summary CSV file.
3. Add checks for whether all scripted semantic labels appeared.
4. Add per-speaker summaries.
5. Add a short result paragraph output for weekly reports.

Deliverable:

- token-level analysis output that can be copied into a weekly report

## Priority 6: Improve Engineering Quality

Tasks:

1. Add Unity EditMode tests for direction quantization.
2. Add Unity EditMode tests for distance quantization.
3. Add tests for CSV escaping.
4. Add tests for metrics byte estimates.
5. Add a scene validation editor script if the scene wiring changes often.

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
今週は，Scene Tokenを用いた意味的空間音声コミュニケーション手法の研究定義を整理し，GitHub上に研究概要，Scene Token仕様，関連研究との比較，想定質疑応答をまとめた。

実装面では，3人の話者を対象としたUnityデモにおいて，方向，距離，発話状態，会話役割，意味ラベルをScene Tokenとして生成し，5条件で比較できる構成を整理した。

今後は，5条件すべてでログを取得し，Scene Tokenの追加によって話者把握や会話理解にどのような差が出るかを評価するための実験設計を進める。
```
