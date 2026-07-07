import csv
import sys
from collections import defaultdict
from pathlib import Path


def average(values):
    return sum(values) / len(values) if values else 0.0


BASE_KEYS = (
    "tokensPerSecond",
    "jsonBytesPerSecond",
    "compactBytesPerSecond",
    "objectMetadataBytesPerSecond",
    "compactSavingsRatio",
)

SELECTION_KEYS = (
    "generatedTokensPerSecond",
    "selectedTokensPerSecond",
    "selectedJsonBytesPerSecond",
    "selectedCompactBytesPerSecond",
    "tokenDropRatio",
    "importantTokenSendRatio",
    "selectionSavingsRatio",
)


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
                for key in BASE_KEYS + SELECTION_KEYS:
                    if key in row and row[key] not in ("", None):
                        by_condition[condition][key].append(float(row[key]))

    header = ["condition"] + list(BASE_KEYS) + list(SELECTION_KEYS)
    print(",".join(header))
    for condition in sorted(by_condition):
        values = by_condition[condition]
        row = [condition]
        for key in BASE_KEYS + SELECTION_KEYS:
            precision = 4 if key.endswith("Ratio") else 2
            row.append(f"{average(values[key]):.{precision}f}")
        print(",".join(row))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
