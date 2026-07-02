# Evaluation Hypotheses

この文書では，実装後に何を検証するのかを先に固定する．
目的は，Scene Tokenの効果を「話者把握」「会話理解」「通信量」「負荷」
の観点から評価できるようにすることである．

## Research Question

```text
VR空間の複数人会話において，空間情報と会話状態を統合したScene Tokenを用いることで，
従来の空間音声提示よりも話者把握と会話理解を支援できるか．
```

## Evaluation Conditions

実装では5条件を用いる．論文上では，段階的に情報を追加する比較として説明する．

| Condition | Implementation name | Included information | Purpose |
| --- | --- | --- | --- |
| C1 | `TRADITIONAL` | original object position | 通常空間音声の基準条件 |
| C2 | `DIRECTION_ONLY` | direction token | 方向Tokenの効果を確認する |
| C3 | `DIRECTION_DISTANCE` | direction + distance tokens | 距離Tokenの追加効果を確認する |
| C4 | `DIRECTION_DISTANCE_SPEAKING` | direction + distance + speaking state | 発話状態Tokenの効果を確認する |
| C5 | `FULL_SCENE_TOKEN` | direction + distance + speaking + turn + semantic token | Scene Token全体の効果を確認する |

## Hypothesis 1: Speaker Localization

仮説:

```text
DirectionおよびDistance Tokenを用いることで，話者の方向認識率が向上し，
回答までの反応時間が短くなる．
```

比較:

- `TRADITIONAL`
- `DIRECTION_ONLY`
- `DIRECTION_DISTANCE`

評価指標:

- direction response accuracy
- direction response latency
- per-condition error pattern

ログ項目:

- `condition`
- `direction`
- `response_direction`
- `isCorrect`
- `responseLatency`

## Hypothesis 2: Active Speaker Identification

仮説:

```text
SpeakingState Tokenを加えることで，現在発話している話者を識別しやすくなる．
```

比較:

- `DIRECTION_DISTANCE`
- `DIRECTION_DISTANCE_SPEAKING`
- `FULL_SCENE_TOKEN`

評価指標:

- speaker response accuracy
- speaker response latency
- active-speaker recognition accuracy

ログ項目:

- `speakerId`
- `speakingState`
- `response_speaker`
- `isCorrect`
- `responseLatency`

## Hypothesis 3: Conversation Understanding

仮説:

```text
TurnStateおよびSemanticTokenを加えたFULL_SCENE_TOKEN条件では，
空間情報のみの条件よりも会話の流れを理解しやすくなる．
```

比較:

- `DIRECTION_DISTANCE_SPEAKING`
- `FULL_SCENE_TOKEN`

評価指標:

- conversation comprehension score
- turn/overlap recognition accuracy
- subjective ease of following the conversation
- usefulness of semantic emphasis

想定質問例:

- 誰が質問したか．
- 誰が回答したか．
- 警告発話はどの方向から聞こえたか．
- 発話が重なった場面を認識できたか．

## Hypothesis 4: Communication Metadata Volume

仮説:

```text
Scene Tokenは，連続的な位置メタデータよりもコンパクトな離散表現として扱える可能性がある．
```

位置付け:

```text
通信量削減は副次的評価であり，主貢献は会話理解支援である．
```

評価指標:

- tokens per second
- JSON-like bytes per second
- compact token bytes per second
- object metadata bytes per second
- compact savings ratio

ログ項目:

- `tokensPerSecond`
- `jsonBytesPerSecond`
- `compactBytesPerSecond`
- `objectMetadataBytesPerSecond`
- `compactSavingsRatio`

## Hypothesis 5: Workload

仮説:

```text
Scene Tokenによる意味的強調は，会話理解を支援しつつ，認知負荷を大きく増加させない．
```

評価指標:

- NASA-TLX
- naturalness rating
- ease of identifying speaker
- ease of following conversation
- discomfort or annoyance rating

注意:

```text
FULL_SCENE_TOKENの音量・ピッチ強調が強すぎると不自然さや負荷が増えるため，
v1.0では軽い音量・ピッチ調整に留める．
```

## Minimum Valid Evaluation Output

最初の予備実験で最低限必要な出力:

- 5条件すべてのCSVログ
- 条件別の方向正答率
- 条件別の話者正答率
- 条件別の平均反応時間
- 条件別の通信量推定
- 会話理解アンケート結果
- NASA-TLXまたは簡易負荷評価

## Discussion Targets

論文の考察では，以下を示す．

1. Scene Tokenで話者を見つけやすくなったか．
2. Scene Tokenで会話を理解しやすくなったか．
3. Scene Tokenで通信量は減ったか，または構造化できたか．
4. Scene Tokenで認知負荷は増えなかったか．
5. どのToken要素が有効で，どの要素に改善余地があるか．
