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


def is_source_log(path):
    return (
        path.name.startswith("peripheral_state_log")
        and path.suffix.lower() == ".csv"
        and not path.stem.endswith("_summary")
        and path.name != "peripheral_batch_summary.csv"
    )


def source_csv_paths(log_dir=DEFAULT_LOG_DIR):
    return sorted(
        (path for path in log_dir.glob("peripheral_state_log*.csv") if is_source_log(path)),
        key=lambda path: path.stat().st_mtime,
    )


def latest_csv_path():
    files = list(reversed(source_csv_paths()))
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


def write_summary_csv(source_path, summaries, output_path=None):
    if output_path is None:
        output_path = source_path.with_name(source_path.stem + "_summary.csv")

    fieldnames = [
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

        for item in summaries:
            row = {
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


def write_batch_summary_csv(log_dir=DEFAULT_LOG_DIR, output_path=None):
    if output_path is None:
        output_path = log_dir / "peripheral_batch_summary.csv"

    fieldnames = [
        "sourceCsv",
        "targetId",
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
            rows = load_rows(source_path)
            for item in summarize(rows):
                row = {
                    "sourceCsv": source_path.name,
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

    return output_path, len(source_paths)


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
    args = parser.parse_args()

    if args.batch:
        output_path = Path(args.batch_summary_csv) if args.batch_summary_csv else None
        written_path, file_count = write_batch_summary_csv(DEFAULT_LOG_DIR, output_path)
        print(f"Batch summary CSV: {written_path}")
        print(f"Source CSV files: {file_count}")
        return

    path = Path(args.csv_path) if args.csv_path else latest_csv_path()
    rows = load_rows(path)
    summaries = summarize(rows)
    print_summary(path, rows, summaries)

    if not args.no_summary_csv:
        output_path = Path(args.summary_csv) if args.summary_csv else None
        written_path = write_summary_csv(path, summaries, output_path)
        print(f"Summary CSV: {written_path}")


if __name__ == "__main__":
    main()
