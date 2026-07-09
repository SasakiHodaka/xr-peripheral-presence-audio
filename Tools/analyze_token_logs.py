import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


EXPECTED_SCRIPTED_SEMANTICS = {
    "QUESTION",
    "ANSWER",
    "INSTRUCTION",
    "WARNING",
    "AGREEMENT",
}

MAIN_CONDITION_ORDER = [
    "C1_TRADITIONAL",
    "C2_DIRECTION_DISTANCE",
    "C3_FULL_SCENE_TOKEN",
    "C4_SELECTED_SCENE_TOKEN",
]

CONDITION_ALIASES = {
    "TRADITIONAL": "C1_TRADITIONAL",
    "DIRECTION_DISTANCE": "C2_DIRECTION_DISTANCE",
    "FULL_SCENE_TOKEN": "C3_FULL_SCENE_TOKEN",
    "SELECTED_SCENE_TOKEN": "C4_SELECTED_SCENE_TOKEN",
}


def safe_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def percent(numerator, denominator):
    if denominator == 0:
        return 0.0
    return 100.0 * numerator / denominator


def most_common_label(counter):
    if not counter:
        return ""
    label, count = counter.most_common(1)[0]
    return f"{label}({count})"


def iter_token_files(target):
    if target.is_dir():
        return sorted(target.glob("scene_tokens_*.csv"))
    return [target]


def normalize_condition(value):
    condition = value or "(none)"
    return CONDITION_ALIASES.get(condition, condition)


def analyze_file(path, by_condition):
    with path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            condition = normalize_condition(row.get("condition", ""))
            stats = by_condition[condition]
            stats["rows"] += 1

            timestamp = safe_float(row.get("timestamp"))
            if stats["first_time"] is None or timestamp < stats["first_time"]:
                stats["first_time"] = timestamp
            if stats["last_time"] is None or timestamp > stats["last_time"]:
                stats["last_time"] = timestamp

            speaker_id = row.get("speakerId", "")
            session_id = row.get("sessionId", "")
            participant_id = row.get("participantId", "")
            trial_index = row.get("trialIndex", "")
            direction = row.get("direction", "")
            distance = row.get("distance", "")
            speaking_state = row.get("speakingState", "")
            turn_state = row.get("turnState", "")
            semantic_token = row.get("semanticToken", "")

            if session_id:
                stats["sessions"][session_id] += 1
            if participant_id:
                stats["participants"][participant_id] += 1
            if trial_index:
                stats["trials"][trial_index] += 1
            if speaker_id:
                stats["speakers"][speaker_id] += 1
            if direction:
                stats["directions"][direction] += 1
            if distance:
                stats["distances"][distance] += 1
            if turn_state:
                stats["turns"][turn_state] += 1
            if semantic_token:
                stats["semantics"][semantic_token] += 1

            if speaking_state == "SPEAKING":
                stats["speaking_rows"] += 1
            if turn_state == "TURN_HOLDER":
                stats["turn_holder_rows"] += 1
            if turn_state == "OVERLAPPER":
                stats["overlap_rows"] += 1
            if semantic_token and semantic_token != "NONE":
                stats["semantic_rows"] += 1

            if speaker_id:
                speaker_stats = stats["by_speaker"][speaker_id]
                speaker_stats["rows"] += 1
                if direction:
                    speaker_stats["directions"][direction] += 1
                if distance:
                    speaker_stats["distances"][distance] += 1
                if turn_state:
                    speaker_stats["turns"][turn_state] += 1
                if semantic_token:
                    speaker_stats["semantics"][semantic_token] += 1
                if speaking_state == "SPEAKING":
                    speaker_stats["speaking_rows"] += 1
                if semantic_token and semantic_token != "NONE":
                    speaker_stats["semantic_rows"] += 1


def make_stats():
    return {
        "rows": 0,
        "speaking_rows": 0,
        "turn_holder_rows": 0,
        "overlap_rows": 0,
        "semantic_rows": 0,
        "first_time": None,
        "last_time": None,
        "sessions": Counter(),
        "participants": Counter(),
        "trials": Counter(),
        "speakers": Counter(),
        "directions": Counter(),
        "distances": Counter(),
        "turns": Counter(),
        "semantics": Counter(),
        "by_speaker": defaultdict(make_speaker_stats),
    }


def make_speaker_stats():
    return {
        "rows": 0,
        "speaking_rows": 0,
        "semantic_rows": 0,
        "directions": Counter(),
        "distances": Counter(),
        "turns": Counter(),
        "semantics": Counter(),
    }


def print_summary(by_condition):
    rows = build_summary_rows(by_condition)
    header = [
        "condition",
        "rows",
        "durationSec",
        "speakingPct",
        "semanticPct",
        "turnHolderRows",
        "overlapRows",
        "sessions",
        "participants",
        "trials",
        "speakers",
        "topDirection",
        "topDistance",
        "topTurn",
        "topSemantic",
    ]
    print(",".join(header))

    for row in rows:
        print(",".join(row))


def build_summary_rows(by_condition):
    rows = []
    for condition in ordered_conditions(by_condition):
        stats = by_condition[condition]
        first_time = stats["first_time"]
        last_time = stats["last_time"]
        duration = 0.0
        if first_time is not None and last_time is not None:
            duration = max(0.0, last_time - first_time)

        row = [
            condition,
            str(stats["rows"]),
            f"{duration:.2f}",
            f"{percent(stats['speaking_rows'], stats['rows']):.2f}",
            f"{percent(stats['semantic_rows'], stats['rows']):.2f}",
            str(stats["turn_holder_rows"]),
            str(stats["overlap_rows"]),
            "|".join(sorted(stats["sessions"].keys())),
            "|".join(sorted(stats["participants"].keys())),
            "|".join(sorted(stats["trials"].keys())),
            "|".join(sorted(stats["speakers"].keys())),
            most_common_label(stats["directions"]),
            most_common_label(stats["distances"]),
            most_common_label(stats["turns"]),
            most_common_label(stats["semantics"]),
        ]
        rows.append(row)
    return rows


def write_summary_csv(path, by_condition):
    header = [
        "condition",
        "rows",
        "durationSec",
        "speakingPct",
        "semanticPct",
        "turnHolderRows",
        "overlapRows",
        "sessions",
        "participants",
        "trials",
        "speakers",
        "topDirection",
        "topDistance",
        "topTurn",
        "topSemantic",
    ]
    with path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(header)
        writer.writerows(build_summary_rows(by_condition))


def build_speaker_rows(by_condition):
    rows = []
    for condition in ordered_conditions(by_condition):
        for speaker_id in sorted(by_condition[condition]["by_speaker"]):
            stats = by_condition[condition]["by_speaker"][speaker_id]
            rows.append(
                [
                    condition,
                    speaker_id,
                    str(stats["rows"]),
                    f"{percent(stats['speaking_rows'], stats['rows']):.2f}",
                    f"{percent(stats['semantic_rows'], stats['rows']):.2f}",
                    most_common_label(stats["directions"]),
                    most_common_label(stats["distances"]),
                    most_common_label(stats["turns"]),
                    most_common_label(stats["semantics"]),
                ]
            )
    return rows


def print_speaker_summary(by_condition):
    print()
    print(
        "condition,speakerId,rows,speakingPct,semanticPct,topDirection,topDistance,topTurn,topSemantic"
    )
    for row in build_speaker_rows(by_condition):
        print(",".join(row))


def write_speaker_csv(path, by_condition):
    header = [
        "condition",
        "speakerId",
        "rows",
        "speakingPct",
        "semanticPct",
        "topDirection",
        "topDistance",
        "topTurn",
        "topSemantic",
    ]
    with path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(header)
        writer.writerows(build_speaker_rows(by_condition))


def print_quality_checks(by_condition):
    print()
    print("quality_check,result,details")
    expected_conditions = {
        "C1_TRADITIONAL",
        "C2_DIRECTION_DISTANCE",
        "C3_FULL_SCENE_TOKEN",
        "C4_SELECTED_SCENE_TOKEN",
    }
    found_conditions = set(by_condition.keys())
    missing = sorted(expected_conditions - found_conditions)
    print(
        "all_conditions_present,{0},{1}".format(
            "OK" if not missing else "CHECK",
            "missing=" + "|".join(missing) if missing else "all main conditions found",
        )
    )

    total_rows = sum(stats["rows"] for stats in by_condition.values())
    speaking_rows = sum(stats["speaking_rows"] for stats in by_condition.values())
    semantic_rows = sum(stats["semantic_rows"] for stats in by_condition.values())
    turn_holder_rows = sum(stats["turn_holder_rows"] for stats in by_condition.values())
    overlap_rows = sum(stats["overlap_rows"] for stats in by_condition.values())
    found_semantics = {
        semantic
        for stats in by_condition.values()
        for semantic in stats["semantics"]
        if semantic and semantic != "NONE"
    }
    missing_semantics = sorted(EXPECTED_SCRIPTED_SEMANTICS - found_semantics)

    print(f"has_rows,{'OK' if total_rows > 0 else 'CHECK'},rows={total_rows}")
    print(f"has_speaking,{'OK' if speaking_rows > 0 else 'CHECK'},speaking_rows={speaking_rows}")
    print(f"has_semantics,{'OK' if semantic_rows > 0 else 'CHECK'},semantic_rows={semantic_rows}")
    print(
        "scripted_semantics_present,{0},{1}".format(
            "OK" if not missing_semantics else "CHECK",
            "missing=" + "|".join(missing_semantics) if missing_semantics else "all scripted labels found",
        )
    )
    print(f"has_turn_holder,{'OK' if turn_holder_rows > 0 else 'CHECK'},turn_holder_rows={turn_holder_rows}")
    print(f"has_overlap,{'OK' if overlap_rows > 0 else 'CHECK'},overlap_rows={overlap_rows}")


def speaker_csv_path(summary_path):
    return summary_path.with_name(summary_path.stem + "_by_speaker" + summary_path.suffix)


def ordered_conditions(by_condition):
    seen = set()
    ordered = []
    for condition in MAIN_CONDITION_ORDER:
        if condition in by_condition:
            ordered.append(condition)
            seen.add(condition)

    for condition in sorted(by_condition):
        if condition not in seen:
            ordered.append(condition)

    return ordered


def main():
    if len(sys.argv) not in (2, 3):
        print("Usage: python Tools/analyze_token_logs.py <scene_tokens_csv_or_directory> [summary_csv]")
        return 1

    target = Path(sys.argv[1])
    files = iter_token_files(target)
    if not files:
        print("No scene_tokens_*.csv files found.")
        return 1

    by_condition = defaultdict(make_stats)
    for path in files:
        analyze_file(path, by_condition)

    print_summary(by_condition)
    print_quality_checks(by_condition)
    print_speaker_summary(by_condition)
    if len(sys.argv) == 3:
        output_path = Path(sys.argv[2])
        speaker_path = speaker_csv_path(output_path)
        write_summary_csv(output_path, by_condition)
        write_speaker_csv(speaker_path, by_condition)
        print()
        print(f"Wrote token summary CSV: {output_path}")
        print(f"Wrote token speaker summary CSV: {speaker_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
