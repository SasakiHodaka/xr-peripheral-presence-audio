#!/usr/bin/env python
import argparse
import csv
import html
from collections import defaultdict
from pathlib import Path


DEFAULT_LOG_DIR = Path.home() / "AppData" / "LocalLow" / "DefaultCompany" / "My project"
STATE_COLUMNS = ("outOfView", "approaching", "speaking", "gazing", "near", "crossing")
METADATA_COLUMNS = ("participantId", "conditionLabel", "trialId")
MIN_TRIAL_DURATION_SECONDS = 10.0
DEFAULT_REACTION_TIME_SECONDS = 10.0


def parse_bool(value):
    return str(value).strip().lower() == "true"


def parse_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def is_source_log(path):
    generated_suffixes = (
        "_summary",
        "_cue_effectiveness",
        "_cue_labels",
        "_cue_ranking_report",
    )
    return (
        path.name.startswith("peripheral_state_log")
        and path.suffix.lower() == ".csv"
        and not any(path.stem.endswith(suffix) for suffix in generated_suffixes)
        and path.name != "peripheral_batch_summary.csv"
    )


def source_csv_paths(log_dir=DEFAULT_LOG_DIR):
    return sorted(
        (path for path in log_dir.glob("peripheral_state_log*.csv") if is_source_log(path)),
        key=lambda path: path.stat().st_mtime,
    )


def source_rows_by_path(log_dir=DEFAULT_LOG_DIR):
    rows_by_path = []
    for path in source_csv_paths(log_dir):
        rows_by_path.append((path, load_rows(path)))
    return rows_by_path


def latest_csv_path(log_dir=DEFAULT_LOG_DIR):
    files = list(reversed(source_csv_paths(log_dir)))
    if not files:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")
    return files[0]


def load_rows(path):
    with path.open("r", encoding="utf-8-sig", newline="") as file:
        return list(csv.DictReader(file))


def summarize(rows):
    by_target = defaultdict(list)
    for row in rows:
        by_target[row.get("targetId", "")].append(row)

    summaries = []
    for target_id, target_rows in sorted(by_target.items()):
        time_column = "trialElapsed" if target_rows and "trialElapsed" in target_rows[0] else "time"
        times = [parse_float(row.get(time_column)) for row in target_rows]
        duration = max(times) - min(times) if times else 0.0

        counts = {
            column: sum(1 for row in target_rows if parse_bool(row.get(column)))
            for column in STATE_COLUMNS
        }
        first_times = {
            column: first_true_time(target_rows, column, time_column)
            for column in STATE_COLUMNS
        }

        approach_to_near = None
        first_approach = first_times["approaching"]
        if first_approach is not None:
            for row in target_rows:
                time = parse_float(row.get(time_column))
                if time >= first_approach and parse_bool(row.get("near")):
                    approach_to_near = time - first_approach
                    break

        out_of_view_approaching = sum(
            1
            for row in target_rows
            if parse_bool(row.get("outOfView")) and parse_bool(row.get("approaching"))
        )

        summaries.append(
            {
                "metadata": first_metadata(target_rows),
                "targetId": target_id or "(empty)",
                "rows": len(target_rows),
                "duration": duration,
                "counts": counts,
                "firstTimes": first_times,
                "approachToNear": approach_to_near,
                "outOfViewApproaching": out_of_view_approaching,
            }
        )

    return summaries


def cue_effectiveness_rows(rows, reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS, objective_only=False):
    groups = defaultdict(list)
    for row in rows:
        condition = row.get("conditionLabel", "")
        cue_candidate = row.get("cueCandidate", "")
        target_id = row.get("targetId", "")
        if not cue_candidate:
            continue
        groups[(condition, target_id, cue_candidate)].append(row)

    output = []
    for (condition, target_id, cue_candidate), group_rows in sorted(groups.items()):
        response_count = sum(1 for row in group_rows if parse_bool(row.get("responseGiven")))
        detection_success = response_count / max(1, len(group_rows))
        reaction_times = [
            parse_float(row.get("reactionTime"), None)
            for row in group_rows
            if row.get("reactionTime")
        ]
        reaction_times = [value for value in reaction_times if value is not None and value >= 0.0]
        mean_reaction_time = sum(reaction_times) / len(reaction_times) if reaction_times else reaction_time_cap
        normalized_reaction_time = min(mean_reaction_time, reaction_time_cap) / reaction_time_cap

        direction_values = [
            row.get("directionResponse", "").strip()
            for row in group_rows
            if row.get("directionResponse", "").strip()
        ]
        direction_response_rate = len(direction_values) / max(1, len(group_rows))
        direction_accuracy = calculate_direction_accuracy(group_rows)
        direction_score = direction_accuracy if direction_accuracy is not None else direction_response_rate

        if objective_only:
            mean_rating = 0.0
            mean_awareness = 0.0
            mean_naturalness = 0.0
            mean_annoyance = 0.0
            mean_confidence = 0.0
            normalized_rating = 0.0
            awareness_score = 0.0
            naturalness_score = 0.0
            annoyance_penalty = 0.0
            confidence_score = 0.0
        else:
            mean_rating = mean_positive_rating(group_rows, "subjectiveRating")
            mean_awareness = mean_positive_rating(group_rows, "awarenessRating")
            mean_naturalness = mean_positive_rating(group_rows, "naturalnessRating")
            mean_annoyance = mean_positive_rating(group_rows, "annoyanceRating")
            mean_confidence = mean_positive_rating(group_rows, "confidenceRating")
            normalized_rating = mean_rating / 5.0 if mean_rating > 0.0 else 0.0
            awareness_score = mean_awareness / 5.0 if mean_awareness > 0.0 else normalized_rating
            naturalness_score = mean_naturalness / 5.0 if mean_naturalness > 0.0 else 0.0
            annoyance_penalty = mean_annoyance / 5.0 if mean_annoyance > 0.0 else 0.0
            confidence_score = mean_confidence / 5.0 if mean_confidence > 0.0 else 0.0

        playback_rows = sum(1 for row in group_rows if parse_bool(row.get("playbackActive")))
        playback_rate = playback_rows / max(1, len(group_rows))

        cue_effectiveness = detection_success + direction_score - normalized_reaction_time
        if not objective_only:
            cue_effectiveness += (
                awareness_score
                + 0.5 * naturalness_score
                + 0.25 * confidence_score
                - 0.5 * annoyance_penalty
            )

        output.append(
            {
                "conditionLabel": condition,
                "targetId": target_id,
                "cueCandidate": cue_candidate,
                "rows": len(group_rows),
                "detectionSuccess": detection_success,
                "meanReactionTime": mean_reaction_time,
                "directionResponseRate": direction_response_rate,
                "directionAccuracy": direction_accuracy,
                "meanRating": mean_rating,
                "meanAwarenessRating": mean_awareness,
                "meanNaturalnessRating": mean_naturalness,
                "meanAnnoyanceRating": mean_annoyance,
                "meanConfidenceRating": mean_confidence,
                "playbackRate": playback_rate,
                "cueEffectiveness": cue_effectiveness,
            }
        )

    return output


def mean_positive_rating(rows, column):
    ratings = [
        parse_float(row.get(column), None)
        for row in rows
        if row.get(column)
    ]
    ratings = [value for value in ratings if value is not None and value > 0.0]
    return sum(ratings) / len(ratings) if ratings else 0.0


def calculate_direction_accuracy(rows):
    values = []
    for row in rows:
        direction_correct = str(row.get("directionCorrect", "")).strip()
        if direction_correct:
            values.append(parse_bool(direction_correct))
            continue

        response = row.get("directionResponse", "").strip()
        expected = row.get("expectedDirection", "").strip()
        if response and expected:
            values.append(response.lower() == expected.lower())

    if not values:
        return None

    return sum(1 for value in values if value) / len(values)


def best_cue_labels(effectiveness_rows):
    best_by_situation = {}
    for row in effectiveness_rows:
        key = (row["conditionLabel"], row["targetId"])
        current = best_by_situation.get(key)
        if current is None or row["cueEffectiveness"] > current["cueEffectiveness"]:
            best_by_situation[key] = row

    return [best_by_situation[key] for key in sorted(best_by_situation)]


def clamp01(value):
    return max(0.0, min(1.0, value))


def cue_type_from_candidate(cue_candidate):
    mapping = {
        "NoCue": "None",
        "PredictedCue": "",
        "Footstep": "Footstep",
        "Breathing": "AmbientPresence",
        "ClothRustle": "AmbientPresence",
        "Voice": "Voice",
        "AmbientPresence": "AmbientPresence",
        "MixedCue": "AmbientPresence",
    }
    return mapping.get(cue_candidate, cue_candidate)


def target_values_from_effectiveness(row, reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS, objective_only=False):
    mean_reaction = parse_float(row.get("meanReactionTime"), reaction_time_cap)
    normalized_reaction = min(mean_reaction, reaction_time_cap) / reaction_time_cap
    detection = parse_float(row.get("detectionSuccess"))
    direction = parse_float(row.get("directionAccuracy"), None)
    if direction is None:
        direction = parse_float(row.get("directionResponseRate"))
    if objective_only:
        awareness = 0.0
        rating = 0.0
    else:
        awareness = parse_float(row.get("meanAwarenessRating")) / 5.0
        rating = awareness if awareness > 0.0 else parse_float(row.get("meanRating")) / 5.0

    presence_score = clamp01(
        0.15
        + 0.35 * detection
        + 0.2 * direction
        + 0.25 * rating
        - 0.2 * normalized_reaction
    )

    cue_type = cue_type_from_candidate(row.get("cueCandidate", ""))
    if cue_type == "None":
        volume_gain = 0.0
    else:
        volume_gain = clamp01(0.2 + 0.75 * presence_score)

    return cue_type, presence_score, volume_gain


def summarize_source(source_path):
    rows = load_rows(source_path)
    summaries = apply_metadata_fallback(summarize(rows), source_path)
    if summaries:
        return summaries

    return [empty_summary(source_path)]


def empty_summary(source_path):
    metadata = parse_metadata_from_file_name(source_path)
    return {
        "metadata": metadata,
        "targetId": "(none)",
        "rows": 0,
        "duration": 0.0,
        "counts": {column: 0 for column in STATE_COLUMNS},
        "firstTimes": {column: None for column in STATE_COLUMNS},
        "approachToNear": None,
        "outOfViewApproaching": 0,
    }


def apply_metadata_fallback(summaries, source_path):
    fallback = parse_metadata_from_file_name(source_path)
    for item in summaries:
        for column in METADATA_COLUMNS:
            if not item["metadata"].get(column):
                item["metadata"][column] = fallback.get(column, "")

    return summaries


def parse_metadata_from_file_name(path):
    metadata = {column: "" for column in METADATA_COLUMNS}
    stem = path.stem
    prefix = "peripheral_state_log_"
    if not stem.startswith(prefix):
        return metadata

    parts = stem[len(prefix):].split("_")
    if len(parts) < 5:
        return metadata

    maybe_date = parts[-2]
    maybe_time = parts[-1]
    if not (maybe_date.isdigit() and len(maybe_date) == 8 and maybe_time.isdigit() and len(maybe_time) == 6):
        return metadata

    metadata["participantId"] = parts[0]
    metadata["conditionLabel"] = "_".join(parts[1:-3]) if len(parts) > 5 else parts[1]
    metadata["trialId"] = parts[-3]
    return metadata


def first_true_time(rows, column, time_column="time"):
    for row in rows:
        if parse_bool(row.get(column)):
            return parse_float(row.get(time_column))
    return None


def first_metadata(rows):
    metadata = {}
    first_row = rows[0] if rows else {}
    for column in METADATA_COLUMNS:
        metadata[column] = first_row.get(column, "")
    return metadata


def fmt_time(value):
    if value is None:
        return "-"
    return f"{value:.3f}s"


def print_summary(path, rows, summaries):
    print(f"CSV: {path}")
    print(f"Rows: {len(rows)}")
    print()

    for item in summaries:
        print(f"Target: {item['targetId']}")
        print(f"  rows: {item['rows']}")
        print(f"  duration: {item['duration']:.3f}s")
        print(f"  outOfView+approaching rows: {item['outOfViewApproaching']}")
        print(f"  approach -> near: {fmt_time(item['approachToNear'])}")
        print("  counts:")
        for column in STATE_COLUMNS:
            print(f"    {column}: {item['counts'][column]}")
        print("  first detected:")
        for column in STATE_COLUMNS:
            print(f"    {column}: {fmt_time(item['firstTimes'][column])}")
        print()


def write_summary_csv(source_path, summaries, output_path=None):
    if output_path is None:
        output_path = source_path.with_name(source_path.stem + "_summary.csv")

    fieldnames = [
        "participantId",
        "conditionLabel",
        "trialId",
        "targetId",
        "rows",
        "duration",
        "outOfViewApproaching",
        "approachToNear",
    ]
    fieldnames.extend(f"{column}Count" for column in STATE_COLUMNS)
    fieldnames.extend(f"{column}FirstTime" for column in STATE_COLUMNS)

    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()

        for item in apply_metadata_fallback(summaries, source_path):
            row = {
                "participantId": item["metadata"]["participantId"],
                "conditionLabel": item["metadata"]["conditionLabel"],
                "trialId": item["metadata"]["trialId"],
                "targetId": item["targetId"],
                "rows": item["rows"],
                "duration": f"{item['duration']:.3f}",
                "outOfViewApproaching": item["outOfViewApproaching"],
                "approachToNear": format_optional_float(item["approachToNear"]),
            }

            for column in STATE_COLUMNS:
                row[f"{column}Count"] = item["counts"][column]
                row[f"{column}FirstTime"] = format_optional_float(item["firstTimes"][column])

            writer.writerow(row)

    return output_path


def write_batch_summary_csv(log_dir=DEFAULT_LOG_DIR, output_path=None, min_duration=MIN_TRIAL_DURATION_SECONDS):
    if output_path is None:
        output_path = log_dir / "peripheral_batch_summary.csv"

    fieldnames = [
        "sourceCsv",
        "participantId",
        "conditionLabel",
        "trialId",
        "targetId",
        "demoCheck",
        "durationCheck",
        "rows",
        "duration",
        "outOfViewApproaching",
        "approachToNear",
    ]
    fieldnames.extend(f"{column}Count" for column in STATE_COLUMNS)
    fieldnames.extend(f"{column}FirstTime" for column in STATE_COLUMNS)

    source_paths = source_csv_paths(log_dir)
    if not source_paths:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")

    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()

        for source_path in source_paths:
            for item in summarize_source(source_path):
                row = {
                    "sourceCsv": source_path.name,
                    "participantId": item["metadata"]["participantId"],
                    "conditionLabel": item["metadata"]["conditionLabel"],
                    "trialId": item["metadata"]["trialId"],
                    "targetId": item["targetId"],
                    "demoCheck": demo_check(item),
                    "durationCheck": duration_check(item, min_duration),
                    "rows": item["rows"],
                    "duration": f"{item['duration']:.3f}",
                    "outOfViewApproaching": item["outOfViewApproaching"],
                    "approachToNear": format_optional_float(item["approachToNear"]),
                }

                for column in STATE_COLUMNS:
                    row[f"{column}Count"] = item["counts"][column]
                    row[f"{column}FirstTime"] = format_optional_float(item["firstTimes"][column])

                writer.writerow(row)

    return output_path, len(source_paths)


def collect_batch_rows(log_dir=DEFAULT_LOG_DIR, min_duration=MIN_TRIAL_DURATION_SECONDS):
    source_paths = source_csv_paths(log_dir)
    if not source_paths:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")

    rows = []
    for source_path in source_paths:
        for item in summarize_source(source_path):
            row = {
                "sourceCsv": source_path.name,
                "participantId": item["metadata"]["participantId"],
                "conditionLabel": item["metadata"]["conditionLabel"],
                "trialId": item["metadata"]["trialId"],
                "targetId": item["targetId"],
                "demoCheck": demo_check(item),
                "durationCheck": duration_check(item, min_duration),
                "rows": item["rows"],
                "duration": f"{item['duration']:.3f}",
                "outOfViewApproaching": item["outOfViewApproaching"],
                "approachToNear": format_optional_float(item["approachToNear"]),
            }

            for column in STATE_COLUMNS:
                row[f"{column}Count"] = item["counts"][column]
                row[f"{column}FirstTime"] = format_optional_float(item["firstTimes"][column])

            rows.append(row)

    return rows, len(source_paths)


def demo_check(summary):
    target_id = summary["targetId"]
    counts = summary["counts"]
    condition = summary["metadata"].get("conditionLabel", "")

    if condition == "None":
        return "OK" if summary["rows"] == 0 else "Check none"

    if target_id == "Target_Approach":
        return "OK" if counts["approaching"] > 0 and counts["near"] > 0 else "Check approach/near"

    if target_id == "Target_Back":
        return "OK" if summary["outOfViewApproaching"] > 0 else "Check rear approach"

    if target_id == "Target_Crossing":
        return "OK" if counts["crossing"] > 0 else "Check crossing"

    if target_id == "Target_Speaking":
        return "OK" if counts["speaking"] > 0 else "Check speaking"

    return "-"


def duration_check(summary, min_duration=MIN_TRIAL_DURATION_SECONDS):
    if summary["metadata"].get("conditionLabel") == "None" and summary["rows"] == 0:
        return "OK"

    duration = summary["duration"]
    if duration >= min_duration:
        return "OK"

    return f"Short (<{min_duration:g}s)"


def write_html_report(log_dir=DEFAULT_LOG_DIR, output_path=None, min_duration=MIN_TRIAL_DURATION_SECONDS):
    if output_path is None:
        output_path = log_dir / "peripheral_report.html"

    rows, file_count = collect_batch_rows(log_dir, min_duration)
    columns = [
        "sourceCsv",
        "participantId",
        "conditionLabel",
        "trialId",
        "targetId",
        "demoCheck",
        "durationCheck",
        "rows",
        "duration",
        "outOfViewApproaching",
        "approachToNear",
        "outOfViewCount",
        "approachingCount",
        "speakingCount",
        "gazingCount",
        "nearCount",
        "crossingCount",
    ]

    table_rows = []
    for row in rows:
        cells = "".join(format_html_cell(row, column) for column in columns)
        row_class = "ok" if row.get("demoCheck") == "OK" and row.get("durationCheck") == "OK" else "check"
        table_rows.append(f"<tr class=\"{row_class}\">{cells}</tr>")

    header_cells = "".join(f"<th>{html.escape(column)}</th>" for column in columns)
    document = f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Peripheral Research Report</title>
  <style>
    body {{
      font-family: Segoe UI, Arial, sans-serif;
      margin: 24px;
      color: #1f2933;
      background: #f7f9fb;
    }}
    h1 {{
      font-size: 24px;
      margin: 0 0 8px;
    }}
    .meta {{
      margin: 0 0 20px;
      color: #52606d;
    }}
    table {{
      border-collapse: collapse;
      width: 100%;
      background: white;
      font-size: 13px;
    }}
    th, td {{
      border: 1px solid #d9e2ec;
      padding: 7px 9px;
      text-align: right;
      white-space: nowrap;
    }}
    th {{
      background: #e6f6ff;
      color: #102a43;
      position: sticky;
      top: 0;
      z-index: 1;
    }}
    td:first-child, td:nth-child(2), th:first-child, th:nth-child(2) {{
      text-align: left;
    }}
    tr:nth-child(even) {{
      background: #f8fafc;
    }}
    tr.check td {{
      background: #fff8e6;
    }}
    .status-ok {{
      color: #0b6b3a;
      font-weight: 600;
    }}
    .status-check {{
      color: #9a3412;
      font-weight: 600;
    }}
  </style>
</head>
<body>
  <h1>Peripheral Research Report</h1>
  <p class="meta">Source CSV files: {file_count} / Rows: {len(rows)}</p>
  <table>
    <thead><tr>{header_cells}</tr></thead>
    <tbody>
      {''.join(table_rows)}
    </tbody>
  </table>
</body>
</html>
"""

    output_path.write_text(document, encoding="utf-8")
    return output_path, file_count, len(rows)


def write_cue_effectiveness_csv(
    source_path,
    rows,
    output_path=None,
    reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS,
    objective_only=False,
):
    if output_path is None:
        output_path = source_path.with_name(source_path.stem + "_cue_effectiveness.csv")

    fieldnames = [
        "conditionLabel",
        "targetId",
        "cueCandidate",
        "isBestCue",
        "rows",
        "detectionSuccess",
        "meanReactionTime",
        "directionResponseRate",
        "directionAccuracy",
        "meanRating",
        "meanAwarenessRating",
        "meanNaturalnessRating",
        "meanAnnoyanceRating",
        "meanConfidenceRating",
        "playbackRate",
        "cueEffectiveness",
    ]

    effectiveness = cue_effectiveness_rows(rows, reaction_time_cap, objective_only)
    best_keys = {
        (row["conditionLabel"], row["targetId"], row["cueCandidate"])
        for row in best_cue_labels(effectiveness)
    }

    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()
        for item in effectiveness:
            key = (item["conditionLabel"], item["targetId"], item["cueCandidate"])
            writer.writerow(
                {
                    "conditionLabel": item["conditionLabel"],
                    "targetId": item["targetId"],
                    "cueCandidate": item["cueCandidate"],
                    "isBestCue": key in best_keys,
                    "rows": item["rows"],
                    "detectionSuccess": f"{item['detectionSuccess']:.3f}",
                    "meanReactionTime": f"{item['meanReactionTime']:.3f}",
                    "directionResponseRate": f"{item['directionResponseRate']:.3f}",
                    "directionAccuracy": format_optional_float(item["directionAccuracy"]),
                    "meanRating": f"{item['meanRating']:.3f}",
                    "meanAwarenessRating": f"{item['meanAwarenessRating']:.3f}",
                    "meanNaturalnessRating": f"{item['meanNaturalnessRating']:.3f}",
                    "meanAnnoyanceRating": f"{item['meanAnnoyanceRating']:.3f}",
                    "meanConfidenceRating": f"{item['meanConfidenceRating']:.3f}",
                    "playbackRate": f"{item['playbackRate']:.3f}",
                    "cueEffectiveness": f"{item['cueEffectiveness']:.3f}",
                }
            )

    return output_path, len(effectiveness), len(best_keys)


def write_label_dataset_csv(
    source_path,
    rows,
    output_path=None,
    reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS,
    objective_only=False,
):
    if output_path is None:
        output_path = source_path.with_name(source_path.stem + "_cue_labels.csv")

    effectiveness = cue_effectiveness_rows(rows, reaction_time_cap, objective_only)
    labels = best_cue_labels(effectiveness)

    fieldnames = [
        "conditionLabel",
        "targetId",
        "cueCandidate",
        "cueType",
        "presenceScore",
        "volumeGain",
        "cueEffectiveness",
        "detectionSuccess",
        "meanReactionTime",
        "directionResponseRate",
        "directionAccuracy",
        "meanRating",
        "meanAwarenessRating",
        "meanNaturalnessRating",
        "meanAnnoyanceRating",
        "meanConfidenceRating",
        "rows",
    ]

    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()

        for item in labels:
            cue_type, presence_score, volume_gain = target_values_from_effectiveness(
                item,
                reaction_time_cap,
                objective_only,
            )
            writer.writerow(
                {
                    "conditionLabel": item["conditionLabel"],
                    "targetId": item["targetId"],
                    "cueCandidate": item["cueCandidate"],
                    "cueType": cue_type,
                    "presenceScore": f"{presence_score:.3f}",
                    "volumeGain": f"{volume_gain:.3f}",
                    "cueEffectiveness": f"{item['cueEffectiveness']:.3f}",
                    "detectionSuccess": f"{item['detectionSuccess']:.3f}",
                    "meanReactionTime": f"{item['meanReactionTime']:.3f}",
                    "directionResponseRate": f"{item['directionResponseRate']:.3f}",
                    "directionAccuracy": format_optional_float(item["directionAccuracy"]),
                    "meanRating": f"{item['meanRating']:.3f}",
                    "meanAwarenessRating": f"{item['meanAwarenessRating']:.3f}",
                    "meanNaturalnessRating": f"{item['meanNaturalnessRating']:.3f}",
                    "meanAnnoyanceRating": f"{item['meanAnnoyanceRating']:.3f}",
                    "meanConfidenceRating": f"{item['meanConfidenceRating']:.3f}",
                    "rows": item["rows"],
                }
            )

    return output_path, len(labels)


def write_batch_label_dataset_csv(
    log_dir=DEFAULT_LOG_DIR,
    output_path=None,
    reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS,
    objective_only=False,
):
    if output_path is None:
        output_path = Path(log_dir) / "peripheral_batch_cue_labels.csv"

    fieldnames = [
        "sourceCsv",
        "conditionLabel",
        "targetId",
        "cueCandidate",
        "cueType",
        "presenceScore",
        "volumeGain",
        "cueEffectiveness",
        "detectionSuccess",
        "meanReactionTime",
        "directionResponseRate",
        "directionAccuracy",
        "meanRating",
        "meanAwarenessRating",
        "meanNaturalnessRating",
        "meanAnnoyanceRating",
        "meanConfidenceRating",
        "rows",
    ]

    rows_by_source = source_rows_by_path(log_dir)
    if not rows_by_source:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")

    combined_labels = []
    for source_path, rows in rows_by_source:
        effectiveness = cue_effectiveness_rows(rows, reaction_time_cap, objective_only)
        labels = best_cue_labels(effectiveness)
        for item in labels:
            cue_type, presence_score, volume_gain = target_values_from_effectiveness(
                item,
                reaction_time_cap,
                objective_only,
            )
            combined_labels.append(
                {
                    "sourceCsv": source_path.name,
                    "conditionLabel": item["conditionLabel"],
                    "targetId": item["targetId"],
                    "cueCandidate": item["cueCandidate"],
                    "cueType": cue_type,
                    "presenceScore": f"{presence_score:.3f}",
                    "volumeGain": f"{volume_gain:.3f}",
                    "cueEffectiveness": f"{item['cueEffectiveness']:.3f}",
                    "detectionSuccess": f"{item['detectionSuccess']:.3f}",
                    "meanReactionTime": f"{item['meanReactionTime']:.3f}",
                    "directionResponseRate": f"{item['directionResponseRate']:.3f}",
                    "directionAccuracy": format_optional_float(item["directionAccuracy"]),
                    "meanRating": f"{item['meanRating']:.3f}",
                    "meanAwarenessRating": f"{item['meanAwarenessRating']:.3f}",
                    "meanNaturalnessRating": f"{item['meanNaturalnessRating']:.3f}",
                    "meanAnnoyanceRating": f"{item['meanAnnoyanceRating']:.3f}",
                    "meanConfidenceRating": f"{item['meanConfidenceRating']:.3f}",
                    "rows": item["rows"],
                }
            )

    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(combined_labels)

    return output_path, len(rows_by_source), len(combined_labels)


def write_cue_ranking_report(
    source_path,
    rows,
    output_path=None,
    reaction_time_cap=DEFAULT_REACTION_TIME_SECONDS,
    objective_only=False,
):
    if output_path is None:
        output_path = source_path.with_name(source_path.stem + "_cue_ranking_report.md")

    effectiveness = cue_effectiveness_rows(rows, reaction_time_cap, objective_only)
    grouped = defaultdict(list)
    for item in effectiveness:
        grouped[(item["conditionLabel"], item["targetId"])].append(item)

    lines = [
        "# Peripheral Cue Ranking Report",
        "",
        f"Source CSV: `{source_path.name}`",
        f"Reaction time cap: {reaction_time_cap:g}s",
        "",
        "Effectiveness formula:",
        "",
        "```text",
        "detectionSuccess",
        "+ directionAccuracy",
        "- normalizedReactionTime",
        "+ awarenessRating",
        "+ 0.5 * naturalnessRating",
        "+ 0.25 * confidenceRating",
        "- 0.5 * annoyanceRating",
        "```",
        "",
        "If separated ratings are missing, `subjectiveRating` is used as the awareness term.",
        "",
    ]

    if not grouped:
        lines.extend(["No cue candidate rows were found.", ""])
    else:
        lines.append("## Best Cue Labels")
        lines.append("")
        lines.append("| Condition | Target | Best cue | Score | Rows |")
        lines.append("| -- | -- | -- | --: | --: |")
        for key in sorted(grouped):
            ranked = sorted(grouped[key], key=lambda row: row["cueEffectiveness"], reverse=True)
            best = ranked[0]
            lines.append(
                "| "
                + " | ".join(
                    [
                        markdown_cell(best["conditionLabel"]),
                        markdown_cell(best["targetId"]),
                        markdown_cell(best["cueCandidate"]),
                        f"{best['cueEffectiveness']:.3f}",
                        str(best["rows"]),
                    ]
                )
                + " |"
            )

        lines.append("")
        lines.append("## Ranking By Situation")
        lines.append("")
        for key in sorted(grouped):
            condition, target_id = key
            ranked = sorted(grouped[key], key=lambda row: row["cueEffectiveness"], reverse=True)
            lines.append(f"### {condition} / {target_id}")
            lines.append("")
            lines.append(
                "| Rank | Cue | Score | Detection | Direction | RT(s) | Overall | Awareness | Naturalness | Annoyance | Confidence | Rows |"
            )
            lines.append("| --: | -- | --: | --: | --: | --: | --: | --: | --: | --: | --: | --: |")
            for rank, item in enumerate(ranked, start=1):
                direction = item["directionAccuracy"]
                if direction is None:
                    direction = item["directionResponseRate"]
                lines.append(
                    "| "
                    + " | ".join(
                        [
                            str(rank),
                            markdown_cell(item["cueCandidate"]),
                            f"{item['cueEffectiveness']:.3f}",
                            f"{item['detectionSuccess']:.3f}",
                            f"{direction:.3f}",
                            f"{item['meanReactionTime']:.3f}",
                            f"{item['meanRating']:.3f}",
                            f"{item['meanAwarenessRating']:.3f}",
                            f"{item['meanNaturalnessRating']:.3f}",
                            f"{item['meanAnnoyanceRating']:.3f}",
                            f"{item['meanConfidenceRating']:.3f}",
                            str(item["rows"]),
                        ]
                    )
                    + " |"
                )
            lines.append("")

    output_path.write_text("\n".join(lines), encoding="utf-8")
    return output_path, len(effectiveness), len(grouped)


def markdown_cell(value):
    return str(value).replace("|", "\\|")


def format_html_cell(row, column):
    value = str(row.get(column, ""))
    if column in ("demoCheck", "durationCheck"):
        class_name = "status-ok" if value == "OK" else "status-check"
        return f"<td class=\"{class_name}\">{html.escape(value)}</td>"

    return f"<td>{html.escape(value)}</td>"


def format_optional_float(value):
    if value is None:
        return ""
    return f"{value:.3f}"


def main():
    parser = argparse.ArgumentParser(description="Summarize Unity peripheral research CSV logs.")
    parser.add_argument(
        "csv_path",
        nargs="?",
        help="CSV file to analyze. If omitted, the latest peripheral_state_log*.csv is used.",
    )
    parser.add_argument(
        "--no-summary-csv",
        action="store_true",
        help="Print only; do not write a *_summary.csv file.",
    )
    parser.add_argument(
        "--summary-csv",
        help="Output path for the summary CSV. Defaults to <input>_summary.csv.",
    )
    parser.add_argument(
        "--batch",
        action="store_true",
        help="Summarize all peripheral source CSVs in the log directory.",
    )
    parser.add_argument(
        "--batch-summary-csv",
        help="Output path for --batch. Defaults to peripheral_batch_summary.csv in the log directory.",
    )
    parser.add_argument(
        "--html-report",
        action="store_true",
        help="Write an HTML report for all peripheral source CSVs.",
    )
    parser.add_argument(
        "--cue-effectiveness",
        action="store_true",
        help="Write cue-candidate effectiveness scores for a source CSV.",
    )
    parser.add_argument(
        "--cue-effectiveness-csv",
        help="Output path for --cue-effectiveness. Defaults to <input>_cue_effectiveness.csv.",
    )
    parser.add_argument(
        "--label-dataset",
        action="store_true",
        help="Write best-cue labels with presenceScore and volumeGain targets.",
    )
    parser.add_argument(
        "--label-dataset-csv",
        help="Output path for --label-dataset. Defaults to <input>_cue_labels.csv.",
    )
    parser.add_argument(
        "--batch-label-dataset",
        action="store_true",
        help="Write best-cue labels for every peripheral source CSV in the log directory.",
    )
    parser.add_argument(
        "--batch-label-dataset-csv",
        help="Output path for --batch-label-dataset. Defaults to peripheral_batch_cue_labels.csv in the log directory.",
    )
    parser.add_argument(
        "--cue-ranking-report",
        action="store_true",
        help="Write a Markdown report ranking cue candidates by condition and target.",
    )
    parser.add_argument(
        "--cue-ranking-report-path",
        help="Output path for --cue-ranking-report. Defaults to <input>_cue_ranking_report.md.",
    )
    parser.add_argument(
        "--reaction-time-cap",
        type=float,
        default=DEFAULT_REACTION_TIME_SECONDS,
        help="Reaction time cap used for cueEffectiveness normalization. Defaults to 10.",
    )
    parser.add_argument(
        "--objective-only",
        action="store_true",
        help="Build cue labels from objective metrics only. Subjective ratings are ignored.",
    )
    parser.add_argument(
        "--html-report-path",
        help="Output path for --html-report. Defaults to peripheral_report.html in the log directory.",
    )
    parser.add_argument(
        "--min-duration",
        type=float,
        default=MIN_TRIAL_DURATION_SECONDS,
        help="Minimum trial duration in seconds for durationCheck. Defaults to 10.",
    )
    parser.add_argument(
        "--log-dir",
        default=str(DEFAULT_LOG_DIR),
        help="Directory containing peripheral_state_log*.csv files.",
    )
    args = parser.parse_args()
    log_dir = Path(args.log_dir)

    if args.html_report:
        output_path = Path(args.html_report_path) if args.html_report_path else None
        written_path, file_count, row_count = write_html_report(log_dir, output_path, args.min_duration)
        print(f"HTML report: {written_path}")
        print(f"Source CSV files: {file_count}")
        print(f"Report rows: {row_count}")
        return

    if args.batch:
        output_path = Path(args.batch_summary_csv) if args.batch_summary_csv else None
        written_path, file_count = write_batch_summary_csv(log_dir, output_path, args.min_duration)
        print(f"Batch summary CSV: {written_path}")
        print(f"Source CSV files: {file_count}")
        return

    path = Path(args.csv_path) if args.csv_path else latest_csv_path(log_dir)
    rows = load_rows(path)

    if args.cue_effectiveness:
        output_path = Path(args.cue_effectiveness_csv) if args.cue_effectiveness_csv else None
        written_path, row_count, label_count = write_cue_effectiveness_csv(
            path,
            rows,
            output_path,
            args.reaction_time_cap,
            args.objective_only,
        )
        print(f"Cue effectiveness CSV: {written_path}")
        print(f"Cue candidate rows: {row_count}")
        print(f"Best cue labels: {label_count}")
        return

    if args.label_dataset:
        output_path = Path(args.label_dataset_csv) if args.label_dataset_csv else None
        written_path, label_count = write_label_dataset_csv(
            path,
            rows,
            output_path,
            args.reaction_time_cap,
            args.objective_only,
        )
        print(f"Label dataset CSV: {written_path}")
        print(f"Best cue labels: {label_count}")
        return

    if args.batch_label_dataset:
        output_path = Path(args.batch_label_dataset_csv) if args.batch_label_dataset_csv else None
        written_path, source_count, label_count = write_batch_label_dataset_csv(
            log_dir,
            output_path,
            args.reaction_time_cap,
            args.objective_only,
        )
        print(f"Batch label dataset CSV: {written_path}")
        print(f"Source CSV files: {source_count}")
        print(f"Best cue labels: {label_count}")
        return

    if args.cue_ranking_report:
        output_path = Path(args.cue_ranking_report_path) if args.cue_ranking_report_path else None
        written_path, row_count, situation_count = write_cue_ranking_report(
            path,
            rows,
            output_path,
            args.reaction_time_cap,
            args.objective_only,
        )
        print(f"Cue ranking report: {written_path}")
        print(f"Cue candidate rows: {row_count}")
        print(f"Situations: {situation_count}")
        return

    summaries = summarize_source(path)
    print_summary(path, rows, summaries)

    if not args.no_summary_csv:
        output_path = Path(args.summary_csv) if args.summary_csv else None
        written_path = write_summary_csv(path, summaries, output_path)
        print(f"Summary CSV: {written_path}")


if __name__ == "__main__":
    main()
