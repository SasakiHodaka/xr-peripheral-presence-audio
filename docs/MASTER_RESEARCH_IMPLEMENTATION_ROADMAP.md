# Master Research Implementation Roadmap

研究テーマ:

```text
VR空間におけるScene Tokenを用いた意味的空間音声コミュニケーション手法の提案
```

このロードマップは，実装手順を修士研究として説明できる形に整理したものである．
各Phaseでは，実装内容だけでなく，既存研究との対応と検証項目を明示する．

## Overall System

```text
VR Space
    ↓
Scene Parsing
    ↓
Scene Token
    ↓
Semantic Communication
    ↓
Scene Token
    ↓
Spatial Audio Rendering
    ↓
User
```

本研究では，画像分野における `Scene -> Scene Parsing -> Scene Token`
の考え方を参考にし，VR空間音声へ拡張する．ここでのScene Parsingは，
VR空間内の話者，位置，距離，発話状態，会話役割，意味ラベルを解析し，
Scene Tokenへ変換する処理を指す．

## Phase 1: System Construction

### Step 1: Scene Token Prototype Validation

目的:

```text
Scene Tokenによる基本的な空間音声通信システムを動作させる．
```

実装:

- `scene-token-prototype` ブランチへ切り替える．
- `Assets/Scenes/SceneTokenMock.unity` を起動する．
- 自動会話の再生を確認する．
- 5条件の切り替えを確認する．
- CSV保存を確認する．

確認項目:

- 音声再生
- Token生成
- Token表示
- CSV保存
- 自動会話再生
- 条件切替

検証すること:

```text
研究評価に使える最小構成のUnity実験デモが動作するか．
```

## Phase 2: Scene Parsing and Token Generation

### Step 2: Scene Token Generator

目的:

```text
VR空間の状態をScene Tokenへ変換する．
```

既存研究との対応:

- IVAS: 空間音声通信における音声オブジェクトと空間情報
- MASA: 空間音響メタデータによる再生支援
- Turn Taking: 発話状態，会話役割，発話交替
- Scene Token / Scene Parsing: シーン状態を離散表現へ変換する考え方

入力:

- Avatar ID
- 位置
- 方向
- 距離
- 発話状態
- 会話役割
- 意味ラベル

出力:

```json
{
  "speakerId": "A",
  "direction": "FRONT_RIGHT",
  "distance": "NEAR",
  "speakingState": "SPEAKING",
  "turnState": "TURN_HOLDER",
  "semanticToken": "QUESTION"
}
```

検証すること:

```text
Avatar状態から，方向・距離・発話状態・会話役割・意味ラベルが正しくToken化されるか．
```

主な確認ログ:

- `speakerId`
- `direction`
- `distance`
- `speakingState`
- `turnState`
- `semanticToken`
- `condition`

## Phase 3: Scene Token Communication

### Step 3: Scene Token Communication

目的:

```text
波形や完全な3D座標ではなく，Scene Tokenを通信単位として扱う．
```

既存研究との対応:

- Semantic Communication: 波形そのものではなく意味情報を伝送する考え方
- Object-Based Audio: 音声オブジェクトと位置メタデータを扱う枠組み
- MASA / IVAS: 空間音声の伝送・再生に必要なメタデータ

検証すること:

- Scene Tokenの更新頻度
- JSON風Tokenサイズ
- Compact Tokenサイズ
- Object Metadataとの通信量比較

主な評価項目:

- `tokensPerSecond`
- `jsonBytesPerSecond`
- `compactBytesPerSecond`
- `objectMetadataBytesPerSecond`
- `compactSavingsRatio`

注意:

```text
通信量削減は副次的評価とする．主貢献は会話理解支援である．
```

## Phase 4: Scene Token Renderer

### Step 4: Spatial Audio Renderer

目的:

```text
受信したScene Tokenから空間音声を再構成する．
```

既存研究との対応:

- MASA: 空間メタデータから音場再生を支援する考え方
- Object-Based Audio: 話者を音声オブジェクトとして扱い，位置に応じて再生する考え方
- Turn Taking: 発話中・主導権・重なり発話の状態表現

再構成規則:

| Token | Rendering rule |
| --- | --- |
| `Direction` | 音源方向を決定する |
| `Distance` | 音量，距離減衰を決定する |
| `SpeakingState` | 再生・非再生，発話中強調を決定する |
| `TurnState` | 主発話者，重なり発話者の強調度を決定する |
| `SemanticToken` | 質問，回答，指示，警告などに応じた音量・ピッチ・明瞭度を調整する |

検証すること:

```text
Tokenの違いが，方向，音量，再生状態，意味的強調として反映されるか．
```

## Phase 5: Scenario Design

### Step 5: Conversation Scenarios

目的:

```text
Scene Tokenの効果を評価できる3人会話シナリオを作成する．
```

シナリオ:

| Scenario | Content | Required tokens |
| --- | --- | --- |
| Scenario 1 | 質問 -> 回答 | `QUESTION`, `ANSWER`, `TURN_HOLDER` |
| Scenario 2 | 指示 -> 確認 | `INSTRUCTION`, `ANSWER`, `TURN_HOLDER` |
| Scenario 3 | 警告 -> 重なり発話 | `WARNING`, `OVERLAPPER`, `SPEAKING` |

検証すること:

- 30秒から60秒で繰り返し再生できるか．
- A/B/Cの3話者が出現するか．
- 主要Semantic Tokenがすべて出現するか．
- 短い重なり発話が含まれるか．

## Phase 6: Logging and Analysis

### Step 6: Log Collection and Python Analysis

目的:

```text
実験ログから評価指標を算出できる状態にする．
```

処理:

```text
CSV log
    ↓
Python analysis
    ↓
Evaluation metrics
```

必要なCSV:

- `scene_tokens_*.csv`
- `scene_token_events_*.csv`
- `scene_token_metrics_*.csv`

解析:

```bash
python Tools/analyze_scene_token_logs.py <log_directory>
python Tools/analyze_token_logs.py <log_directory>
python Tools/analyze_event_logs.py <log_directory>
python Tools/summarize_experiment_run.py <log_directory> summary.md
```

評価項目:

- 話者認識率
- 方向認識率
- 反応時間
- 会話理解度
- NASA-TLX
- 通信量

検証すること:

```text
condition / speaker / direction / response / latency が正しく記録され，
条件別の正答率，反応時間，通信量を集計できるか．
```

## Phase 7: Comparative Experiment

### Step 7: Condition Comparison

目的:

```text
Scene Tokenに含める情報が増えることで，話者把握と会話理解が改善するかを比較する．
```

最小比較条件:

| Condition | Meaning |
| --- | --- |
| Condition 1 | 通常空間音声 |
| Condition 2 | `Direction` |
| Condition 3 | `Direction + Distance` |
| Condition 4 | `Scene Token` |

現在の実装上の5条件:

| Condition | Implementation |
| --- | --- |
| 1 | `TRADITIONAL` |
| 2 | `DIRECTION_ONLY` |
| 3 | `DIRECTION_DISTANCE` |
| 4 | `DIRECTION_DISTANCE_SPEAKING` |
| 5 | `FULL_SCENE_TOKEN` |

検証すること:

- Directionだけで方向認識が改善するか．
- Distanceを加えると距離感や話者把握が改善するか．
- SpeakingStateを加えると発話者認識が改善するか．
- Full Scene Tokenで会話理解が改善するか．

## Phase 8: Discussion

### Step 8: Research Discussion

目的:

```text
Scene TokenがVR空間音声コミュニケーションに与える効果と限界を考察する．
```

論文で示すこと:

1. Scene Tokenによって話者を見つけやすくなったか．
2. Scene Tokenによって会話を理解しやすくなったか．
3. Scene Tokenによって通信量は削減できたか．
4. Scene Tokenによって認知負荷は増えなかったか．

主張の整理:

```text
Scene Tokenは，空間情報と会話状態を統合した離散表現であり，
従来の空間音声メタデータでは扱いにくい会話理解支援を可能にする．
```

限界:

- Semantic Tokenは現段階では手動またはスクリプトで付与する．
- ASRやLLMによる自動意味推定は将来課題とする．
- 通信量削減は副次的分析であり，主貢献は会話理解支援である．

## Immediate Implementation Order

次に実装・確認する順番:

1. `SceneTokenMock.unity` を起動し，5条件の動作を確認する．
2. 5条件を通した1回分のCSVログを取得する．
3. Python解析で正答率，反応時間，通信量が出るか確認する．
4. 自然な会話音声へ差し替える．
5. `FULL_SCENE_TOKEN` の音量・ピッチ・強調規則を整理する．
6. 予備実験用の3シナリオを作成する．
7. 比較実験用の評価シートとNASA-TLXを準備する．
