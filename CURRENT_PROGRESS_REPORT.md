# Current Progress Report

## 研究の現在位置

本研究は，XR空間において利用者の視野外または周辺に存在する他者を，空間音響cueによって自然に気づかせることを目的としている．

現在の実装では，Unity上で他者との距離，方向，接近状態，発話状態，横切り状態，視野状態などを取得し，それらの状態に応じて提示音の種類，存在感スコア，音量補正値を出力する仕組みを構築している．

ただし，現在の学習データは，研究者自身の判断やルールベースの出力をもとに作成した初期データである．そのため，Unityと学習パイプラインの動作確認には利用できるが，一般的な利用者にとって適切な気配音であることを示す根拠としては不十分である．

今後の中心課題は，主観的な初期ラベルから，シミュレーションと評価に基づく教師データへ移行することである．

## 現在のUnity実装

現在のUnityプロトタイプには，以下の機能が実装されている．

- `PeripheralStateDetector`: 他者の状態を検出する
- `PeripheralCueModel`: 状態情報からcue種類，存在感スコア，音量補正値を推定する
- `PeripheralCueAudioEmitter`: 推定されたcueを3D音響として再生する
- `PeripheralStateLogger`: 状態情報とcue出力をCSVに記録する
- `PeripheralTrialController`: 試行時間とpre-trial時間を管理する
- `PeripheralTrialConditionController`: Approach，BackApproach，Crossing，Speakingなどの条件を切り替える
- `PeripheralAuiLogCollectionController`: target scenario，cue condition，environment presetを自動で組み合わせてログを収集する

現在のcue条件は以下である．

```text
NoCue
FixedCue
StateBasedCue
EnvironmentAdaptiveCue
LearnedCue
```

現在のtarget scenarioは以下である．

```text
Approach
BackApproach
Crossing
Speaking
None
```

## 現在の学習パイプライン

Unityログから学習用データセットを作成し，軽量なcue-controlモデルを学習するパイプラインを構築している．

データセット生成:

```powershell
python Tools/build_cue_training_dataset.py
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
Assets/Models/cue_model_unity.json
cue_training_predictions.csv
```

`Assets/Models/cue_model_unity.json` を `PeripheralCueModel.learnedModelJson` に割り当て，`comparisonCondition` を `LearnedCue` に設定することで，Unity上で学習済みモデルを利用できる．

## 現在のデータの限界

現在のデータは，以下の意味では有効である．

- Unityログから学習データを作成できることの確認
- cueTypeやpresenceScoreを学習モデルで推定できることの確認
- ルールベース出力と学習モデル出力を比較するための初期ベースライン

一方で，以下の限界がある．

- 教師ラベルが研究者自身の判断やルールに依存している
- 「その音が利用者にとって本当に分かりやすいか」は検証されていない
- 後方接近時に足音がよいのか，気配音がよいのかは物理シミュレーションだけでは決まらない
- SoundSpacesのようなRIR推定とは異なり，本研究の正解ラベルは人間の認知に依存する

したがって，現在の結果は「学習パイプラインが動作する初期実装結果」であり，「状況に応じた適切な気配音を決定できた」という最終結果ではない．

## 今後の研究方針

今後は，以下の流れで教師データを再設計する．

```text
1. Unityで状況を大量生成する
   距離，方向，接近速度，発話状態，横切り状態，視野状態を組み合わせる．

2. 各状況に複数の提示音候補を用意する
   足音，声，気配音，衣擦れ音，呼吸音，無音などを提示候補にする．

3. 被験者実験または評価タスクを行う
   位置認知精度，反応時間，接近認知，分かりやすさ，自然さ，不快感を測定する．

4. 最も認知支援効果が高い音を教師ラベルにする
   「私が良いと思う音」ではなく，「評価結果に基づく音」を正解データにする．

5. 最終データセットでNNまたは軽量モデルを学習する
   入力状態からcueType，presenceScore，volumeGainを推定する．

6. 未知状況とUnity上の動作で評価する
   分類精度，スコア誤差，反応時間，位置認知精度，自然さを確認する．
```

## 先行研究との関係

Meta Audio Simulator，SoundSpaces，SoundSpaces 2.0，Neural Acoustic Fields，自己教師あり学習は，「人間が1件ずつラベル付けするのではなく，シミュレータから大量データを生成する」という点で本研究の参考になる．

ただし，SoundSpacesでは，部屋形状，壁材質，音源位置，聞き手位置などからRIRを物理シミュレーションで計算できる．つまり，正解データは物理法則から導出できる．

一方，本研究の出力は，足音，声，気配音，無音などの提示音選択である．これは物理法則だけでは決まらず，人間が他者の存在をどのように認知するかに依存する．そのため，本研究ではシミュレーションに加えて，人間の認知性能を評価する必要がある．

## 次に行うこと

次の実装・研究作業は以下である．

1. Unityで生成する状況パラメータの表を作成する
2. 各状況に対する提示音候補を整理する
3. 位置認知精度，反応時間，自然さ，不快感を記録する評価形式を決める
4. 評価結果から`cueType`，`presenceScore`，`volumeGain`を作るルールを定義する
5. 評価済み教師データ用のCSVスキーマを実装する
6. ルールベースモデル，主観ラベルモデル，評価済みラベルモデルを比較する

この方針により，現在の主観的な初期データから，シミュレーションと実験に基づく気配音提示モデルへ移行する．
