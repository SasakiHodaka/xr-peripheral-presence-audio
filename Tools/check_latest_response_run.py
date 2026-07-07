import csv
import sys
from pathlib import Path


def latest_event_file(root):
    files = sorted(root.glob("scene_token_events_*.csv"), key=lambda path: path.stat().st_mtime, reverse=True)
    return files[0] if files else None


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


def main():
    if len(sys.argv) != 2:
        print("Usage: python Tools/check_latest_response_run.py <unity_log_directory>")
        return 1

    root = Path(sys.argv[1])
    if not root.exists() or not root.is_dir():
        print(f"Log directory not found: {root}")
        return 1

    path = latest_event_file(root)
    if path is None:
        print("No scene_token_events_*.csv files found.")
        return 1

    direction = 0
    speaker = 0
    scored_direction = 0
    scored_speaker = 0
    ambiguous = 0
    conditions = set()

    with path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            event_type = row.get("eventType", "")
            payload = parse_payload(row.get("value", ""))
            condition = payload.get("condition", "")
            is_ambiguous = payload.get("ambiguous", "").lower() == "true"

            if event_type == "response_direction":
                direction += 1
                conditions.add(condition)
                if is_ambiguous:
                    ambiguous += 1
                else:
                    scored_direction += 1

            if event_type == "response_speaker":
                speaker += 1
                conditions.add(condition)
                if is_ambiguous:
                    ambiguous += 1
                else:
                    scored_speaker += 1

    print(f"latest_event_file={path}")
    print(f"direction_responses={direction}")
    print(f"speaker_responses={speaker}")
    print(f"scored_direction_responses={scored_direction}")
    print(f"scored_speaker_responses={scored_speaker}")
    print(f"ambiguous_responses={ambiguous}")
    print("conditions=" + ("|".join(sorted(conditions)) if conditions else "(none)"))

    if direction == 0 or speaker == 0:
        print("result=CHECK missing response events")
        return 2

    if scored_direction == 0 or scored_speaker == 0:
        print("result=CHECK responses exist, but all were ambiguous")
        return 2

    print("result=OK response logging is ready for analysis")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
