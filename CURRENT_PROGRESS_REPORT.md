# Current Progress Report

更新日: 2026-06-04

## 今週の目的

今週は，XR空間における周辺人物への気づきを支援する気配音システムについて，被験者実験に進むためのUnity実験基盤と学習パイプラインを整備した．

実装の中心は，便利機能そのものではなく，以下の研究フローをUnity上で実行できるようにすることである．

```text
Unityで状況パターンを生成する
-> 各状況に複数の気配音候補を提示する
-> 被験者実験で認知性能を測る
-> 最も有効な音を正解ラベルにする
-> PresenceScoreと音量補正値を数理モデルで作る
-> そのデータでNNを学習させる
-> 未知状況での推定性能を評価する
```

## 研究全体における位置づけ

本作業は，研究全体のうち以下に該当する．

- 環境・行動情報の取得
- データセット生成
- 学習モデル構築の初期基盤
- 被験者評価の準備

まだ本番の被験者実験結果は入っていない．現在のUnityデモに入っている気配音ルールは，研究者の主観に基づく初期ルールであり，最終的な正解ラベルではない．

## 現在できていること

Unity側では，周辺人物の状態を検出し，状況に応じた気配音候補を出し分け，CSVログとして保存できる．

主な実装済みコンポーネント:

- `PeripheralStateDetector`: 周辺人物の状態を検出する．
- `PeripheralCueModel`: `NoCue`，`FixedCue`，`StateBasedCue`，`LearnedCue`，`EnvironmentAdaptiveCue` を切り替える．
- `PeripheralCueAudioEmitter`: 選択された気配音候補を3D音として再生する．
- `PeripheralCueExperimentController`: 反応，方向回答，主観評価を受け取る．
- `PeripheralCueTrialSequencer`: 状況と気配音候補の組み合わせを順番またはランダムに提示する．
- `PeripheralStateLogger`: 状態，音，反応，方向正誤，主観評価をCSVに保存する．
- `PeripheralDebugUI`: Play Mode中に現在の条件，候補音，反応状態を確認する．

## 入力パラメータ

今回扱う主な入力情報は以下である．

| 項目 | 内容 |
| -- | -- |
| 状況条件 | `Approach`, `BackApproach`, `Crossing`, `Speaking` |
| 気配音候補 | `NoCue`, `Footstep`, `Breathing`, `ClothRustle`, `Voice`, `AmbientPresence`, `MixedCue` |
| 状態特徴量 | 視野外，接近，発話，注視，近距離，横切り |
| 幾何特徴量 | 距離，視野角，ユーザ基準の相対位置 |
| 運動特徴量 | 接近速度，横方向速度 |
| 被験者入力 | 検出反応，方向回答，1から5の主観評価 |

取得方法:

Unity Play Mode上で試行を実行し，`PeripheralStateLogger` がCSVとして保存する．

## 出力情報

現在のCSVには，モデル学習と評価に必要な以下の情報を出力できる．

| 項目 | 内容 |
| -- | -- |
| `cueCandidate` | 実験で提示した音候補 |
| `cueType` | モデルまたはルールが出力した音種別 |
| `presenceScore` | 周辺人物の存在感をどの程度表現するか |
| `volumeGain` | 音量補正値 |
| `reactionTime` | 検出反応時間 |
| `directionResponse` | 被験者の方向回答 |
| `directionCorrect` | 方向回答の正誤 |
| `subjectiveRating` | 主観評価 |

## 学習・解析パイプライン

Python側では，Unityログから学習用データセットを作成し，軽量モデルを学習し，Unityで読めるJSONモデルとして出力できる．

主なコマンド:

```powershell
python Tools/analyze_peripheral_csv.py --cue-effectiveness
python Tools/analyze_peripheral_csv.py --label-dataset
python Tools/build_cue_training_dataset.py --include-none
python Tools/train_cue_model.py --epochs 40
python Tools/summarize_cue_training_dataset.py
```

現在の既存ログを使った初期モデル結果:

| 項目 | 値 |
| -- | -- |
| 使用ログ | 23 files |
| 全データ数 | 16,511 rows |
| 学習データ | 12,383 rows |
| テストデータ | 4,128 rows |
| `cueType` accuracy | 0.7987 |
| `presenceScore` MAE | 0.0566 |
| `volumeGain` MAE | 0.0566 |

注意点:

この結果は，現時点では研究者主観の初期ルールを学習したベースラインである．被験者実験から得られた「人間にとって有効な音」を学習した結果ではない．

## 教師データ生成方法

今後の本来の教師データは，被験者実験の結果から作る．

現在の方針:

| 条件 | 生成ラベル |
| -- | -- |
| 検出成功率が高い | 有効な候補音として加点 |
| 方向正答率が高い | 状況理解に有効として加点 |
| 反応時間が短い | 気づきやすい音として加点 |
| 主観評価が高い | 自然さ・納得感が高い音として加点 |

解析スクリプトでは，暫定的に以下の指標で `cueEffectiveness` を計算する．

```text
detectionSuccess
+ directionAccuracy
- normalizedReactionTime
+ normalizedRating
```

この値が最も高い音候補を，その状況の暫定的な正解ラベル候補にする．

## 現在の限界

現状で詰め切れていない点は以下である．

- 本番の被験者データがまだない．
- 主観評価は現在1から5の単一評価であり，自然さ，不快感，気づきやすさを分けていない．
- `cueEffectiveness` の式は暫定であり，実験目的に合わせて重み調整が必要である．
- Unity上の音源素材と音量条件は，厳密な実験条件としてまだ固定し切れていない．
- 現在の学習結果は，研究者主観ルールの再現性能であり，人間の認知性能を改善した証拠ではない．
- Meta Audio SimulatorやSoundSpacesのような大規模音響シミュレーションは，まだ本実装には統合していない．

## 次に行う作業

次回は，便利機能の追加ではなく，研究結果を出すための実装に進む．

1. 少数条件でミニ実験ログを取る．
   - `BackApproach`
   - `Approach`
   - `Crossing`
   - `Speaking`
   - `NoCue`, `Footstep`, `Breathing`, `AmbientPresence` などを比較する．

2. `cueEffectiveness` を条件別・候補音別に集計する．

3. 各状況で最も有効な音をラベル候補として出力する．

4. 研究者主観ルールと，被験者ログ由来ラベルを比較する．

5. ラベルデータで `LearnedCue` モデルを再学習し，`StateBasedCue` と比較する．

## 次回の実装候補

次に入れるべき実装は，UIの便利機能ではなく，評価結果を直接作る機能である．

優先度が高いもの:

- `Tools/analyze_peripheral_csv.py` に条件別・候補音別ランキングのMarkdownレポート出力を追加する．
- `cueCandidate` ごとの検出率，方向正答率，反応時間，主観評価，`cueEffectiveness` を表にする．
- 各 `conditionLabel` で最良音を `isBestCue=True` として明示する．
- `StateBasedCue` と `LearnedCue` の比較用サマリを出力する．

## Git保存状況

直近の保存済みコミット:

```text
90f136a Add randomized cue trial sequencing
510d530 Add peripheral direction accuracy logging
b805089 Export Unity-compatible cue model
9136b6d Fix audio emitter script guid
313855e Document hybrid cue learning roadmap
```

現在の作業ツリーは，この進捗メモ更新前の時点でクリーンだった．
