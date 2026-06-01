# AUI Progress Presentation Draft

## Slide 1: 研究全体の位置づけ

### Slide text

```text
新時代インターフェースに向けたAUI学習

目的:
人間・環境・エージェントの状態を理解し、
状況に応じて提示方法を変える適応的インターフェースを作る。
```

### Talk script

私の研究全体の方向性は、新時代のインターフェースを作ることです。  
その中でも今回は、XR空間で周辺にいる人物の状態を検出し、AUIがどのようなcueを提示すべきかを学習する初期パイプラインを実装しました。

ここでのAUIは、固定的なUIではなく、状況に応じて音響cueや提示強度を変える適応的ユーザインターフェースとして扱っています。

## Slide 2: 今回実装した学習パイプライン

### Slide text

```text
Unityログ
-> AUI学習データセット
-> cue-controlモデル学習
-> 評価・モデル保存
```

```text
Input:
target state, distance, angle, speed, local position

Output:
cueType, presenceScore, volumeGain,
lowPass, reverb, occlusion
```

### Talk script

今回はUnityで取得した周辺人物検出ログを、AUI学習用のデータセットに変換しました。  
入力は、対象人物の状態、距離、角度、接近速度、ユーザ基準の位置です。  
出力は、AUIが提示すべきcue種別と、音量やフィルタ、残響、遮蔽に関する制御パラメータです。

これにより、ルールベースで設計したcue制御を、まずは学習モデルとして再現できる状態にしました。

## Slide 3: データセット

### Slide text

```text
Source CSV files: 18
Samples: 17,562
```

```text
cueType distribution:
Footstep: 12,946
Voice: 2,264
None: 2,104
AmbientPresence: 248
```

### Talk script

既存のUnityログ18本から、17,562サンプルの学習データセットを作成しました。  
cueTypeの内訳は、Footstepが最も多く、次にVoice、None、AmbientPresenceとなっています。

既存ログには古い形式のものが含まれていたため、現在のrule-based cue policyを使って教師ラベルを補完しています。  
そのため、これは最終データセットではなく、AUI学習パイプラインを立ち上げるための初期データセットです。

## Slide 4: 初期モデルと評価結果

### Slide text

```text
Model:
lightweight cue-control baseline

Train rows: 13,172
Test rows: 4,390

cueType test accuracy: 0.8091
presenceScore MAE: 0.3074
volumeGain MAE: 0.3074
```

### Talk script

初期モデルとして、標準ライブラリだけで動く軽量なcue-control baselineを実装しました。  
cueTypeは分類問題として扱い、presenceScoreやvolumeGainなどは回帰問題として扱っています。

結果として、テストデータに対するcueTypeのaccuracyは約0.81でした。  
presenceScoreとvolumeGainのMAEは約0.31です。

現段階では高精度モデルを作ることよりも、Unityログからデータセット化し、学習、評価、モデル保存まで通ることを重視しています。

## Slide 5: 現在の限界

### Slide text

```text
Current limitation:
old logs do not include enough EnvironmentAdaptiveCue variation
```

```text
cueLowPassHz: fixed at 22000
cueReverbAmount: fixed at 0
cueOcclusionGain: fixed at 1
```

### Talk script

現在の限界は、既存ログが古く、cueConditionやEnvironmentAdaptiveCueの変化が十分に含まれていない点です。  
そのため、low-pass、reverb、occlusionの教師値はほぼ固定になっています。

したがって、現時点で環境適応を本格的に学習できたというよりは、AUI学習のパイプラインが成立した段階です。  
次に新しいログを取ることで、環境に応じたcue制御を学習対象にできます。

## Slide 6: 次にやること

### Slide text

```text
Next:
collect new logs with cue conditions
```

```text
Target scenarios:
Approach / BackApproach / Crossing / Speaking

Cue conditions:
NoCue / FixedCue / StateBasedCue / EnvironmentAdaptiveCue
```

```text
EnvironmentAdaptiveCue:
vary roomScale, materialClass, reverbAmount,
occlusionStrength, distanceAttenuation, rt60, drr
```

### Talk script

次は、新しいUnityログを収集します。  
対象シナリオとして、Approach、BackApproach、Crossing、Speakingを使い、cue条件としてNoCue、FixedCue、StateBasedCue、EnvironmentAdaptiveCueを比較します。

特にEnvironmentAdaptiveCueでは、EnvironmentAcousticProfileの値を変化させて、low-pass、reverb、occlusionが変わるデータを作ります。  
これにより、AUIが環境に応じたcue制御を学習できるようになります。

## One-Minute Summary

```text
今回は、Unityで取得した周辺人物状態ログからAUI学習用データセットを作成し、
cueTypeとcue制御パラメータを予測する初期モデルを実装しました。

18本のCSVログから17,562サンプルを作成し、
初期モデルではcueTypeのテストaccuracyが約0.81になりました。

現時点では古いログを用いたrule-based教師による初期学習なので、
次はEnvironmentAdaptiveCue条件で新規ログを取り、
環境適応パラメータを本格的に学習対象にします。
```

## Likely Questions And Answers

### Q1. これは本当に学習と言えるのか？

```text
はい。Unityログから入力特徴量と教師ラベルを作り、
train/test splitで評価し、モデル保存と予測CSV出力まで実装しています。
ただし、現段階ではrule-based policyを教師とした初期学習です。
```

### Q2. なぜ精度が0.81なのか？

```text
現在は軽量な初期baselineであり、データにもクラス偏りがあります。
目的は最終精度ではなく、AUI学習パイプラインを成立させることです。
次に新規ログを増やし、モデルと特徴量を改善します。
```

### Q3. reverbやocclusionのMAEが0なのは良い結果なのか？

```text
良い結果というより、既存ログではそれらの教師値が固定だからです。
EnvironmentAdaptiveCueの新規ログを収集すると、これらの値が変化し、
本格的な環境適応学習の評価が可能になります。
```

### Q4. 次に何ができれば研究として進むのか？

```text
4つのtarget scenarioと4つのcue conditionでログを取り、
EnvironmentAcousticProfileを変化させたデータを追加します。
その後、AUIモデルがcueTypeだけでなく環境適応パラメータも
予測できるかを評価します。
```

### Q5. 最終的に何につながるのか？

```text
このAUI学習は、状況に応じて提示方法を変える
新時代インターフェースの基礎です。
次の第二プロジェクトではマルチモーダル状況推定へ拡張し、
最終的には人間中心のマルチモーダルCPSインターフェースへ発展させます。
```

