# 進捗報告（2026/07/14）

## 1. 研究全体の位置づけ

研究目的        ✓
システム設計    ✓
実装            ▶ Phase 1-3: Ground TruthからEvaluationまでの最小E2E
評価            ✓ 最小ログ集計
考察            □

今回の目的:

Ground Truthを入力として、Scene Token生成、Priority-only Selection、Semantic Packet生成、Presentation Policy、CSVログ、評価集計までの最小データパイプラインを構築する。

## 2. 今回取り組んだ背景

提案手法では、Ground Truthを入力としてScene Tokenを生成し、Selection Policyによって送信対象を決定する。
そのため、まずUnity上でシナリオデータを読み込み、意味情報へ変換し、通信制御結果をログとして評価できる基盤を構築した。

## 3. 実装内容

- `GroundTruthLoader.cs`
- `GroundTruthSceneTokenGenerator.cs`
- `PrioritySelectionPolicy.cs`
- `SemanticPacket.cs`
- `SemanticPacketBuilder.cs`
- `PresentationPolicy.cs`
- `ExperimentLogger.cs`
- `ScenarioPlayer.cs`
- `ScenarioBootstrapper.cs`
- `scenario_01.json`
- `scenario_02.json`
- `scenario_03.json`
- `analysis/evaluate.py`

## 4. 動作確認

- `scenario_03.json` をUnityから読み込み成功
- Event数4件を取得成功
- Direction計算により `Right`, `Behind`, `Front` を生成
- Priority-only Selectionにより `TokenOnly`, `AudioAndToken`, `None` を生成
- Semantic Packetの `flags` と `packetBytes` をCSVへ出力
- Presentation Policyにより `VisualCue`, `SpatialAudioAndVisualCue`, `Muted` を出力
- `analysis/evaluate.py` により最新CSVを集計成功

最新評価結果:

```text
Total events: 4
Selected events: 3
Selection rate: 75.0%
Total packet bytes: 440
Average packet bytes: 146.7
High-priority selected: 1/1
High-priority selection rate: 100.0%

By communication level:
  AudioAndToken: 1
  None: 1
  TokenOnly: 2

By packet flags:
  (none): 1
  5: 2
  7: 1

By presentation:
  Muted: 1
  SpatialAudioAndVisualCue: 1
  VisualCue: 2
```

## 5. 現状の到達点

Ground Truth        ✓
Scene Token         ✓
Logger              ✓
Priority Selection  ✓
Semantic Packet     ✓
Presentation        ✓
Evaluation          ✓

## 6. 発生した課題

- Unityで開いていた実プロジェクトが `C:\Users\acd-pc67\SemanticSpatialAudio` だったため、最初に別フォルダへ作成したファイルを実プロジェクト側へ反映する必要があった。
- 既存コンポーネントのInspector保存値により古いCSV名が使われたため、実行ごとにタイムスタンプ付きCSVを生成する方式へ変更した。
- 評価スクリプトがファイル名順で古いログを拾ったため、更新時刻が最新のログを選択する方式へ修正した。

## 7. 次回予定

- 現在の最小E2Eを基準に、S1/S2/S3のログを個別に取得する。
- `Priority-only Selection` をBaselineとして保持する。
- 次段階で `Relevance` または `User State` を追加し、Priority-onlyとの比較評価へ進む。

## 8. 研究全体への貢献

Ground TruthからEvaluationまでの最小E2Eが完成したことで、提案手法の入力、意味表現、選択制御、通信表現、提示方針、評価集計が一つの再現可能なパイプラインとして接続された。
これにより、今後のSelection Policyの改良や比較評価を、設計議論ではなくCSVログと評価結果に基づいて進められる基盤が整った。
