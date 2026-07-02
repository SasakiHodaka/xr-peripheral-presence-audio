import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


def iter_event_files(target):
    if target.is_dir():
        return sorted(target.glob("scene_token_events_*.csv"))
    return [target]


def parse_payload(value):
    result = {}
    if not value:
        return result

    for part in value.split(";"):
        if "=" not in part:
            continue
        key, item_value = part.split("=", 1)
        result[key] = item_value
    return result


def make_stats():
    return {
        "events": 0,
        "event_types": Counter(),
        "direction_responses": Counter(),
        "speaker_responses": Counter(),
        "direction_scored": 0,
        "direction_correct": 0,
        "direction_latency_sum": 0.0,
        "direction_latency_count": 0,
        "speaker_scored": 0,
        "speaker_correct": 0,
        "speaker_latency_sum": 0.0,
        "speaker_latency_count": 0,
        "ambiguous_responses": 0,
        "sessions": Counter(),
        "participants": Counter(),
        "trials": Counter(),
    }


def analyze_file(path, by_condition):
    with path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            event_type = row.get("eventType", "")
            payload = parse_payload(row.get("value", ""))
            condition = payload.get("condition", "(none)")
            stats = by_condition[condition]
            stats["events"] += 1
            stats["event_types"][event_type] += 1

            session_id = payload.get("sessionId", "")
            participant_id = payload.get("participantId", "")
            trial = payload.get("trial", "")
            response = payload.get("response", "")
            is_correct = payload.get("isCorrect", "").lower() == "true"
            ambiguous = payload.get("ambiguous", "").lower() == "true"
            response_latency = safe_float(payload.get("responseLatency", "-1"), -1.0)

            if session_id:
                stats["sessions"][session_id] += 1
            if participant_id:
                stats["participants"][participant_id] += 1
            if trial:
                stats["trials"][trial] += 1
            if event_type == "response_direction" and response:
                stats["direction_responses"][response] += 1
                if ambiguous:
                    stats["ambiguous_responses"] += 1
                else:
                    stats["direction_scored"] += 1
                    if response_latency >= 0:
                        stats["direction_latency_sum"] += response_latency
                        stats["direction_latency_count"] += 1
                    if is_correct:
                        stats["direction_correct"] += 1
            if event_type == "response_speaker" and response:
                stats["speaker_responses"][response] += 1
                if ambiguous:
                    stats["ambiguous_responses"] += 1
                else:
                    stats["speaker_scored"] += 1
                    if response_latency >= 0:
                        stats["speaker_latency_sum"] += response_latency
                        stats["speaker_latency_count"] += 1
                    if is_correct:
                        stats["speaker_correct"] += 1


def format_counter(counter):
    if not counter:
        return ""
    return "|".join(f"{key}:{counter[key]}" for key in sorted(counter))


def safe_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def accuracy(correct, scored):
    if scored == 0:
        return 0.0
    return 100.0 * correct / scored


def average_latency(total, count):
    if count == 0:
        return 0.0
    return total / count


def print_summary(by_condition):
    header = [
        "condition",
        "events",
        "sessions",
        "participants",
        "trials",
        "eventTypes",
        "directionResponses",
        "speakerResponses",
        "directionAccuracyPct",
        "speakerAccuracyPct",
        "directionLatencySec",
        "speakerLatencySec",
        "ambiguousResponses",
    ]
    print(",".join(header))
    for row in build_summary_rows(by_condition):
        print(",".join(row))


def build_summary_rows(by_condition):
    rows = []
    for condition in sorted(by_condition):
        stats = by_condition[condition]
        rows.append(
            [
                condition,
                str(stats["events"]),
                "|".join(sorted(stats["sessions"].keys())),
                "|".join(sorted(stats["participants"].keys())),
                "|".join(sorted(stats["trials"].keys())),
                format_counter(stats["event_types"]),
                format_counter(stats["direction_responses"]),
                format_counter(stats["speaker_responses"]),
                f"{accuracy(stats['direction_correct'], stats['direction_scored']):.2f}",
                f"{accuracy(stats['speaker_correct'], stats['speaker_scored']):.2f}",
                f"{average_latency(stats['direction_latency_sum'], stats['direction_latency_count']):.3f}",
                f"{average_latency(stats['speaker_latency_sum'], stats['speaker_latency_count']):.3f}",
                str(stats["ambiguous_responses"]),
            ]
        )
    return rows


def write_summary_csv(path, by_condition):
    header = [
        "condition",
        "events",
        "sessions",
        "participants",
        "trials",
        "eventTypes",
        "directionResponses",
        "speakerResponses",
        "directionAccuracyPct",
        "speakerAccuracyPct",
        "directionLatencySec",
        "speakerLatencySec",
        "ambiguousResponses",
    ]
    with path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(header)
        writer.writerows(build_summary_rows(by_condition))


def print_quality_checks(by_condition):
    print()
    print("quality_check,result,details")
    total_events = sum(stats["events"] for stats in by_condition.values())
    direction_responses = sum(sum(stats["direction_responses"].values()) for stats in by_condition.values())
    speaker_responses = sum(sum(stats["speaker_responses"].values()) for stats in by_condition.values())
    direction_scored = sum(stats["direction_scored"] for stats in by_condition.values())
    speaker_scored = sum(stats["speaker_scored"] for stats in by_condition.values())
    ambiguous_responses = sum(stats["ambiguous_responses"] for stats in by_condition.values())
    session_starts = sum(stats["event_types"].get("session_start", 0) for stats in by_condition.values())
    trial_starts = sum(stats["event_types"].get("trial_start", 0) for stats in by_condition.values())

    print(f"has_events,{'OK' if total_events > 0 else 'CHECK'},events={total_events}")
    print(f"has_session_start,{'OK' if session_starts > 0 else 'CHECK'},session_start={session_starts}")
    print(f"has_trial_start,{'OK' if trial_starts > 0 else 'CHECK'},trial_start={trial_starts}")
    print(
        f"has_direction_responses,{'OK' if direction_responses > 0 else 'CHECK'},response_direction={direction_responses}"
    )
    print(
        f"has_speaker_responses,{'OK' if speaker_responses > 0 else 'CHECK'},response_speaker={speaker_responses}"
    )
    print(
        f"has_scored_direction_responses,{'OK' if direction_scored > 0 else 'CHECK'},scored_direction={direction_scored}"
    )
    print(
        f"has_scored_speaker_responses,{'OK' if speaker_scored > 0 else 'CHECK'},scored_speaker={speaker_scored}"
    )
    print(f"ambiguous_responses,INFO,ambiguous={ambiguous_responses}")


def main():
    if len(sys.argv) not in (2, 3):
        print("Usage: python Tools/analyze_event_logs.py <scene_token_events_csv_or_directory> [summary_csv]")
        return 1

    target = Path(sys.argv[1])
    files = iter_event_files(target)
    if not files:
        print("No scene_token_events_*.csv files found.")
        return 1

    by_condition = defaultdict(make_stats)
    for path in files:
        analyze_file(path, by_condition)

    print_summary(by_condition)
    print_quality_checks(by_condition)
    if len(sys.argv) == 3:
        output_path = Path(sys.argv[2])
        write_summary_csv(output_path, by_condition)
        print()
        print(f"Wrote event summary CSV: {output_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
