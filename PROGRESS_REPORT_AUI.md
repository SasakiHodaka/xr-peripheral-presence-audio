# Progress Report: AUI Learning Implementation

## Report Summary

次回進捗報告では、以下のように報告できる。

```text
AUI学習に向けて、Unityの周辺人物検出ログから
cue-control用の学習データセットを生成し、
初期の軽量モデルを学習・評価・保存するパイプラインを実装した。
```

## What Was Implemented

### 1. Unity Log To Dataset

UnityのCSVログをAUI学習用データセットに変換する処理を実装した。

Script:

```powershell
python Tools/build_cue_training_dataset.py --include-none
```

Output:

```text
cue_training_dataset.csv
```

Current result:

```text
Source CSV files: 18
Rows: 17,562
```

### 2. AUI Cue-Control Model Training

AUIの初期モデルとして、以下を予測する軽量モデルを実装した。

Predicted outputs:

- `cueType`
- `presenceScore`
- `volumeGain`
- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

Script:

```powershell
python Tools/train_cue_model.py --epochs 40
```

Outputs:

```text
Models/cue_model.json
cue_training_predictions.csv
```

### 3. Evaluation

Current training result:

```text
Train rows: 13,172
Test rows: 4,390
cueType test accuracy: 0.8091
presenceScore MAE: 0.3074
volumeGain MAE: 0.3074
cueLowPassHz MAE: 0.0000
cueReverbAmount MAE: 0.0000
cueOcclusionGain MAE: 0.0000
```

Class distribution in the full dataset:

```text
Footstep: 12,946
Voice: 2,264
None: 2,104
AmbientPresence: 248
```

Dataset summary command:

```powershell
python Tools/summarize_cue_training_dataset.py
```

Current dataset target ranges:

```text
presenceScore: min 0.0000 / max 1.0000 / mean 0.7035
volumeGain: min 0.0000 / max 1.0000 / mean 0.7035
cueLowPassHz: fixed at 22000
cueReverbAmount: fixed at 0
cueOcclusionGain: fixed at 1
```

## What This Means

現時点で完了していること:

- UnityログからAUI学習用CSVを作れる。
- cueTypeとcue制御パラメータを教師ラベルとして扱える。
- 学習、評価、モデル保存、予測CSV出力まで通っている。
- 報告できる初期精度が出ている。

重要な解釈:

```text
これは最終モデルではなく、AUI学習パイプラインが成立したことを示す初期ベースラインである。
```

## Current Limitation

既存ログは古く、現在の `cueCondition` や環境適応パラメータが十分に含まれていない。

そのため、古いログでは以下の値が固定に近い。

- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

このため、現在のMAEは0になっているが、これはモデルが高度な環境適応を学習したという意味ではない。

正確には、

```text
既存ログに対して、現在のrule-based cue policyを教師ラベルとして補完し、
初期AUIモデルを学習した段階
```

である。

## Next Implementation Step

次に必要なのは、新しいUnityログ収集である。

Collect logs for:

```text
Target scenarios:
- Approach
- BackApproach
- Crossing
- Speaking

Cue conditions:
- NoCue
- FixedCue
- StateBasedCue
- EnvironmentAdaptiveCue
```

特に `EnvironmentAdaptiveCue` では、以下を変えてログを取る。

```text
EnvironmentAcousticProfile:
- roomScale
- materialClass
- reverbAmount
- occlusionStrength
- distanceAttenuation
- rt60
- drr
```

これにより、次の学習では以下が意味を持つ。

- low-pass cutoff prediction
- reverb prediction
- occlusion gain prediction
- environment-adaptive cue control

## Suggested Slide Structure

進捗報告スライドは5枚でよい。

```text
1. 研究全体: 新時代インターフェースとしてのAUI
2. 今回の実装: Unityログ -> AUI学習データセット
3. 学習モデル: cueType + cue parameters prediction
4. 初期結果: 17,562 rows / cueType accuracy 0.8091
5. 次の課題: EnvironmentAdaptiveCueログ収集と再学習
```

## One-Sentence Report

```text
Unity上の周辺人物状態ログを用いて、AUIが提示すべきcue種別とcue制御パラメータを予測する初期学習パイプラインを実装し、17,562サンプルで学習・評価まで完了した。
```
