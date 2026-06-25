import csv
import sys
from collections import defaultdict
from pathlib import Path


def average(values):
    return sum(values) / len(values) if values else 0.0


def main():
    if len(sys.argv) != 2:
        print("Usage: python Tools/analyze_scene_token_logs.py <metrics_csv_or_directory>")
        return 1

    target = Path(sys.argv[1])
    files = sorted(target.glob("scene_token_metrics_*.csv")) if target.is_dir() else [target]
    if not files:
        print("No scene_token_metrics_*.csv files found.")
        return 1

    by_condition = defaultdict(lambda: defaultdict(list))

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = row.get("condition", "")
                if not condition:
                    continue
                for key in (
                    "tokensPerSecond",
                    "jsonBytesPerSecond",
                    "compactBytesPerSecond",
                    "objectMetadataBytesPerSecond",
                    "compactSavingsRatio",
                ):
                    by_condition[condition][key].append(float(row[key]))

    print("condition,tokensPerSecond,jsonBytesPerSecond,compactBytesPerSecond,objectMetadataBytesPerSecond,compactSavingsRatio")
    for condition in sorted(by_condition):
        values = by_condition[condition]
        print(
            ",".join(
                [
                    condition,
                    f"{average(values['tokensPerSecond']):.2f}",
                    f"{average(values['jsonBytesPerSecond']):.2f}",
                    f"{average(values['compactBytesPerSecond']):.2f}",
                    f"{average(values['objectMetadataBytesPerSecond']):.2f}",
                    f"{average(values['compactSavingsRatio']):.4f}",
                ]
            )
        )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
