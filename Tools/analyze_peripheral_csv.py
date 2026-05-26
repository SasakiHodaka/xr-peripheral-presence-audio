#!/usr/bin/env python
import argparse
import csv
from collections import defaultdict
from pathlib import Path


DEFAULT_LOG_DIR = Path.home() / "AppData" / "LocalLow" / "DefaultCompany" / "My project"
STATE_COLUMNS = ("outOfView", "approaching", "speaking", "gazing", "near", "crossing")


def parse_bool(value):
    return str(value).strip().lower() == "true"


def parse_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def latest_csv_path():
    files = sorted(
        DEFAULT_LOG_DIR.glob("peripheral_state_log*.csv"),
        key=lambda path: path.stat().st_mtime,
        reverse=True,
    )
    if not files:
        raise FileNotFoundError(f"No peripheral CSV files found in {DEFAULT_LOG_DIR}")
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
        times = [parse_float(row.get("time")) for row in target_rows]
        duration = max(times) - min(times) if times else 0.0

        counts = {
            column: sum(1 for row in target_rows if parse_bool(row.get(column)))
            for column in STATE_COLUMNS
        }
        first_times = {
            column: first_true_time(target_rows, column)
            for column in STATE_COLUMNS
        }

        approach_to_near = None
        first_approach = first_times["approaching"]
        if first_approach is not None:
            for row in target_rows:
                time = parse_float(row.get("time"))
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


def first_true_time(rows, column):
    for row in rows:
        if parse_bool(row.get(column)):
            return parse_float(row.get("time"))
    return None


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


def main():
    parser = argparse.ArgumentParser(description="Summarize Unity peripheral research CSV logs.")
    parser.add_argument(
        "csv_path",
        nargs="?",
        help="CSV file to analyze. If omitted, the latest peripheral_state_log*.csv is used.",
    )
    args = parser.parse_args()

    path = Path(args.csv_path) if args.csv_path else latest_csv_path()
    rows = load_rows(path)
    summaries = summarize(rows)
    print_summary(path, rows, summaries)


if __name__ == "__main__":
    main()
