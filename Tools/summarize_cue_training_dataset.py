#!/usr/bin/env python
import argparse
import csv
from collections import Counter, defaultdict
from pathlib import Path


DEFAULT_DATASET = Path("cue_training_dataset.csv")
CLASS_COLUMNS = ("cueType", "conditionLabel", "cueCondition", "materialClass", "targetId")
NUMERIC_TARGET_COLUMNS = (
    "roomScale",
    "environmentReverbAmount",
    "environmentOcclusionStrength",
    "environmentDistanceAttenuation",
    "environmentRt60",
    "environmentDrr",
    "presenceScore",
    "volumeGain",
    "cueLowPassHz",
    "cueReverbAmount",
    "cueOcclusionGain",
)


def parse_float(value):
    try:
        return float(value)
    except (TypeError, ValueError):
        return None


def load_rows(path):
    with path.open("r", encoding="utf-8-sig", newline="") as file:
        return list(csv.DictReader(file))


def summarize(rows):
    summary = {
        "rows": len(rows),
        "counts": {},
        "numeric": {},
    }

    for column in CLASS_COLUMNS:
        summary["counts"][column] = Counter(row.get(column, "") for row in rows)

    for column in NUMERIC_TARGET_COLUMNS:
        values = [parse_float(row.get(column)) for row in rows]
        values = [value for value in values if value is not None]
        if values:
            summary["numeric"][column] = {
                "min": min(values),
                "max": max(values),
                "mean": sum(values) / len(values),
                "unique": len(set(values)),
            }
        else:
            summary["numeric"][column] = {
                "min": None,
                "max": None,
                "mean": None,
                "unique": 0,
            }

    return summary


def print_summary(summary):
    print(f"Rows: {summary['rows']}")
    print()

    for column, counts in summary["counts"].items():
        print(f"{column}:")
        for value, count in counts.most_common():
            label = value if value else "(empty)"
            print(f"  {label}: {count}")
        print()

    print("Numeric targets:")
    for column, stats in summary["numeric"].items():
        print(
            f"  {column}: "
            f"min={format_value(stats['min'])}, "
            f"max={format_value(stats['max'])}, "
            f"mean={format_value(stats['mean'])}, "
            f"unique={stats['unique']}"
        )


def format_value(value):
    if value is None:
        return "n/a"
    return f"{value:.4f}"


def main():
    parser = argparse.ArgumentParser(description="Summarize cue training dataset contents.")
    parser.add_argument("--dataset", default=str(DEFAULT_DATASET), help="Input cue_training_dataset.csv path.")
    args = parser.parse_args()

    rows = load_rows(Path(args.dataset))
    print_summary(summarize(rows))


if __name__ == "__main__":
    main()
