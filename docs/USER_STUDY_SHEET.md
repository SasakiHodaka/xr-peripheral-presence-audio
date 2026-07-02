# User Study Sheet

This sheet defines the minimum questionnaire and scoring items for the
three-condition user study.

## Conditions

Run the main experiment with:

1. `TRADITIONAL`
2. `DIRECTION_DISTANCE`
3. `FULL_SCENE_TOKEN`

Counterbalance the condition order across participants when possible.

## Per-Condition Objective Tasks

Collect during each condition:

| Item | Input | Log source |
| --- | --- | --- |
| Speaker direction response | Arrow keys or HUD direction buttons | `response_direction` |
| Speaker identity response | `J`, `K`, `L` or HUD buttons | `response_speaker` |
| Response latency | automatic | event log |
| Ambiguous target flag | automatic | event log |

Use only non-ambiguous rows for accuracy scoring.

## Per-Condition Subjective Questions

Use a 7-point Likert scale:

1 = strongly disagree, 7 = strongly agree.

| ID | Question |
| --- | --- |
| Q1 | I could easily identify who was speaking. |
| Q2 | I could easily understand where the speaker was located. |
| Q3 | I could easily follow the flow of the conversation. |
| Q4 | I could notice important utterances such as instructions or warnings. |
| Q5 | The audio presentation felt natural. |
| Q6 | The audio presentation was not annoying or distracting. |
| Q7 | This condition would be useful for VR remote collaboration. |

## NASA-TLX Short Form

Use a 0-100 scale for each item:

| Item | Prompt |
| --- | --- |
| Mental demand | How mentally demanding was the task? |
| Temporal demand | How hurried or rushed was the pace? |
| Effort | How hard did you have to work to perform well? |
| Frustration | How irritated, stressed, or annoyed did you feel? |
| Performance | How successful did you feel? |

Physical demand can be omitted for the desktop mock scene unless a VR headset
task is used.

## Post-Experiment Ranking

Ask after all conditions:

1. Which condition made it easiest to understand who was speaking?
2. Which condition made it easiest to understand where the speaker was?
3. Which condition made it easiest to follow the conversation?
4. Which condition would you prefer for VR remote collaboration?
5. Free comment: What made the audio easy or difficult to understand?

## Communication-Only Evaluation

Token selection is evaluated separately from the subjective user study.

Compare runs with:

- `SceneTokenManager.enableTokenSelection = false`
- `SceneTokenManager.enableTokenSelection = true`

Report:

- `generatedTokensPerSecond`
- `selectedTokensPerSecond`
- `tokenDropRatio`
- `importantTokenSendRatio`
- `selectionSavingsRatio`

The expected communication result is:

```text
Important tokens remain transmitted while low-priority tokens are reduced.
```
