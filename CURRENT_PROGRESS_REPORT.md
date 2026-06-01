# Current Progress Report

## 研究全体の方向性

本研究の大きな目的は、新時代のインターフェースを作ることである。

具体的には、人間・環境・エージェントの状態をマルチモーダルに理解し、状況に応じて音・視覚・触覚などの提示方法を変える適応的インターフェースを目指している。

現在の第一段階では、XR空間における周辺人物への気づきを対象とし、視覚UIだけに頼らず、空間音響cueによって人の存在・接近・発話・移動を自然に伝える仕組みを実装している。

研究の発展方向は以下である。

```text
XR周辺人物への適応的音響cue
-> AUI学習によるcue-control
-> マルチモーダル状況推定
-> 人間中心マルチモーダルCPSインターフェース
```

## 現在の研究課題

現在の中心課題は以下である。

```text
XR空間で視野外・背後・周辺にいる人物に対して、
どのような音響cueをどの強度・方向・音響特性で提示すれば、
気づきやすく、かつ自然で邪魔にならないインターフェースになるか。
```

このために、周辺人物の状態を検出し、状況に応じてcueを変えるAUIの基礎実装を進めている。

## 実装済みのUnityシステム

現在のUnityプロトタイプでは、以下のコンポーネントを実装・整理した。

- `PeripheralStateDetector`: 周辺人物の状態を検出する。
- `PeripheralCueModel`: 検出結果からcue種別とcue制御パラメータを予測する。
- `PeripheralCueAudioEmitter`: cueを3D空間音響として再生する。
- `EnvironmentAcousticProfile`: 環境音響プロファイルを保持する。
- `PeripheralStateLogger`: 検出状態、cue予測、再生状態、環境プロファイルをCSVに記録する。
- `PeripheralTrialController`: 試行時間とpre-trial時間を管理する。
- `PeripheralTrialConditionController`: Approach、BackApproach、Crossing、Speakingなどの条件を切り替える。
- `PeripheralAuiLogCollectionController`: AUI学習用に、target scenario、cue condition、environment presetを自動で組み合わせる。
- `PeripheralDebugUI`: Play Mode中に状態、cue、AUI trial番号を確認する。

現在のcue条件は以下である。

```text
NoCue
FixedCue
StateBasedCue
EnvironmentAdaptiveCue
```

現在のtarget scenarioは以下である。

```text
Approach
BackApproach
Crossing
Speaking
```

環境プリセットは以下である。

```text
Neutral
Reverberant
Occluded
```

これにより、AUI学習用に以下の組み合わせを収集できる状態になった。

```text
target scenario
× cue condition
× environment preset
```

## ログ出力の拡張

CSVログには、周辺人物の状態だけでなく、cue制御と環境音響プロファイルも記録するようにした。

主な入力特徴量:

- `outOfView`
- `approaching`
- `speaking`
- `gazing`
- `near`
- `crossing`
- `distance`
- `viewAngle`
- `radialSpeed`
- `lateralSpeed`
- `localX`, `localY`, `localZ`

cue条件:

- `cueCondition`

環境特徴量:

- `roomScale`
- `materialClass`
- `environmentReverbAmount`
- `environmentOcclusionStrength`
- `environmentDistanceAttenuation`
- `environmentRt60`
- `environmentDrr`

教師ラベル・出力:

- `cueType`
- `presenceScore`
- `volumeGain`
- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

実再生状態:

- `playbackActive`
- `playbackVolume`
- `playbackLowPassHz`
- `playbackReverbAmount`
- `footstepInterval`

## AUI学習パイプライン

UnityログからAUI学習データセットを作成し、初期モデルを学習するパイプラインを実装した。

データセット生成:

```powershell
python Tools/build_cue_training_dataset.py --include-none
```

モデル学習:

```powershell
python Tools/train_cue_model.py --epochs 40
```

データセット確認:

```powershell
python Tools/summarize_cue_training_dataset.py
```

出力:

```text
cue_training_dataset.csv
Models/cue_model.json
cue_training_predictions.csv
```

## 現在の学習結果

既存Unityログ18本から、AUI学習用データセットを生成した。

```text
Source CSV files: 18
Samples: 17,562
Train rows: 13,172
Test rows: 4,390
```

cueTypeの分布:

```text
Footstep: 12,946
Voice: 2,264
None: 2,104
AmbientPresence: 248
```

初期モデルの結果:

```text
cueType test accuracy: 0.8091
presenceScore MAE: 0.3070
volumeGain MAE: 0.3070
cueLowPassHz MAE: 0.0000
cueReverbAmount MAE: 0.0000
cueOcclusionGain MAE: 0.0000
```

この結果により、UnityログからAUI学習データセットを作成し、cueTypeとcue制御パラメータを予測する初期モデルを学習・評価・保存するところまで完了した。

## 現在の限界

現時点の学習は、既存の古いログを利用している。

古いログには、現在追加した以下の情報が十分に含まれていない。

- `cueCondition`
- `EnvironmentAdaptiveCue`
- 環境音響プロファイルの変化
- low-pass、reverb、occlusionの変化

そのため、現在の既存データでは以下が固定値になっている。

```text
cueCondition: StateBasedCue
materialClass: Neutral
cueLowPassHz: 22000
cueReverbAmount: 0
cueOcclusionGain: 1
```

したがって、現在の学習結果は「AUI学習パイプラインが成立した初期ベースライン」であり、EnvironmentAdaptiveCueを本格的に学習できた段階ではない。

## 実装上の改善点

上記の限界を解消するために、AUIログ収集用の自動化を追加した。

`PeripheralAuiLogCollectionController` により、以下を自動で組み合わせられる。

```text
Target scenarios:
Approach / BackApproach / Crossing / Speaking

Cue conditions:
NoCue / FixedCue / StateBasedCue / EnvironmentAdaptiveCue

Environment presets:
Neutral / Reverberant / Occluded
```

これにより、次のログ収集では環境適応cueの教師値が変化し、以下を学習対象にできる。

- low-pass cutoff prediction
- reverb amount prediction
- occlusion gain prediction
- environment-adaptive cue control

## 検証状況

以下の確認を行った。

```text
python Tools/build_cue_training_dataset.py --include-none
python Tools/train_cue_model.py --epochs 40
python Tools/summarize_cue_training_dataset.py
```

Unityのバッチコンパイルも実行し、エラーなく終了した。

```text
Exiting batchmode successfully now!
return code 0
```

## 研究設計の整理

研究設計として、以下の文書も作成した。

- `RESEARCH_DESIGN.md`: 第一研究の研究課題、仮説、実験条件、評価指標。
- `SECOND_PROJECT_RESEARCH_DESIGN.md`: 第二プロジェクトとマルチモーダルCPSへの発展計画。
- `AI_TRAINING_SCHEMA.md`: AUI学習用データセットとモデルの設計。
- `AUI_TRAINING_REPORT.md`: AUI学習結果の詳細。

第一研究の位置づけ:

```text
適応的空間音響cueによって、
XR空間における周辺人物への気づきを改善する。
```

第二プロジェクトの位置づけ:

```text
複数の軽量信号から人間・環境・エージェントの状態を推定し、
状況に応じて適応フィードバックを行う。
```

最終的な発展:

```text
人間中心マルチモーダルCPSインターフェース
```

## 次に行うこと

次の実装・実験準備は以下である。

1. Unityで `Tools > Peripheral Research > Create Demo Hierarchy` を実行する。
2. `PeripheralSystem` の `PeripheralAuiLogCollectionController.autoAdvanceTrials` を有効にする。
3. `EnvironmentAdaptiveCue` を含む新規ログを収集する。
4. `cue_training_dataset.csv` を再生成する。
5. `Tools/train_cue_model.py` で再学習する。
6. 環境適応パラメータの予測精度を評価する。

特に次の段階では、以下を重点的に確認する。

```text
EnvironmentAdaptiveCue条件で、
roomScale、materialClass、reverbAmount、occlusionStrength、rt60、drrの違いが
cueLowPassHz、cueReverbAmount、cueOcclusionGainに反映され、
それをAUIモデルが学習できるか。
```

## 報告用要約

```text
現在、XR空間で周辺人物への気づきを支援するAUIの実装を進めている。
Unity上で人物状態を検出し、cueTypeや音量、フィルタ、残響、遮蔽などのcue制御パラメータを出力・記録する仕組みを実装した。
さらに、UnityログからAUI学習データセットを生成し、初期モデルを学習・評価・保存するパイプラインを構築した。
既存ログ18本から17,562サンプルを作成し、初期モデルではcueTypeのテスト精度が約0.81となった。
現在は古いログを用いた初期ベースラインであるため、次にEnvironmentAdaptiveCueを含む新規ログを収集し、環境適応パラメータを本格的に学習する。
```

