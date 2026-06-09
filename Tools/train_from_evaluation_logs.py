#!/usr/bin/env python
import csv
import argparse
import subprocess
import sys
from pathlib import Path


DEFAULT_REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_LOG_DIR = Path.home() / "AppData" / "LocalLow" / "DefaultCompany" / "My project"
DEFAULT_LABEL_OUTPUT = DEFAULT_LOG_DIR / "peripheral_batch_cue_labels.csv"
DEFAULT_FALLBACK_DATASET = Path("cue_training_dataset.csv")
DEFAULT_MODEL_PATH = Path("Assets") / "Models" / "cue_model_unity.json"
DEFAULT_PREDICTIONS_PATH = Path("cue_training_predictions_evaluation.csv")


def run_command(command, cwd):
    print("Running:", " ".join(str(part) for part in command))
    subprocess.run(command, cwd=str(cwd), check=True)


def data_row_count(path):
    if not path.exists():
        return 0

    with path.open("r", encoding="utf-8-sig", newline="") as file:
        reader = csv.reader(file)
        row_count = sum(1 for _ in reader)

    return max(0, row_count - 1)


def main():
    parser = argparse.ArgumentParser(
        description="Build objective cue labels from evaluation logs and train the cue-control model."
    )
    parser.add_argument(
        "--log-dir",
        default=str(DEFAULT_LOG_DIR),
        help="Directory containing peripheral_state_log*.csv files.",
    )
    parser.add_argument(
        "--label-output",
        default=str(DEFAULT_LABEL_OUTPUT),
        help="Output CSV for the combined objective label dataset.",
    )
    parser.add_argument(
        "--model",
        default=str(DEFAULT_MODEL_PATH),
        help="Output model JSON path.",
    )
    parser.add_argument(
        "--predictions",
        default=str(DEFAULT_PREDICTIONS_PATH),
        help="Output predictions CSV path.",
    )
    parser.add_argument(
        "--subjective",
        action="store_true",
        help="Include subjective ratings when building cue labels.",
    )
    parser.add_argument("--test-ratio", type=float, default=0.25, help="Held-out test split ratio.")
    parser.add_argument("--seed", type=int, default=7, help="Random seed for train/test split.")
    parser.add_argument("--epochs", type=int, default=80, help="Regression training epochs.")
    parser.add_argument(
        "--classifier-epochs",
        type=int,
        default=220,
        help="Gradient-descent epochs for cueType classification.",
    )
    parser.add_argument("--classifier", choices=("linear", "centroid"), default="linear", help="cueType classifier.")
    parser.add_argument(
        "--class-weight",
        choices=("none", "balanced"),
        default="none",
        help="Class weighting for the linear classifier.",
    )
    parser.add_argument("--split-mode", choices=("random", "group"), default="group", help="Train/test split mode.")
    parser.add_argument(
        "--group-columns",
        default="directionLabel,motionState",
        help="Comma-separated columns for group split.",
    )
    args = parser.parse_args()

    repo_root = DEFAULT_REPO_ROOT
    log_dir = Path(args.log_dir)
    label_output = Path(args.label_output)
    model_path = Path(args.model)
    predictions_path = Path(args.predictions)
    objective_flag = [] if args.subjective else ["--objective-only"]

    analyze_command = [
        sys.executable,
        str(repo_root / "Tools" / "analyze_peripheral_csv.py"),
        "--batch-label-dataset",
        "--log-dir",
        str(log_dir),
        "--batch-label-dataset-csv",
        str(label_output),
        *objective_flag,
    ]
    run_command(analyze_command, repo_root)

    dataset_path = label_output
    if data_row_count(dataset_path) == 0:
        fallback_dataset = repo_root / DEFAULT_FALLBACK_DATASET
        if not fallback_dataset.exists():
            generate_command = [
                sys.executable,
                str(repo_root / "Tools" / "generate_simulation_dataset.py"),
                "--mode",
                "grid",
                "--output",
                str(fallback_dataset),
            ]
            run_command(generate_command, repo_root)

        dataset_path = fallback_dataset
        print(f"Label dataset was empty; falling back to simulation dataset: {dataset_path}")

    train_command = [
        sys.executable,
        str(repo_root / "Tools" / "train_cue_model.py"),
        "--dataset",
        str(dataset_path),
        "--model",
        str(model_path),
        "--predictions",
        str(predictions_path),
        "--test-ratio",
        str(args.test_ratio),
        "--seed",
        str(args.seed),
        "--epochs",
        str(args.epochs),
        "--classifier-epochs",
        str(args.classifier_epochs),
        "--classifier",
        args.classifier,
        "--class-weight",
        args.class_weight,
        "--split-mode",
        args.split_mode,
        "--group-columns",
        args.group_columns,
    ]
    run_command(train_command, repo_root)

    print(f"Label dataset: {label_output}")
    print(f"Training dataset: {dataset_path}")
    print(f"Model: {model_path}")
    print(f"Predictions: {predictions_path}")


if __name__ == "__main__":
    main()
