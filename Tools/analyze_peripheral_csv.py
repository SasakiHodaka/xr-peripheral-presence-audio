#!/usr/bin/env python
import argparse
import csv
import html
from collections import defaultdict
from pathlib import Path


DEFAULT_LOG_DIR = Path.home() / "AppData" / "LocalLow" / "DefaultCompany" / "My project"
STATE_COLUMNS = ("outOfView", "approaching", "speaking", "gazing", "near", "crossing")
METADATA_COLUMNS = ("participantId", "conditionLabel", "trialId")


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


def first_true_time(rows, column):
    for row in rows:
        if parse_bool(row.get(column)):
            return parse_float(row.get("time"))
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


def write_batch_summary_csv(log_dir=DEFAULT_LOG_DIR, output_path=None):
    if output_path is None:
        output_path = log_dir / "peripheral_batch_summary.csv"

    fieldnames = [
        "sourceCsv",
        "participantId",
        "conditionLabel",
        "trialId",
        "targetId",
        "demoCheck",
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
            for item in apply_metadata_fallback(summarize(rows), source_path):
                row = {
                    "sourceCsv": source_path.name,
                    "participantId": item["metadata"]["participantId"],
                    "conditionLabel": item["metadata"]["conditionLabel"],
                    "trialId": item["metadata"]["trialId"],
                    "targetId": item["targetId"],
                    "demoCheck": demo_check(item),
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


def collect_batch_rows(log_dir=DEFAULT_LOG_DIR):
    source_paths = source_csv_paths(log_dir)
    if not source_paths:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")

    rows = []
    for source_path in source_paths:
        source_rows = load_rows(source_path)
        for item in apply_metadata_fallback(summarize(source_rows), source_path):
            row = {
                "sourceCsv": source_path.name,
                "participantId": item["metadata"]["participantId"],
                "conditionLabel": item["metadata"]["conditionLabel"],
                "trialId": item["metadata"]["trialId"],
                "targetId": item["targetId"],
                "demoCheck": demo_check(item),
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

    if target_id == "Target_Approach":
        return "OK" if counts["approaching"] > 0 and counts["near"] > 0 else "Check approach/near"

    if target_id == "Target_Back":
        return "OK" if summary["outOfViewApproaching"] > 0 else "Check rear approach"

    if target_id == "Target_Crossing":
        return "OK" if counts["crossing"] > 0 else "Check crossing"

    if target_id == "Target_Speaking":
        return "OK" if counts["speaking"] > 0 else "Check speaking"

    return "-"


def write_html_report(log_dir=DEFAULT_LOG_DIR, output_path=None):
    if output_path is None:
        output_path = log_dir / "peripheral_report.html"

    rows, file_count = collect_batch_rows(log_dir)
    columns = [
        "sourceCsv",
        "participantId",
        "conditionLabel",
        "trialId",
        "targetId",
        "demoCheck",
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
        row_class = "ok" if row.get("demoCheck") == "OK" else "check"
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


def format_html_cell(row, column):
    value = str(row.get(column, ""))
    if column == "demoCheck":
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
        "--html-report-path",
        help="Output path for --html-report. Defaults to peripheral_report.html in the log directory.",
    )
    args = parser.parse_args()

    if args.html_report:
        output_path = Path(args.html_report_path) if args.html_report_path else None
        written_path, file_count, row_count = write_html_report(DEFAULT_LOG_DIR, output_path)
        print(f"HTML report: {written_path}")
        print(f"Source CSV files: {file_count}")
        print(f"Report rows: {row_count}")
        return

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
