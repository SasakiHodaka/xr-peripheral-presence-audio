# Next Steps

## Immediate

1. Open `Assets/Scenes/SceneTokenMock.unity`.
2. Press Play and run one full session across all five conditions.
3. Confirm token, event, and metrics CSV files are generated.
4. Run `Tools/analyze_scene_token_logs.py` against the metrics output.
5. Save one representative output summary for the next research note.

## Short-Term Implementation

1. Add a small in-scene panel for participant ID and session ID.
2. Add explicit trial completion feedback to the HUD.
3. Add a deterministic scripted conversation sequence for repeatable runs.
4. Add optional recorded voice clips in `Assets/Audio`.
5. Add a CSV parser or notebook for token-level behavioral analysis.

## Research Improvements

1. Define concrete hypotheses for each condition.
2. Decide dependent variables:
   - localization accuracy
   - turn tracking accuracy
   - semantic recognition or comprehension
   - perceived workload
   - bandwidth reduction
3. Add post-trial subjective rating collection.
4. Compare compact scene token traffic with richer object metadata.
5. Introduce real speech transcription and semantic classification after the manual-token baseline is stable.

## Engineering Quality

1. Add Unity EditMode tests for token quantization.
2. Add tests for CSV escaping and metrics aggregation.
3. Add a small validation scene check script.
4. Keep generated folders out of Git:
   - `Library`
   - `Logs`
   - `UserSettings`
   - `Temp`
   - `obj`
5. Re-run normal Unity Editor import after major package or scene changes.
