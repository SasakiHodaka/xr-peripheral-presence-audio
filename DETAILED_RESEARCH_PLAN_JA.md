# 詳細研究計画

作成日: 2026-06-09

## 研究タイトル案

```text
XRにおける周辺的人物存在認知を支援する適応型空間音Cueの研究
```

英語タイトル案:

```text
Adaptive Spatial Audio Cues for Peripheral Human-Presence Awareness in XR
```

## 研究の中心

本研究の中心は、XR空間において利用者の視野外・周辺視野・背後・遮蔽位置にいる人物の存在を、空間音Cueによって自然に気づかせることである。

重要なのは、単に音を鳴らすことではない。状況に応じて、どの音を、どの方向から、どの強さで、どの程度自然に提示するかを制御することである。

本研究で扱う基本ループ:

```text
人物の位置・動き・状態
+ 利用者から見た視野状態
+ 環境音響プロファイル
-> 適応型空間音Cue
-> 気づき・方向認知・自然さ・没入感
```

## 研究問題

XRでは、利用者が作業対象や仮想情報に注意を向けているため、周辺や背後にいる人物に気づきにくい。視覚的な矢印や警告表示は有効だが、視覚負荷や没入感低下を招く可能性がある。

そこで本研究では、視覚チャンネルを占有しにくい空間音を使い、周辺的人物存在を支援する。ただし、音Cueは強すぎると不快・邪魔になり、弱すぎると気づけない。したがって、状況に応じた適応制御が必要になる。

研究問題:

```text
XRシステムは、どのように空間音Cueを適応制御すれば、自然さや没入感を損なわずに周辺的人物存在への気づきを向上できるか。
```

## 研究ギャップ

既存研究には以下の蓄積がある。

- XRやVRにおける空間音は、方向提示・注意誘導・ナビゲーションに有効である。
- 周辺アバター認知や共同作業では、音Cueが空間認知や共存在感を支援できる。
- SoundSpaces系の研究では、仮想環境からRIRなどの音響データを大量生成できる。
- HRIでは、機能音やAR音Cueが協調作業や安全認知に有効になりうる。

しかし、本研究が狙う以下の接続はまだ明確に扱われていない。

```text
XR内の周辺的人物存在状況
-> 候補となる機能的空間音Cue
-> 人間の認知・自然さ・不快感に基づく評価
-> Cue制御ラベル生成
-> Unity上の適応Cue制御
```

つまり、本研究の新規性は「空間音が使える」ことではなく、「周辺的人物存在という状況に対して、評価に基づいて適応Cue制御を設計・学習する」点に置く。

## 研究目的

第1研究の目的は、現在のUnityプロトタイプを用いて、適応型空間音Cueが周辺的人物存在の認知を改善するかを検証することである。

具体的には以下を示す。

1. 視野外・背後・横切り・発話などの人物状態をUnity上で検出できる。
2. 人物状態に応じて空間音Cueを提示できる。
3. Cue条件により、検出時間、見逃し率、方向認知、自然さ、没入感、不快感が変化する。
4. 評価結果をCueラベルとして保存し、後続のCue制御モデル学習に使える。

## 研究質問

主研究質問:

```text
RQ1: 適応型空間音Cueは、NoCueおよび固定Cueと比較して、XRにおける周辺的人物存在への気づきを改善するか。
```

副研究質問:

```text
RQ2: 状態ベースCueと環境適応Cueでは、自然さ・没入感・不快感に差が出るか。

RQ3: 背後接近、横切り、発話のうち、どの状況で適応Cueの効果が最も大きいか。

RQ4: 前後方向と左右方向で、空間音Cueによる方向認知精度に差があるか。

RQ5: 評価に基づくCueラベルは、ルールベースまたは開発者選択ラベルよりも、学習用データとして妥当か。
```

## 仮説

```text
H1: StateBasedCueとEnvironmentAdaptiveCueは、NoCueより検出時間を短縮し、見逃し率を下げる。

H2: EnvironmentAdaptiveCueは、FixedCueおよびStateBasedCueより自然さと没入感を高く保つ。

H3: BackApproachでは、他のシナリオより適応Cueの効果が大きい。

H4: 前後方向の方向誤差は左右方向より大きく、Cue設計上の注意点になる。

H5: 評価由来ラベルは、検出時間・方向認知・自然さ・不快感と結びつくため、単純なルールラベルより研究上の根拠が強い。
```

## 実験条件

### 被験者内要因

Cue条件:

```text
NoCue
FixedCue
StateBasedCue
EnvironmentAdaptiveCue
```

Target scenario:

```text
BackApproach
Crossing
Speaking
```

必要に応じて追加:

```text
Approach
None
```

ただし第1実験では、条件数が増えすぎるため `Approach` と `None` は補助条件または練習条件に回す。

### 条件数

最小構成:

```text
3 target scenarios x 4 cue conditions = 12 conditions
```

各条件2試行:

```text
12 conditions x 2 repetitions = 24 trials / participant
```

各条件3試行:

```text
12 conditions x 3 repetitions = 36 trials / participant
```

推奨:

```text
初回パイロット: 24 trials
本実験: 36 trials
```

## 参加者数

初回パイロット:

```text
N = 3-5
```

目的:

- Unity試行が破綻しないか確認する。
- Cueが聞こえるか確認する。
- 試行時間と疲労を確認する。
- ログ列が不足していないか確認する。

本実験:

```text
N = 12-20
```

目的:

- 条件間比較を行う。
- 効果量と信頼区間を報告する。
- 小規模研究として妥当な範囲で統計的傾向を確認する。

## 1試行の流れ

```text
1. Pre-trial
   画面または音で準備状態に入る。

2. Event start
   ターゲット人物が接近・横切り・発話を開始する。

3. Cue presentation
   Cue条件に応じて空間音Cueを提示する。

4. Participant response
   被験者が気づいた時点でボタンを押す。
   可能なら方向も入力する。

5. Short rating
   必要に応じて短い主観評価を行う。

6. Trial end
   ログ保存、次試行へ進む。
```

## 反応入力

最小入力:

```text
Space key / controller button: 気づいた
```

方向入力:

```text
Left
Right
Front
Back
Unknown
```

信頼度:

```text
1-7 rating
```

方向入力が面倒な場合、最初の実装では以下でよい。

```text
気づいた時点の頭部回転方向
```

これにより、ユーザがターゲット側へ向いたかを客観ログから推定できる。

## 従属変数

### 客観指標

```text
detectionTime
missRate
directionAccuracy
directionError
headTurnLatency
falsePositiveRate
```

定義案:

```text
detectionTime = participantResponseTime - targetEventStartTime

miss = targetEventStartTimeから指定秒数以内に反応なし

directionAccuracy = responseDirectionがtargetDirectionGroupと一致

headTurnLatency = target方向へ一定角度以上頭部が向いた時刻 - targetEventStartTime
```

### 主観指標

7段階評価:

```text
awarenessSupport
naturalness
immersion
annoyance
discomfort
directionConfidence
```

初回実験では、毎試行すべて聞くと負荷が高い。推奨は以下。

```text
毎試行: directionConfidenceのみ
各条件後: awarenessSupport, naturalness, immersion, annoyance, discomfort
```

## ログ設計

既存ログに追加すべき列:

```text
eventStartTime
participantResponded
participantResponseTime
detectionTime
responseDirection
responseConfidence
targetDirectionGroup
directionCorrect
headTurnStarted
headTurnLatency
ratingAwarenessSupport
ratingNaturalness
ratingImmersion
ratingAnnoyance
ratingDiscomfort
trialRandomOrder
isPracticeTrial
```

Cueラベル生成用に追加すべき列:

```text
generatedSituationId
candidateCueType
candidateVolume
candidateLowPassHz
candidateReverbAmount
candidateOcclusionGain
selectedCueLabel
computedPresenceScore
computedVolumeGain
labelSource
```

`labelSource` は以下を区別する。

```text
RuleBased
DeveloperPrototype
ParticipantEvaluation
```

## 実験手順

### Calibration

本試行前に短いキャリブレーションを入れる。

```text
left cue
right cue
front cue
back cue
footstep cue
voice cue
ambient cue
```

目的:

- 空間音方向を理解してもらう。
- 音量の極端な問題を確認する。
- 被験者ごとのHRTF差や前後誤認の影響を少し減らす。

### Practice

練習試行:

```text
3-6 trials
```

本実験ログとは区別し、`isPracticeTrial = true` とする。

### Main Trial

本実験:

```text
24-36 trials
```

順序:

```text
participantごとにランダム化
同じCue条件が連続しすぎないよう制約付きランダム
```

休憩:

```text
12 trialsごとに短い休憩
```

## 分析計画

### 主分析

比較:

```text
cueCondition -> detectionTime
cueCondition -> missRate
cueCondition -> directionAccuracy
cueCondition -> naturalness
cueCondition -> immersion
cueCondition -> annoyance
```

最重要の交互作用:

```text
targetScenario x cueCondition
```

特に注目:

```text
BackApproach x EnvironmentAdaptiveCue
BackApproach x StateBasedCue
```

### 方向別分析

前後誤認が重要なので、方向グループ別に見る。

```text
front
rear
lateral
crossing
```

報告:

```text
directionAccuracy by directionGroup
directionError by directionGroup
detectionTime by directionGroup
```

### 統計方針

Nが小さい可能性が高いため、p値だけに依存しない。

報告するもの:

```text
平均
中央値
標準偏差
95%信頼区間
効果量
被験者別プロット
```

可能なら:

```text
反復測定ANOVA
または線形混合モデル
```

小規模なら:

```text
条件差の効果量と信頼区間を中心に報告
```

## Cueラベル生成

最初の本実験は、Cue条件比較を主目的にする。Cueラベル生成は第1実験後半または第2実験として分ける方が安全である。

### Cueラベル生成の設計

入力:

```text
distance
directionGroup
viewState
approachSpeed
speaking
crossing
environmentProfile
```

候補Cue:

```text
Footstep
Voice
AmbientPresence
ClothingRustle
Breathing
None
```

評価値:

```text
reactionTime
directionAccuracy
approachRecognition
clarityRating
naturalnessRating
discomfortRating
annoyanceRating
```

ラベル:

```text
selectedCueLabel
presenceScore
volumeGain
```

### 初期スコア式

```text
presenceScore =
  0.30 * detectionScore
+ 0.25 * directionScore
+ 0.20 * clarityScore
+ 0.15 * naturalnessScore
- 0.10 * discomfortScore
```

補足:

- `detectionScore` は反応時間を0-1に正規化する。
- `directionScore` は方向正答なら1、不明なら0.5、不正解なら0にする。
- `clarityScore`, `naturalnessScore`, `discomfortScore` は7段階評価を0-1に変換する。
- 最初は透明な固定重みでよい。重み最適化は後続研究に回す。

## モデル学習計画

第1段階:

```text
RuleBasedCue
```

目的:

- 実装確認
- 比較基準

第2段階:

```text
DeveloperPrototypeLabel model
```

目的:

- 現在のパイプラインが学習できるか確認
- 最終結果ではなく予備実装

第3段階:

```text
ParticipantEvaluationLabel model
```

目的:

- 評価由来ラベルからCue制御を学習
- 本研究のAI要素として主張可能

推奨モデル:

```text
RandomForest / Gradient Boosting
Small MLP
```

評価:

```text
cueType accuracy
cueType F1
presenceScore MAE / RMSE
volumeGain MAE / RMSE
unseen situation generalization
Unity playback behavior
```

## 実装優先順位

今やるべき順番:

```text
1. 参加者反応ログ
2. 試行開始イベント時刻ログ
3. 方向入力または頭部回転ベース方向推定
4. trial randomization
5. practice/calibration trial flag
6. 条件別summary export
7. 主観評価ログ
8. candidate cue trial mode
9. evaluation-to-label dataset export
10. 学習モデル更新
```

この順番にする理由:

```text
検出時間と方向認知が取れないと、研究としての主張ができない。
AIモデル改善は、その後でよい。
```

## 第1実験で言えること

第1実験で主張できること:

```text
適応型空間音Cueが、XR内の周辺的人物存在への気づきに与える効果。
状況別に、どのCue条件が有効か。
音Cueが自然さ・没入感・不快感に与える影響。
評価由来Cueラベル生成に必要なログ構造。
```

第1実験で主張しないこと:

```text
完全な音響場推定ができた。
SoundSpaces相当の物理音響シミュレーションを実装した。
空間音だけで高精度な方向認知が常に可能である。
全XRタスクに一般化できる。
```

## 研究ストーリー

論文または発表では、以下の順で説明する。

```text
1. XRでは視覚注意が限られる。
2. 周辺・背後・遮蔽された人物存在に気づきにくい。
3. 空間音は視覚を占有せずに方向と存在感を伝えられる。
4. しかし音Cueは状況依存で、強すぎると邪魔になり、弱すぎると気づけない。
5. そこで、人物状態と環境に応じてCueを適応制御するUnityプロトタイプを作る。
6. NoCue, FixedCue, StateBasedCue, EnvironmentAdaptiveCueを比較する。
7. 検出時間、方向認知、自然さ、没入感、不快感で評価する。
8. 評価結果をCueラベルとして保存し、後続のCue制御学習につなげる。
```

## 次の具体的タスク

直近の作業は以下。

```text
Task 1: ParticipantResponseLogger を追加する。
Task 2: PeripheralStateLogger に response 系列を追加する。
Task 3: TrialController に eventStartTime を明示的に持たせる。
Task 4: TrialConditionController または新規 controller でランダム順序を扱う。
Task 5: analyze_peripheral_csv.py に detectionTime / miss / direction 集計を追加する。
Task 6: パイロット実験用の手順書を作る。
```

最初の実装マイルストーン:

```text
Unity Play ModeでBackApproachを実行し、
被験者がボタンを押した時刻と検出時間がCSVに出る。
```

