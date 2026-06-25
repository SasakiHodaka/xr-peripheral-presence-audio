import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


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


def analyze_file(path, by_condition):
    with path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            condition = row.get("condition", "") or "(none)"
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
    }


def print_summary(by_condition):
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

    for condition in sorted(by_condition):
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
        print(",".join(row))


def print_quality_checks(by_condition):
    print()
    print("quality_check,result,details")
    expected_conditions = {
        "TRADITIONAL",
        "DIRECTION_ONLY",
        "DIRECTION_DISTANCE",
        "DIRECTION_DISTANCE_SPEAKING",
        "FULL_SCENE_TOKEN",
    }
    found_conditions = set(by_condition.keys())
    missing = sorted(expected_conditions - found_conditions)
    print(
        "all_conditions_present,{0},{1}".format(
            "OK" if not missing else "CHECK",
            "missing=" + "|".join(missing) if missing else "all five conditions found",
        )
    )

    total_rows = sum(stats["rows"] for stats in by_condition.values())
    speaking_rows = sum(stats["speaking_rows"] for stats in by_condition.values())
    semantic_rows = sum(stats["semantic_rows"] for stats in by_condition.values())
    turn_holder_rows = sum(stats["turn_holder_rows"] for stats in by_condition.values())
    overlap_rows = sum(stats["overlap_rows"] for stats in by_condition.values())

    print(f"has_rows,{'OK' if total_rows > 0 else 'CHECK'},rows={total_rows}")
    print(f"has_speaking,{'OK' if speaking_rows > 0 else 'CHECK'},speaking_rows={speaking_rows}")
    print(f"has_semantics,{'OK' if semantic_rows > 0 else 'CHECK'},semantic_rows={semantic_rows}")
    print(f"has_turn_holder,{'OK' if turn_holder_rows > 0 else 'CHECK'},turn_holder_rows={turn_holder_rows}")
    print(f"has_overlap,{'OK' if overlap_rows > 0 else 'CHECK'},overlap_rows={overlap_rows}")


def main():
    if len(sys.argv) != 2:
        print("Usage: python Tools/analyze_token_logs.py <scene_tokens_csv_or_directory>")
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
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
