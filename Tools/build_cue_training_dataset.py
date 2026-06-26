#!/usr/bin/env python
import argparse
import csv
from pathlib import Path


DEFAULT_LOG_DIR = Path.home() / "AppData" / "LocalLow" / "DefaultCompany" / "My project"
DEFAULT_OUTPUT_NAME = "cue_training_dataset.csv"

FEATURE_COLUMNS = [
    "conditionLabel",
    "cueCondition",
    "roomScale",
    "materialClass",
    "environmentReverbAmount",
    "environmentOcclusionStrength",
    "environmentDistanceAttenuation",
    "environmentRt60",
    "environmentDrr",
    "targetId",
    "outOfView",
    "approaching",
    "speaking",
    "gazing",
    "near",
    "crossing",
    "distance",
    "viewAngle",
    "radialSpeed",
    "lateralSpeed",
    "localX",
    "localY",
    "localZ",
]

TARGET_COLUMNS = [
    "cueType",
    "presenceScore",
    "volumeGain",
    "cueLowPassHz",
    "cueReverbAmount",
    "cueOcclusionGain",
]


def is_source_log(path):
    return (
        path.name.startswith("peripheral_state_log")
        and path.suffix.lower() == ".csv"
        and not path.stem.endswith("_summary")
        and path.name != "peripheral_batch_summary.csv"
    )


def source_csv_paths(log_dir):
    return sorted(
        (path for path in log_dir.glob("peripheral_state_log*.csv") if is_source_log(path)),
        key=lambda path: path.stat().st_mtime,
    )


def read_rows(path):
    with path.open("r", encoding="utf-8-sig", newline="") as file:
        return list(csv.DictReader(file))


def build_dataset_rows(source_paths, include_none=False):
    rows = []
    for source_path in source_paths:
        for row in read_rows(source_path):
            targets = build_target_values(row)
            cue_type = targets["cueType"]
            if not include_none and cue_type == "None":
                continue

            dataset_row = {"sourceCsv": source_path.name}
            for column in FEATURE_COLUMNS:
                dataset_row[column] = row.get(column, default_feature(column))

            for column in TARGET_COLUMNS:
                dataset_row[column] = row.get(column) or targets[column]

            rows.append(dataset_row)

    return rows


def build_target_values(row):
    out_of_view = parse_bool(row.get("outOfView"))
    approaching = parse_bool(row.get("approaching"))
    speaking = parse_bool(row.get("speaking"))
    near = parse_bool(row.get("near"))
    crossing = parse_bool(row.get("crossing"))
    distance = parse_float(row.get("distance"))

    near_distance = 1.0
    far_distance = 5.0
    near_factor = 1.0 - inverse_lerp(near_distance, far_distance, distance)
    score = clamp01(near_factor)

    if out_of_view:
        score += 0.25
    if approaching:
        score += 0.35
    if speaking:
        score += 0.35
    if crossing:
        score += 0.2
    if near:
        score += 0.15

    score = clamp01(score)
    cue_type = select_cue_type(speaking, approaching, crossing, out_of_view, near, score)
    volume_gain = 0.0 if cue_type == "None" else score

    return {
        "cueType": cue_type,
        "presenceScore": f"{0.0 if cue_type == 'None' else score:.6f}",
        "volumeGain": f"{volume_gain:.6f}",
        "cueLowPassHz": "22000",
        "cueReverbAmount": "0",
        "cueOcclusionGain": "1",
    }


def default_feature(column):
    defaults = {
        "cueCondition": "StateBasedCue",
        "roomScale": "1",
        "materialClass": "Neutral",
        "environmentReverbAmount": "0",
        "environmentOcclusionStrength": "0",
        "environmentDistanceAttenuation": "0",
        "environmentRt60": "0",
        "environmentDrr": "0",
    }
    return defaults.get(column, "")


def select_cue_type(speaking, approaching, crossing, out_of_view, near, score):
    if score <= 0.05:
        return "None"
    if speaking:
        return "Voice"
    if approaching or crossing:
        return "Footstep"
    if out_of_view or near:
        return "AmbientPresence"
    return "None"


def parse_bool(value):
    return str(value).strip().lower() == "true"


def parse_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def inverse_lerp(start, end, value):
    if abs(end - start) < 1e-9:
        return 0.0
    return clamp01((value - start) / (end - start))


def clamp01(value):
    return max(0.0, min(1.0, value))


def write_dataset(rows, output_path):
    fieldnames = ["sourceCsv"] + FEATURE_COLUMNS + TARGET_COLUMNS
    with output_path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def main():
    parser = argparse.ArgumentParser(description="Build a compact cue-control training dataset from Unity peripheral logs.")
    parser.add_argument(
        "--log-dir",
        default=str(DEFAULT_LOG_DIR),
        help="Directory containing peripheral_state_log*.csv files.",
    )
    parser.add_argument(
        "--output",
        default=DEFAULT_OUTPUT_NAME,
        help="Output CSV path. Defaults to cue_training_dataset.csv in the current directory.",
    )
    parser.add_argument(
        "--include-none",
        action="store_true",
        help="Keep rows where cueType is None. By default, silent cue rows are skipped.",
    )
    args = parser.parse_args()

    log_dir = Path(args.log_dir)
    output_path = Path(args.output)
    source_paths = source_csv_paths(log_dir)
    if not source_paths:
        raise FileNotFoundError(f"No peripheral CSV files found in {log_dir}")

    rows = build_dataset_rows(source_paths, args.include_none)
    write_dataset(rows, output_path)
    print(f"Training dataset: {output_path}")
    print(f"Source CSV files: {len(source_paths)}")
    print(f"Rows: {len(rows)}")


if __name__ == "__main__":
    main()
