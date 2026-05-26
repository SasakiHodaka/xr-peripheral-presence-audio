# Peripheral Research Notes

## Demo Flow

1. Open `Assets/Scenes/SampleScene.unity`.
2. Run `Tools > Peripheral Research > Create Demo Hierarchy`.
3. Select `PeripheralSystem`.
4. Assign `Main Camera` or `XR Origin/Main Camera` to `PeripheralStateDetector.userHead` if it is empty.
5. Enter Play Mode.
6. Confirm that the Game view shows `Peripheral Debug`.
7. Confirm that Unity Console prints `Peripheral CSV created: ...`.

CSV files are written to Unity's `Application.persistentDataPath`.
In the current Windows Editor setup this is typically:

```text
C:\Users\acd-pc67\AppData\LocalLow\DefaultCompany\My project
```

The logger uses timestamped filenames by default:

```text
peripheral_state_log_yyyyMMdd_HHmmss.csv
```

## CSV Columns

- `time`: Unity play time in seconds.
- `targetId`: Target identifier, such as `Target_Approach`.
- `state`: Combined peripheral state flags.
- `outOfView`: Target is outside the configured field of view.
- `approaching`: Target is moving toward the user.
- `speaking`: Target is marked as speaking.
- `gazing`: Target is facing the user within the gaze threshold.
- `near`: Target is inside the near-distance threshold.
- `crossing`: Target is crossing in front of the user.
- `distance`: Distance from user head to target in meters.
- `viewAngle`: Angle between user forward direction and target direction.
- `radialSpeed`: Positive value means the target is approaching.
- `lateralSpeed`: Sideways movement speed in user-local space.
- `localX`, `localY`, `localZ`: Target position in user-head local coordinates.

## Initial Metrics To Inspect

- How often `outOfView` is true while `approaching` is also true.
- Time from first `approaching` detection to `near`.
- Whether `crossing` appears during `Target_Crossing` movement.
- Whether `speaking` appears only for speaking targets.
- Whether `viewAngle` and `localX` match the user's intuitive left/right and front/back perception.

## CSV Analysis Script

Run this from the project root to analyze the newest CSV log:

```powershell
python Tools/analyze_peripheral_csv.py
```

This also writes a summary CSV next to the source log:

```text
peripheral_state_log_yyyyMMdd_HHmmss_summary.csv
```

To analyze a specific CSV file:

```powershell
python Tools/analyze_peripheral_csv.py "C:\Users\acd-pc67\AppData\LocalLow\DefaultCompany\My project\peripheral_state_log_yyyyMMdd_HHmmss.csv"
```

To print only without writing a summary CSV:

```powershell
python Tools/analyze_peripheral_csv.py --no-summary-csv
```

To summarize all source logs in the log directory:

```powershell
python Tools/analyze_peripheral_csv.py --batch
```

This writes:

```text
peripheral_batch_summary.csv
```

To generate a browser-readable HTML report:

```powershell
python Tools/analyze_peripheral_csv.py --html-report
```

This writes:

```text
peripheral_report.html
```

The batch CSV and HTML report include `demoCheck`. This is a quick demo-health check, not a final research metric:

- `Target_Approach`: expects `approaching` and `near`.
- `Target_Back`: expects `outOfView + approaching`.
- `Target_Crossing`: expects `crossing`.
- `Target_Speaking`: expects `speaking`.

Very short Play Mode sessions can show `Check approach/near` because the approach target may not have enough time to reach `near`.

The script prints per-target row counts, state counts, first detection times, `outOfView + approaching` counts, and the time from first `approaching` to first `near`.

## Current Scope

Older Presence and greeting-meeting assets remain in the project. Do not delete them yet; `PeripheralTarget` can still bridge to `PresenceTarget` and `GroupWorkPresenceAudio`.
