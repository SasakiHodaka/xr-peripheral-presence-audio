# Evaluation Data Specification

This document fixes the data contract for the first pilot evaluation. The goal
is to ensure that each scripted utterance can be connected to ground truth,
participant responses, token logs, packet logs, and final analysis outputs.

## Evaluation Unit

The primary evaluation unit is one scripted utterance or response window.

```text
Scenario utterance
  -> expected Scene Token fields
  -> participant response
  -> scored result
```

Token rows are still sampled continuously, but scoring should be based on
clearly defined utterance windows rather than arbitrary frames.

## Four-Layer Data Contract

The evaluation data should preserve the separation between the proposed method,
the Unity implementation, the recorded logs, and the final analysis.

| Layer | Role | Required artifacts |
| --- | --- | --- |
| Research Layer | Defines what should be represented and evaluated | RQ, hypotheses, scenario ground truth, scoring rules |
| Implementation Layer | Produces Scene Tokens, selections, packets, and rendering conditions | Unity scene, scripted conversation, C1-C4 condition control |
| Logging Layer | Captures all data needed after the run | token CSV, event CSV, metrics CSV, packet CSV, questionnaire CSV |
| Evaluation Layer | Converts logs into research evidence | analyzers, `summary.md`, condition statistics, figures, hypothesis tests |

The current Unity logs cover much of the Logging Layer, but RQ3 and RQ4 also
need comprehension and questionnaire data.

Evaluation processing should follow this order:

```text
CSV
  -> Analyzer
  -> summary.md
  -> Statistics
  -> Figure / Table
  -> Paper
```

## Scenario Ground Truth

Each scenario should define utterances before running participants.

Required scenario fields:

| Field | Meaning | Example |
| --- | --- | --- |
| `scenarioId` | Scenario identifier | `A_instruction` |
| `utteranceId` | Unique utterance identifier | `A03` |
| `startTime` | Script-relative start time in seconds | `12.0` |
| `endTime` | Script-relative end time in seconds | `15.0` |
| `speakerId` | Ground-truth active speaker | `A` |
| `expectedDirection` | Expected direction token at response time | `FRONT_RIGHT` |
| `expectedDistance` | Expected distance token at response time | `MID` |
| `expectedSpeakingState` | Ground-truth speaking state | `SPEAKING` |
| `expectedTurnState` | Ground-truth turn state | `TURN_HOLDER` |
| `expectedSemanticToken` | Ground-truth utterance role | `INSTRUCTION` |
| `expectedUrgency` | Ground-truth urgency | `HIGH` |
| `targetObjectId` | Referenced object, if any | `Valve2` |
| `allowResponse` | Whether participant responses should be scored | `true` |
| `expectedAnswerType` | What participant should answer | `speaker`, `direction`, `semantic`, `comprehension` |

## Scenario Set

The first pilot should use three compact scenarios.

| Scenario | Communication load | Expected difficulty | Purpose | Required labels |
| --- | --- | --- | --- | --- |
| Scenario A | Low | Easy | Low-load question and answer baseline | `QUESTION`, `ANSWER`, `TURN_HOLDER` |
| Scenario B | Medium | Medium | Collaborative instruction and target understanding | `INSTRUCTION`, `ANSWER`, `TURN_HOLDER` |
| Scenario C | High | Hard | Warning and overlap under high communication load | `WARNING`, `OVERLAPPER`, `SPEAKING` |

Each scenario should be 30 to 60 seconds and should include at least one clear
response window per condition.

Scenario difficulty is part of the evaluation design. Scenario A checks whether
the proposed representation preserves basic perception under low load. Scenario
B checks semantic and task understanding under medium load. Scenario C checks
whether important tokens remain useful under high load with overlap and warning
events.

## Scenario-to-RQ Mapping

| Scenario | Communication load | Main RQ | Secondary RQ | Reason |
| --- | --- | --- | --- | --- |
| Scenario A | Low | RQ2 | RQ3 | Tests whether direction and speaker perception are maintained in a simple conversation. |
| Scenario B | Medium | RQ3 | RQ2 | Tests whether instruction semantics and target-object understanding improve task comprehension. |
| Scenario C | High | RQ1, RQ3 | RQ2, RQ4 | Tests whether important warning tokens survive selection and remain understandable during overlap. |

## Scenario A/B/C Ground Truth Draft

This table is the final design artifact before implementation and pilot data
collection. Times are script-relative draft values and can be adjusted when the
Unity scripted conversation is finalized.

### Scenario A: Question and Answer

Purpose:

- Evaluate basic direction awareness, active-speaker awareness, and question /
  answer understanding.

| utteranceId | startTime | endTime | speakerId | expectedSemanticToken | expectedTurnState | expectedUrgency | allowResponse | expectedAnswerType | expectedAnswer | scoreWeight |
| --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- | ---: |
| A01 | 0.0 | 3.0 | A | QUESTION | TURN_HOLDER | MEDIUM | true | speaker | A | 1 |
| A02 | 3.5 | 6.5 | B | ANSWER | TURN_HOLDER | LOW | true | speaker | B | 1 |
| A03 | 7.0 | 10.0 | A | AGREEMENT | TURN_HOLDER | LOW | false | comprehension | questioner=A;answerer=B | 1 |

Scoring focus:

- RQ2: speaker and direction accuracy during A01 and A02.
- RQ3: participant identifies who asked and who answered.

### Scenario B: Instruction and Confirmation

Purpose:

- Evaluate whether an instruction and its target can be understood.

| utteranceId | startTime | endTime | speakerId | expectedSemanticToken | expectedTurnState | expectedUrgency | targetObjectId | allowResponse | expectedAnswerType | expectedAnswer | scoreWeight |
| --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- | --- | ---: |
| B01 | 0.0 | 3.5 | B | INSTRUCTION | TURN_HOLDER | HIGH | Valve2 | true | semantic | INSTRUCTION | 2 |
| B02 | 4.0 | 6.5 | C | ANSWER | TURN_HOLDER | LOW | Valve2 | true | speaker | C | 1 |
| B03 | 7.0 | 10.0 | B | INSTRUCTION | TURN_HOLDER | MEDIUM | Valve2 | false | comprehension | targetObjectId=Valve2 | 2 |

Scoring focus:

- RQ2: active speaker recognition during B01 and B02.
- RQ3: participant identifies that the utterance was an instruction and names
  or selects the target object.

### Scenario C: Warning and Overlap

Purpose:

- Evaluate whether warning utterances and overlap moments are detected.

| utteranceId | startTime | endTime | speakerId | expectedSemanticToken | expectedTurnState | expectedUrgency | allowResponse | expectedAnswerType | expectedAnswer | scoreWeight |
| --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- | ---: |
| C01 | 0.0 | 3.0 | C | WARNING | TURN_HOLDER | HIGH | true | semantic | WARNING | 3 |
| C02 | 3.5 | 5.5 | A+B | CHAT | OVERLAPPER | LOW | false | overlap | OVERLAP | 2 |
| C03 | 6.0 | 9.0 | A | ANSWER | TURN_HOLDER | LOW | true | speaker | A | 1 |

Scoring focus:

- RQ2: active speaker recognition before and after overlap.
- RQ3: participant detects the warning and recognizes that overlap occurred.

## Ground Truth Rules

- `allowResponse=true` marks response windows that can be scored.
- `allowResponse=false` rows can still be used for comprehension questions but
  should not score immediate direction or speaker responses.
- Overlap rows should be excluded from single-speaker accuracy unless the task
  explicitly asks for overlap detection.
- `expectedAnswerType=speaker` scores against `speakerId`.
- `expectedAnswerType=semantic` scores against `expectedSemanticToken`.
- `expectedAnswerType=overlap` scores whether the participant detected overlap.
- `expectedAnswerType=comprehension` is scored from post-scenario questions.
- `scoreWeight` reflects the expected research importance of the utterance.
  Missing a `WARNING` should affect the weighted score more than missing a
  low-priority `CHAT` utterance.

## Design Phase Exit Criteria

The design phase is complete when:

- Scenario A/B/C are accepted as the pilot scenario set.
- Each scored row has `utteranceId`, ground-truth label, and scoring rule.
- Required Unity logs can preserve the row identity or response-window target.
- The same scenario and ground-truth table can be reused by another evaluator
  to reproduce the same scoring procedure.
- The remaining work is implementation, pilot data collection, analysis, and
  iteration.

## Runtime Log Fields

The existing token, event, metrics, and packet logs should preserve these
fields.

Token-level fields:

| Field | Source |
| --- | --- |
| `sessionId` | `SceneTokenExperimentSession` |
| `participantId` | `SceneTokenExperimentSession` |
| `trialIndex` | `SceneTokenExperimentSession` |
| `condition` | `SceneTokenDecoderRenderer.renderCondition` |
| `speakerId` | `SpeakerObject` |
| `direction` | `DirectionAnalyzer` |
| `distance` | `DistanceAnalyzer` |
| `speakingState` | `ConversationAnalyzer` |
| `turnState` | `ConversationAnalyzer` |
| `semanticToken` | `SpeakerObject` or script |
| `urgency` | `SpeakerObject` or script |
| `importance` | `ImportanceCalculator` |
| `priority` | `SceneTokenManager.CalculatePriority` |
| `selectedForTransmission` | `TokenSelector` |
| `selectionReason` | `TokenSelector` |

Response-event fields:

| Field | Meaning |
| --- | --- |
| `response` | Participant answer |
| `expected` | Ground-truth answer at that moment |
| `isCorrect` | Scored correctness |
| `ambiguous` | Whether scoring should exclude the response |
| `responseLatency` | Seconds from trial start |
| `condition` | Active rendering condition |
| `trial` | Trial index |

Packet-level fields:

| Field | Meaning |
| --- | --- |
| `generatedTokenCount` | Tokens generated before selection |
| `selectedTokenCount` | Tokens kept after selection |
| `droppedTokenCount` | Tokens dropped by selection |
| `importantTokenCount` | Important generated tokens |
| `importantTokenKeptCount` | Important selected tokens |
| `estimatedBytes` | Header plus payload byte estimate |
| `dropRatio` | Dropped divided by generated |
| `importantTokenKeptRatio` | Important kept divided by important generated |

## Analysis Output Contract

The analyzers should produce these outputs before a pilot result is accepted.

| Output | Required content |
| --- | --- |
| `token_summary.csv` | rows, speaking ratio, semantic ratio, turn-holder rows, overlap rows by condition |
| `token_summary_by_speaker.csv` | token distribution by condition and speaker |
| `event_summary.csv` | response counts, scored accuracy, latency, ambiguous responses by condition |
| packet summary | packets/s, bytes/s, tokens/packet, drop ratio, important-token retention |
| `summary.md` | quality checks, token summary, response summary, communication metrics, packet metrics |

## RQ-to-Data Mapping

Each research question must map to concrete logs and analysis outputs.

| RQ | Evaluation target | Required data |
| --- | --- | --- |
| RQ1 Communication Efficiency | `dropRatio`, `importantTokenKeptRatio`, `estimatedBytes`, `payloadBytes`, `packetsPerSecond`, `tokensPerPacket` |
| RQ2 Perceptual Performance | `direction`, `speakerId`, `speakingState`, `turnState`, `response_direction`, `response_speaker`, `expected`, `isCorrect`, `responseLatency`, `ambiguous` |
| RQ3 Cognitive Performance | `utteranceId`, `expectedSemanticToken`, comprehension answers, turn/overlap recognition, condition, `responseLatency` |
| RQ4 User Experience | naturalness rating, ease of following, workload score, annoyance/distraction rating |

RQ3 and RQ4 require questionnaire or post-condition response data in addition
to the current token and event logs.

## Representative Metric Names

Use these metric names consistently in summaries, figures, and thesis text.

| Metric | RQ | Source |
| --- | --- | --- |
| `PacketBytes` | RQ1 Communication Efficiency | packet log, packet analyzer |
| `DropRatio` | RQ1 Communication Efficiency | packet log, token selection metrics |
| `ImportantTokenKeptRatio` | RQ1 Communication Efficiency | packet log |
| `DirectionAccuracy` | RQ2 Perceptual Performance | event log, event analyzer |
| `SpeakerAccuracy` | RQ2 Perceptual Performance | event log, event analyzer |
| `ResponseLatency` | RQ2 Perceptual Performance, RQ3 Cognitive Performance | event log, event analyzer |
| `SituationUnderstanding` | RQ3 Cognitive Performance | comprehension answers |
| `OverlapRecognition` | RQ3 | comprehension or response task |
| `NASA-TLX` | RQ4 User Experience | questionnaire |
| `Naturalness` | RQ4 User Experience | questionnaire |
| `EaseOfFollowing` | RQ4 User Experience | questionnaire |

## Module-to-RQ Evidence

This table should be used when writing the thesis evaluation section.

| Research module | Evidence generated | RQ supported |
| --- | --- | --- |
| Scene Analysis | direction and distance consistency, speaking and turn-state labels | RQ2 |
| Scene Representation | Scene Token fields, semantic labels, turn/overlap records | RQ2, RQ3 |
| Selection Function | drop ratio, important-token kept ratio, selection savings | RQ1 |
| Communication Layer | packet bytes, packets/s, tokens/packet | RQ1 |
| Rendering Layer | participant perception under full and selected-token rendering through response accuracy, latency, comprehension, and ratings | RQ1, RQ2, RQ3, RQ4 |
| Evaluation Layer | summary tables, statistics, figures, result text | RQ1, RQ2, RQ3, RQ4 |

## Minimum Pilot Acceptance Criteria

A pilot run is usable only if:

- every planned condition appears in token, event, metrics, and packet logs
- every scenario has ground-truth rows before the run
- each condition has at least one scored direction response
- each condition has at least one scored speaker response
- ambiguous responses are retained but excluded from accuracy
- all scripted semantic labels appear in the token log
- packet logs are present and included in the summary

## Roadmap Alignment

Current roadmap status:

| Phase | Scope | Status |
| --- | --- | --- |
| Phase 1 | SceneToken, selection, ScenePacket | implemented |
| Phase 2 | Logger, analyzer, summary | mostly implemented |
| Phase 3 | Scenario A/B/C and ground truth | next |
| Phase 4 | participant study and statistics | not started |
| Phase 5 | thesis writing | not started |

The next concrete task is to design Scenario A/B/C with ground-truth labels and
then update Unity logging only where the current CSV contract is insufficient.
