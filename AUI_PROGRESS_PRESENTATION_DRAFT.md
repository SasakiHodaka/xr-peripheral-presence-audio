# AUI Progress Presentation Draft

## Slide 1: 研究の目的

### Slide Text

```text
XR空間における周辺・視野外の他者の存在を，
状況に応じた空間音響cueによって自然に提示する．

入力:
距離，方向，接近速度，発話状態，横切り状態，視野状態

出力:
cueType，presenceScore，volumeGain
```

### Talk Script

本研究では，XR空間で利用者が視野外や周辺にいる他者を見落とす問題に対して，視覚UIではなく空間音響cueで存在を伝える方法を検討している．

## Slide 2: 現在の実装

### Slide Text

```text
Unity prototype

PeripheralStateDetector
-> PeripheralCueModel / LearnedCue
-> PeripheralCueAudioEmitter
-> CSV logging
```

Implemented:

- 状態検出
- cue出力
- 3D音響再生
- CSVログ
- 学習用データセット生成
- 初期モデル学習

### Talk Script

現在は，Unity上で他者の状態を検出し，cueTypeやpresenceScoreを出力し，その結果をCSVに記録するシステムを実装している．また，CSVログから学習データを作成し，初期の学習モデルをUnityに戻す流れも実装している．

## Slide 3: 現在のデータの限界

### Slide Text

```text
Current dataset:
rule-based / developer-selected prototype labels

Problem:
利用者にとって本当に良いcueかは未検証
```

Example:

```text
後方から接近
-> 足音が正解か？
-> 気配音が正解か？
-> 呼吸音が正解か？
```

### Talk Script

現在のデータは，研究者自身が良いと判断したcueやルールベース出力をもとに作成している．そのため，学習パイプラインの確認には使えるが，利用者にとって妥当な音であることを示す根拠としては不十分である．

## Slide 4: 先行研究からの方針

### Slide Text

References:

- Meta Audio Simulator
- SoundSpaces
- SoundSpaces 2.0
- Neural Acoustic Fields
- Self-supervised learning

Key idea:

```text
人間が1件ずつラベルを作るのではなく，
シミュレータで大量の状況データを生成する．
```

Important difference:

```text
RIRは物理シミュレーションで正解を作れる．
気配音の正解は人間の認知に依存する．
```

### Talk Script

SoundSpacesなどは，仮想環境から大量のデータを生成し，RIRなどの物理的に導出可能な値を教師信号にしている．本研究でも状況データの大量生成という考え方は使えるが，どの気配音が良いかは物理だけでは決まらないため，評価が必要になる．

## Slide 5: 今後のデータ作成方法

### Slide Text

```text
1. Unityで状況を大量生成
2. 各状況に複数のcue候補を提示
3. 認知性能を評価
4. 最も有効なcueを教師ラベル化
5. NNまたは軽量モデルで学習
```

Situation parameters:

- distance
- direction
- approach speed
- speaking
- crossing
- view state

Cue candidates:

- Footstep
- Voice
- AmbientPresence
- ClothingRustle
- Breathing
- None

### Talk Script

今後は，Unityで距離や方向，接近状態などを組み合わせた状況を大量に作り，各状況に複数の音候補を提示する．その後，評価結果から最も認知支援効果の高い音を教師ラベルとして設定する．

## Slide 6: 評価指標

### Slide Text

Objective:

- localization accuracy
- reaction time
- approach recognition
- miss rate

Subjective:

- clarity
- naturalness
- discomfort
- confidence

Score example:

```text
presenceScore =
  a * localizationScore
  + b * reactionTimeScore
  + c * approachRecognitionScore
  + d * clarityScore
  - e * discomfortScore
```

### Talk Script

評価では，好き嫌いではなく，他者の位置や接近に気づけるかを重視する．位置認知精度や反応時間に加えて，自然さや不快感も測定し，総合的に教師ラベルを作成する．

## Slide 7: 現在の結果の位置づけ

### Slide Text

```text
現在確認できたこと:
Unityログ -> 学習データ -> モデル学習 -> Unity利用

まだ確認できていないこと:
そのcueが利用者にとって最適か
```

### Talk Script

現在の成果は，学習パイプラインが動くことを示す初期ベースラインである．今後は，評価済みラベルを作成し，ルールベースモデル，主観ラベルモデル，評価済みラベルモデルを比較する．

## One-Minute Summary

```text
現在は，XR空間における周辺・視野外の他者状態を検出し，
cueTypeやpresenceScoreを出力するUnityシステムと，
そのログから初期学習モデルを作るパイプラインを実装した．

ただし，現在の教師データは研究者判断やルールベースに依存しており，
一般的な利用者にとって妥当な音であることはまだ示せていない．

今後は，SoundSpacesなどのように仮想環境から大量の状況データを生成し，
各状況に対する複数のcue候補を評価する．
その結果に基づいて教師ラベルを作成し，
状況に応じた気配音を推定するモデルを学習する．
```

## Likely Questions And Answers

### Q1. 現在の学習結果は何を示しているのか？

学習パイプラインが動作することを示している．ただし，教師ラベルが主観・ルールベースであるため，cueの妥当性を示す結果ではない．

### Q2. なぜシミュレータだけで正解ラベルを作れないのか？

RIRなどの音響応答は物理シミュレーションで導出できるが，足音・気配音・呼吸音のどれが分かりやすいかは人間の認知に依存するためである．

### Q3. 今後の正解データはどう作るのか？

Unityで生成した状況に複数のcue候補を提示し，位置認知精度，反応時間，自然さ，不快感などを測定する．最も認知支援効果が高いcueを教師ラベルにする．

### Q4. NNはどこで使うのか？

評価済み教師データを作成した後に，距離，方向，接近速度，発話状態，横切り状態，視野状態からcueType，presenceScore，volumeGainを推定するモデルとして使う．
