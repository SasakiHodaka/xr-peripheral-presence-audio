# Scene Token Experiment Summary

- Log directory: `Runs\run_20260706_230957`
- Token log files: 1
- Metrics log files: 1
- Event log files: 1

## Quality Checks

| Check | Result | Details |
| --- | --- | --- |
| All main conditions | OK | ok |
| Token rows | OK | rows=2541 |
| Speaking rows | OK | speaking=746 |
| Semantic rows | OK | semantic=746 |
| Scripted semantic labels | OK | ok |
| Direction responses | OK | responses=11 |
| Speaker responses | OK | responses=11 |
| Scored direction responses | OK | scored=8 |
| Scored speaker responses | OK | scored=7 |

## Token Summary

| Condition | Rows | Speaking % | Semantic % | Turn holder | Overlap | Top direction | Top semantic |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- |
| TRADITIONAL | 894 | 29.6 | 29.6 | 205 | 60 | FRONT (894) | NONE (629) |
| DIRECTION_DISTANCE | 894 | 29.5 | 29.5 | 204 | 60 | FRONT (894) | NONE (630) |
| FULL_SCENE_TOKEN | 753 | 28.8 | 28.8 | 177 | 40 | FRONT (753) | NONE (536) |

## Speaker Token Summary

| Condition | Speaker | Rows | Speaking % | Semantic % | Top direction | Top distance | Top turn | Top semantic |
| --- | --- | ---: | ---: | ---: | --- | --- | --- | --- |
| TRADITIONAL | A | 298 | 37.9 | 37.9 | FRONT (298) | FAR (298) | LISTENER (185) | NONE (185) |
| TRADITIONAL | B | 298 | 30.9 | 30.9 | FRONT (298) | FAR (298) | LISTENER (206) | NONE (206) |
| TRADITIONAL | C | 298 | 20.1 | 20.1 | FRONT (298) | FAR (298) | LISTENER (238) | NONE (238) |
| DIRECTION_DISTANCE | A | 298 | 37.9 | 37.9 | FRONT (298) | FAR (298) | LISTENER (185) | NONE (185) |
| DIRECTION_DISTANCE | B | 298 | 30.5 | 30.5 | FRONT (298) | FAR (298) | LISTENER (207) | NONE (207) |
| DIRECTION_DISTANCE | C | 298 | 20.1 | 20.1 | FRONT (298) | FAR (298) | LISTENER (238) | NONE (238) |
| FULL_SCENE_TOKEN | A | 251 | 38.2 | 38.2 | FRONT (251) | FAR (251) | LISTENER (155) | NONE (155) |
| FULL_SCENE_TOKEN | B | 251 | 32.7 | 32.7 | FRONT (251) | FAR (251) | LISTENER (169) | NONE (169) |
| FULL_SCENE_TOKEN | C | 251 | 15.5 | 15.5 | FRONT (251) | FAR (251) | LISTENER (212) | NONE (212) |

## Response Summary

| Condition | Direction acc. % | Direction latency s | Speaker acc. % | Speaker latency s | Ambiguous |
| --- | ---: | ---: | ---: | ---: | ---: |
| TRADITIONAL | 50.0 | 12.059 | 50.0 | 12.270 | 4 |
| DIRECTION_DISTANCE | 0.0 | 6.891 | 100.0 | 6.383 | 1 |
| FULL_SCENE_TOKEN | 0.0 | 8.615 | 0.0 | 8.940 | 2 |
| (none) | 0.0 | 0.000 | 0.0 | 0.000 | 0 |

## Communication Metrics

| Condition | Tokens/s | JSON B/s | Compact B/s | Object metadata B/s | Compact saving |
| --- | ---: | ---: | ---: | ---: | ---: |
| TRADITIONAL | 28.81 | 3137.25 | 345.76 | 489.83 | 29.4% |
| DIRECTION_DISTANCE | 29.78 | 3239.29 | 357.40 | 506.31 | 29.4% |
| FULL_SCENE_TOKEN | 29.76 | 3230.49 | 357.18 | 506.00 | 29.4% |

## Token Selection Metrics

| Condition | Generated tokens/s | Selected tokens/s | Selected JSON B/s | Token drop | Important send | Selection saving |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| TRADITIONAL | 28.81 | 28.81 | 3137.25 | 0.0% | 100.0% | 0.0% |
| DIRECTION_DISTANCE | 29.78 | 29.78 | 3239.29 | 0.0% | 100.0% | 0.0% |
| FULL_SCENE_TOKEN | 29.76 | 29.76 | 3230.49 | 0.0% | 100.0% | 0.0% |

## Weekly Report Draft

今週は、Scene Token 実験ログを研究評価に使える形で集計するための解析パイプラインを整理した。現在のログでは TRADITIONAL, DIRECTION_DISTANCE, FULL_SCENE_TOKEN の主条件を確認でき、Token ログには合計 2541 行が記録されている。各条件について、発話状態、TurnState、SemanticToken、話者ごとの出現状況を集計できるようになった。また Metrics ログから JSON 表現、Compact 表現、Object Metadata 相当の通信量指標を比較できるため、Scene Token が空間情報と会話状態をどの程度の表現量で扱えるかを確認できる。

参加者応答についても、Direction response を 11 件、Speaker response を 11 件記録できた。これにより、TRADITIONAL、DIRECTION_DISTANCE、FULL_SCENE_TOKEN の 3 条件について、方向回答精度、話者回答精度、反応時間を条件ごとに集計できる状態になった。一方で、一部の応答は発話者が明確でない時点に行われたため ambiguous として記録された。次のパイロットでは、HUD 上の応答タイミング表示や指示文を改善し、曖昧応答を減らす。
