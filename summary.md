# Scene Token Experiment Summary

- Log directory: `Runs\latest_run`
- Token log files: 1
- Metrics log files: 1
- Event log files: 1

## Quality Checks

| Check | Result | Details |
| --- | --- | --- |
| All five conditions | OK | ok |
| Token rows | OK | rows=4773 |
| Speaking rows | OK | speaking=1406 |
| Semantic rows | OK | semantic=1406 |
| Scripted semantic labels | OK | ok |
| Direction responses | CHECK | responses=0 |
| Speaker responses | CHECK | responses=0 |
| Scored direction responses | CHECK | scored=0 |
| Scored speaker responses | CHECK | scored=0 |

## Token Summary

| Condition | Rows | Speaking % | Semantic % | Turn holder | Overlap | Top direction | Top semantic |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- |
| DIRECTION_DISTANCE | 1302 | 29.4 | 29.4 | 303 | 80 | FRONT (1302) | NONE (919) |
| DIRECTION_DISTANCE_SPEAKING | 336 | 27.4 | 27.4 | 72 | 20 | FRONT (336) | NONE (244) |
| DIRECTION_ONLY | 1092 | 30.5 | 30.5 | 253 | 80 | FRONT (1092) | NONE (759) |
| FULL_SCENE_TOKEN | 618 | 29.4 | 29.4 | 142 | 40 | FRONT (618) | NONE (436) |
| TRADITIONAL | 1425 | 29.2 | 29.2 | 328 | 88 | FRONT (1425) | NONE (1009) |

## Speaker Token Summary

| Condition | Speaker | Rows | Speaking % | Semantic % | Top direction | Top distance | Top turn | Top semantic |
| --- | --- | ---: | ---: | ---: | --- | --- | --- | --- |
| DIRECTION_DISTANCE | A | 434 | 36.9 | 36.9 | FRONT (434) | FAR (434) | LISTENER (274) | NONE (274) |
| DIRECTION_DISTANCE | B | 434 | 32.9 | 32.9 | FRONT (434) | FAR (434) | LISTENER (291) | NONE (291) |
| DIRECTION_DISTANCE | C | 434 | 18.4 | 18.4 | FRONT (434) | FAR (434) | LISTENER (354) | NONE (354) |
| DIRECTION_DISTANCE_SPEAKING | A | 112 | 44.6 | 44.6 | FRONT (112) | FAR (112) | LISTENER (62) | NONE (62) |
| DIRECTION_DISTANCE_SPEAKING | B | 112 | 19.6 | 19.6 | FRONT (112) | FAR (112) | LISTENER (90) | NONE (90) |
| DIRECTION_DISTANCE_SPEAKING | C | 112 | 17.9 | 17.9 | FRONT (112) | FAR (112) | LISTENER (92) | NONE (92) |
| DIRECTION_ONLY | A | 364 | 39.0 | 39.0 | FRONT (364) | FAR (364) | LISTENER (222) | NONE (222) |
| DIRECTION_ONLY | B | 364 | 30.8 | 30.8 | FRONT (364) | FAR (364) | LISTENER (252) | NONE (252) |
| DIRECTION_ONLY | C | 364 | 21.7 | 21.7 | FRONT (364) | FAR (364) | LISTENER (285) | NONE (285) |
| FULL_SCENE_TOKEN | A | 206 | 35.0 | 35.0 | FRONT (206) | FAR (206) | LISTENER (134) | NONE (134) |
| FULL_SCENE_TOKEN | B | 206 | 34.0 | 34.0 | FRONT (206) | FAR (206) | LISTENER (136) | NONE (136) |
| FULL_SCENE_TOKEN | C | 206 | 19.4 | 19.4 | FRONT (206) | FAR (206) | LISTENER (166) | NONE (166) |
| TRADITIONAL | A | 475 | 42.3 | 42.3 | FRONT (475) | FAR (475) | LISTENER (274) | NONE (274) |
| TRADITIONAL | B | 475 | 27.8 | 27.8 | FRONT (475) | FAR (475) | LISTENER (343) | NONE (343) |
| TRADITIONAL | C | 475 | 17.5 | 17.5 | FRONT (475) | FAR (475) | LISTENER (392) | NONE (392) |

## Response Summary

| Condition | Direction acc. % | Direction latency s | Speaker acc. % | Speaker latency s | Ambiguous |
| --- | ---: | ---: | ---: | ---: | ---: |
| (none) | 0.0 | 0.000 | 0.0 | 0.000 | 0 |
| DIRECTION_DISTANCE | 0.0 | 0.000 | 0.0 | 0.000 | 0 |
| DIRECTION_DISTANCE_SPEAKING | 0.0 | 0.000 | 0.0 | 0.000 | 0 |
| DIRECTION_ONLY | 0.0 | 0.000 | 0.0 | 0.000 | 0 |
| TRADITIONAL | 0.0 | 0.000 | 0.0 | 0.000 | 0 |

## Communication Metrics

| Condition | Tokens/s | JSON B/s | Compact B/s | Object metadata B/s | Compact saving |
| --- | ---: | ---: | ---: | ---: | ---: |
| DIRECTION_DISTANCE | 29.81 | 2402.32 | 268.27 | 506.73 | 47.1% |
| DIRECTION_DISTANCE_SPEAKING | 29.80 | 2400.20 | 268.19 | 506.59 | 47.1% |
| DIRECTION_ONLY | 29.81 | 2402.49 | 268.31 | 506.82 | 47.1% |
| FULL_SCENE_TOKEN | 29.82 | 2404.23 | 268.37 | 506.93 | 47.1% |
| TRADITIONAL | 27.59 | 2224.89 | 248.34 | 469.09 | 47.1% |

## Weekly Report Draft

本週は、Scene Token実験ログを集計するための解析パイプラインを整理した。Tokenログでは、各条件における発話状態、TurnState、SemanticTokenの出現状況を確認できる。Eventログでは、方向回答と話者回答の正答率、反応時間、曖昧回答数を条件ごとに集計できる。MetricsログからはJSON表現、Compact表現、Object Metadata相当の通信量指標を比較できる。

今後はUnity Editor上で5条件すべての実ログを取得し、Scene Tokenの追加情報が話者把握、方向把握、会話理解に与える影響を分析する。現時点では通信量削減を主張の中心に置かず、空間情報と会話状態を統合した離散表現がVR音声コミュニケーション理解を支援するかを評価対象とする。
