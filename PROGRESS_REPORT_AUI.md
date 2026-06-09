# Progress Report: AUI Learning Implementation

## Summary

Unityログからcue-control用の学習データを作成し，初期の軽量モデルを学習・評価するパイプラインを実装した．

この実装の目的は，最終的な気配音提示モデルを完成させることではなく，以下を確認することである．

- Unity上で取得した状態ログを学習データへ変換できるか
- `cueType`，`presenceScore`，`volumeGain`などをモデルで推定できるか
- 学習済みモデルをUnityに戻して利用できるか

現在の教師ラベルは，ルールベース出力と研究者判断に基づく初期ラベルである．そのため，この結果は「学習パイプラインの初期ベースライン」であり，「利用者にとって最適な気配音を学習できた」という結果ではない．

## Implemented Pipeline

現在のパイプラインは以下である．

```text
Unity peripheral CSV logs
-> cue_training_dataset.csv
-> lightweight cue-control model
-> Models/cue_model.json
-> Assets/Models/cue_model_unity.json
-> Unity LearnedCue playback
```

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

## Current Dataset

現在のデータセットは既存のUnityログから作成した．

```text
Source CSV files: 18
Rows: 17,562
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

現在の入力情報:

- target state flags: `outOfView`, `approaching`, `speaking`, `gazing`, `near`, `crossing`
- distance
- view angle
- radial speed
- lateral speed
- user-local target position
- cue condition
- environment profile values

現在の出力情報:

- `cueType`
- `presenceScore`
- `volumeGain`
- `cueLowPassHz`
- `cueReverbAmount`
- `cueOcclusionGain`

## Current Result

初期モデルの評価結果は以下である．

```text
cueType test accuracy: 0.8091
presenceScore MAE: 0.3074
volumeGain MAE: 0.3074
cueLowPassHz MAE: 0.0000
cueReverbAmount MAE: 0.0000
cueOcclusionGain MAE: 0.0000
```

`cueLowPassHz`，`cueReverbAmount`，`cueOcclusionGain`のMAEが0である理由は，現在の古いログではこれらのラベルがほぼ固定値として補完されているためである．これはモデルが高度な環境適応を学習できたことを意味しない．

## Interpretation

今回確認できたことは以下である．

- Unityログから学習データを生成できる
- cueType分類とpresence/volume回帰の初期モデルを学習できる
- 学習済みモデルをUnity用JSONとして保存できる
- `LearnedCue`条件でUnity側へ接続できる

一方で，現在の教師データには信頼性の課題がある．

現在のラベルは，研究者自身が良いと判断した音やルールベース出力に基づいている．そのため，後方から接近する対象に対して足音がよいのか，気配音がよいのか，呼吸音がよいのかは十分に検証されていない．

したがって，この学習結果は最終的な妥当性評価ではなく，評価済み教師データを作る前段階のベースラインとして扱う．

## Next Step

次の段階では，主観的な初期ラベルではなく，評価に基づく教師データを作成する．

予定する流れ:

```text
1. Unityで状況を大量生成する
   距離，方向，接近速度，発話状態，横切り状態，視野状態を組み合わせる．

2. 各状況に複数のcue候補を提示する
   Footstep，Voice，AmbientPresence，ClothingRustle，Breathing，Noneなどを用いる．

3. 評価指標を測定する
   位置認知精度，反応時間，接近認知，分かりやすさ，自然さ，不快感を記録する．

4. 最も評価の高いcueを教師ラベルにする
   cueType，presenceScore，volumeGainを評価結果から作成する．

5. 評価済みラベルでモデルを再学習する
   ルールベースモデル，主観ラベルモデル，評価済みラベルモデルを比較する．
```

この流れにより，「自分が良いと思った音」ではなく，「シミュレーションと評価に基づく気配音」を学習対象にする．
