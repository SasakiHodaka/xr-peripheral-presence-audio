#!/usr/bin/env python
import argparse
import csv
import json
import math
import random
from collections import Counter, defaultdict
from pathlib import Path


DEFAULT_DATASET = Path("cue_training_dataset.csv")
DEFAULT_MODEL_PATH = Path("Assets") / "Models" / "cue_model_unity.json"
DEFAULT_PREDICTIONS_PATH = Path("cue_training_predictions.csv")

CATEGORICAL_COLUMNS = (
    "conditionLabel",
    "cueCondition",
    "materialClass",
    "targetId",
    "directionLabel",
    "viewState",
    "motionState",
)
BOOLEAN_COLUMNS = (
    "outOfView",
    "approaching",
    "speaking",
    "gazing",
    "near",
    "crossing",
    "frontHemisphere",
    "sideHemisphere",
    "rearHemisphere",
    "far",
    "activeMotion",
    "passivePresenceCandidate",
)
NUMERIC_COLUMNS = (
    "roomScale",
    "environmentReverbAmount",
    "environmentOcclusionStrength",
    "environmentDistanceAttenuation",
    "environmentRt60",
    "environmentDrr",
    "distance",
    "viewAngle",
    "radialSpeed",
    "lateralSpeed",
    "localX",
    "localY",
    "localZ",
)

TARGET_CLASS_COLUMN = "cueType"
TARGET_REGRESSION_COLUMNS = (
    "presenceScore",
    "volumeGain",
    "cueLowPassHz",
    "cueReverbAmount",
    "cueOcclusionGain",
)


def parse_bool(value):
    return str(value).strip().lower() == "true"


def parse_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def load_rows(path):
    with path.open("r", encoding="utf-8-sig", newline="") as file:
        return list(csv.DictReader(file))


def train_test_split(rows, test_ratio, seed):
    shuffled = list(rows)
    random.Random(seed).shuffle(shuffled)
    if len(shuffled) <= 1:
        return shuffled, []

    test_count = max(1, int(round(len(shuffled) * test_ratio)))
    test_count = min(test_count, len(shuffled) - 1)
    return shuffled[test_count:], shuffled[:test_count]


def group_train_test_split(rows, test_ratio, seed, group_columns):
    groups = defaultdict(list)
    for row in rows:
        key = tuple(row.get(column, "") for column in group_columns)
        groups[key].append(row)

    group_items = list(groups.items())
    random.Random(seed).shuffle(group_items)
    if len(group_items) <= 1:
        return rows, []

    target_test_count = max(1, int(round(len(rows) * test_ratio)))
    train_rows = []
    test_rows = []

    for _, group_rows in group_items:
        if len(test_rows) < target_test_count:
            test_rows.extend(group_rows)
        else:
            train_rows.extend(group_rows)

    if not train_rows:
        train_rows = test_rows[len(test_rows) // 2 :]
        test_rows = test_rows[: len(test_rows) // 2]

    return train_rows, test_rows


class FeatureEncoder:
    def __init__(self):
        self.categories = {}
        self.numeric_stats = {}
        self.feature_names = []

    def fit(self, rows):
        self.categories = {
            column: sorted({row.get(column, "") for row in rows})
            for column in CATEGORICAL_COLUMNS
        }

        self.numeric_stats = {}
        for column in NUMERIC_COLUMNS:
            values = [parse_float(row.get(column)) for row in rows]
            mean = sum(values) / len(values) if values else 0.0
            variance = sum((value - mean) ** 2 for value in values) / len(values) if values else 0.0
            std = math.sqrt(variance) or 1.0
            self.numeric_stats[column] = {"mean": mean, "std": std}

        feature_names = []
        for column in CATEGORICAL_COLUMNS:
            for category in self.categories[column]:
                feature_names.append(f"{column}={category}")
        feature_names.extend(BOOLEAN_COLUMNS)
        feature_names.extend(NUMERIC_COLUMNS)
        self.feature_names = feature_names

    def transform_row(self, row):
        features = []
        for column in CATEGORICAL_COLUMNS:
            value = row.get(column, "")
            features.extend(1.0 if value == category else 0.0 for category in self.categories[column])

        features.extend(1.0 if parse_bool(row.get(column)) else 0.0 for column in BOOLEAN_COLUMNS)

        for column in NUMERIC_COLUMNS:
            stats = self.numeric_stats[column]
            value = parse_float(row.get(column))
            features.append((value - stats["mean"]) / stats["std"])

        return features

    def transform(self, rows):
        return [self.transform_row(row) for row in rows]

    def to_dict(self):
        return {
            "categoricalColumns": list(CATEGORICAL_COLUMNS),
            "booleanColumns": list(BOOLEAN_COLUMNS),
            "numericColumns": list(NUMERIC_COLUMNS),
            "categorySets": [
                {
                    "column": column,
                    "categories": self.categories[column],
                }
                for column in CATEGORICAL_COLUMNS
            ],
            "numericStats": [
                {
                    "column": column,
                    "mean": self.numeric_stats[column]["mean"],
                    "std": self.numeric_stats[column]["std"],
                }
                for column in NUMERIC_COLUMNS
            ],
        }


def vector_mean(vectors):
    if not vectors:
        return []

    width = len(vectors[0])
    return [sum(vector[index] for vector in vectors) / len(vectors) for index in range(width)]


def squared_distance(a, b):
    return sum((left - right) ** 2 for left, right in zip(a, b))


def train_centroid_classifier(features, rows):
    by_class = defaultdict(list)
    for vector, row in zip(features, rows):
        by_class[row.get(TARGET_CLASS_COLUMN, "None")].append(vector)

    return {
        label: vector_mean(vectors)
        for label, vectors in sorted(by_class.items())
    }


def predict_class(model, vector):
    if not model:
        return "None"

    return min(model.items(), key=lambda item: squared_distance(vector, item[1]))[0]


def softmax(scores):
    if not scores:
        return []

    maximum = max(scores)
    exps = [math.exp(score - maximum) for score in scores]
    total = sum(exps) or 1.0
    return [value / total for value in exps]


def train_linear_classifier(features, rows, epochs=140, learning_rate=0.05, l2=0.001, class_weight="balanced"):
    labels = sorted({row.get(TARGET_CLASS_COLUMN, "None") for row in rows})
    if not features or not labels:
        return []

    width = len(features[0])
    label_to_index = {label: index for index, label in enumerate(labels)}
    class_counts = Counter(row.get(TARGET_CLASS_COLUMN, "None") for row in rows)
    class_weights = {
        label: (len(rows) / (len(labels) * class_counts[label])) if class_weight == "balanced" and class_counts[label] else 1.0
        for label in labels
    }
    weights = [[0.0] * width for _ in labels]
    biases = [0.0] * len(labels)

    for _ in range(epochs):
        grad_weights = [[0.0] * width for _ in labels]
        grad_biases = [0.0] * len(labels)

        for vector, row in zip(features, rows):
            target_label = row.get(TARGET_CLASS_COLUMN, "None")
            target_index = label_to_index[target_label]
            sample_weight = class_weights[target_label]
            scores = [dot(class_weights, vector) + bias for class_weights, bias in zip(weights, biases)]
            probabilities = softmax(scores)

            for class_index, probability in enumerate(probabilities):
                error = (probability - (1.0 if class_index == target_index else 0.0)) * sample_weight
                grad_biases[class_index] += error
                for feature_index, value in enumerate(vector):
                    grad_weights[class_index][feature_index] += error * value

        scale = 1.0 / len(features)
        for class_index in range(len(labels)):
            biases[class_index] -= learning_rate * grad_biases[class_index] * scale
            for feature_index in range(width):
                regularization = l2 * weights[class_index][feature_index]
                weights[class_index][feature_index] -= learning_rate * (
                    grad_weights[class_index][feature_index] * scale + regularization
                )

    return [
        {
            "label": label,
            "weights": weights[index],
            "bias": biases[index],
            "classWeight": class_weights[label],
        }
        for index, label in enumerate(labels)
    ]


def predict_linear_class(model, vector):
    if not model:
        return "None"

    best = None
    best_score = float("-inf")
    for candidate in model:
        score = dot(candidate["weights"], vector) + candidate["bias"]
        if score > best_score:
            best_score = score
            best = candidate

    return best["label"] if best else "None"


def train_linear_regressor(features, values, epochs=80, learning_rate=0.03, l2=0.001):
    if not features:
        return {"weights": [], "bias": 0.0}

    width = len(features[0])
    weights = [0.0] * width
    bias = sum(values) / len(values) if values else 0.0

    for _ in range(epochs):
        grad_weights = [0.0] * width
        grad_bias = 0.0

        for vector, target in zip(features, values):
            prediction = dot(weights, vector) + bias
            error = prediction - target
            grad_bias += error
            for index, value in enumerate(vector):
                grad_weights[index] += error * value

        scale = 1.0 / len(features)
        bias -= learning_rate * grad_bias * scale
        for index in range(width):
            regularization = l2 * weights[index]
            weights[index] -= learning_rate * (grad_weights[index] * scale + regularization)

    return {"weights": weights, "bias": bias}


def dot(a, b):
    return sum(left * right for left, right in zip(a, b))


def predict_regression(model, vector):
    return dot(model["weights"], vector) + model["bias"]


def train_regressors(features, rows, epochs):
    models = {}
    for column in TARGET_REGRESSION_COLUMNS:
        values = [parse_float(row.get(column)) for row in rows]
        models[column] = train_linear_regressor(features, values, epochs=epochs)

    return models


def evaluate(classifier, regressors, features, rows, linear_classifier=None):
    if not rows:
        return {
            "rows": 0,
            "cueTypeAccuracy": None,
            "regressionMae": {column: None for column in TARGET_REGRESSION_COLUMNS},
        }

    correct = 0
    absolute_errors = {column: [] for column in TARGET_REGRESSION_COLUMNS}
    labels = sorted({row.get(TARGET_CLASS_COLUMN, "None") for row in rows})
    confusion = {
        actual: {predicted: 0 for predicted in labels}
        for actual in labels
    }
    per_class = {
        label: {"correct": 0, "total": 0}
        for label in labels
    }

    for vector, row in zip(features, rows):
        actual_class = row.get(TARGET_CLASS_COLUMN, "None")
        predicted_class = predict_linear_class(linear_classifier, vector) if linear_classifier else predict_class(classifier, vector)
        if predicted_class not in labels:
            labels.append(predicted_class)
            for actual in confusion:
                confusion[actual][predicted_class] = 0
        if actual_class not in confusion:
            confusion[actual_class] = {label: 0 for label in labels}
            per_class[actual_class] = {"correct": 0, "total": 0}

        confusion[actual_class][predicted_class] += 1
        per_class[actual_class]["total"] += 1
        if predicted_class == actual_class:
            correct += 1
            per_class[actual_class]["correct"] += 1

        for column in TARGET_REGRESSION_COLUMNS:
            predicted_value = predict_regression(regressors[column], vector)
            actual_value = parse_float(row.get(column))
            absolute_errors[column].append(abs(predicted_value - actual_value))

    return {
        "rows": len(rows),
        "cueTypeAccuracy": correct / len(rows),
        "cueTypePerClassAccuracy": {
            label: values["correct"] / values["total"] if values["total"] else None
            for label, values in sorted(per_class.items())
        },
        "cueTypeConfusion": confusion,
        "regressionMae": {
            column: sum(values) / len(values) if values else None
            for column, values in absolute_errors.items()
        },
    }


def write_predictions(path, classifier, regressors, features, rows, linear_classifier=None):
    fieldnames = [
        "sourceCsv",
        "conditionLabel",
        "cueCondition",
        "targetId",
        "actualCueType",
        "predictedCueType",
    ]

    for column in TARGET_REGRESSION_COLUMNS:
        fieldnames.append(f"actual_{column}")
        fieldnames.append(f"predicted_{column}")
        fieldnames.append(f"absError_{column}")

    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()

        for vector, row in zip(features, rows):
            output = {
                "sourceCsv": row.get("sourceCsv", ""),
                "conditionLabel": row.get("conditionLabel", ""),
                "cueCondition": row.get("cueCondition", ""),
                "targetId": row.get("targetId", ""),
                "actualCueType": row.get(TARGET_CLASS_COLUMN, ""),
                "predictedCueType": predict_linear_class(linear_classifier, vector) if linear_classifier else predict_class(classifier, vector),
            }

            for column in TARGET_REGRESSION_COLUMNS:
                actual = parse_float(row.get(column))
                predicted = predict_regression(regressors[column], vector)
                output[f"actual_{column}"] = f"{actual:.6f}"
                output[f"predicted_{column}"] = f"{predicted:.6f}"
                output[f"absError_{column}"] = f"{abs(predicted - actual):.6f}"

            writer.writerow(output)


def save_model(path, encoder, classifier, regressors, metrics, class_counts, linear_classifier=None):
    path.parent.mkdir(parents=True, exist_ok=True)
    payload = {
        "modelType": "linear_classifier_plus_linear_regressors" if linear_classifier else "centroid_classifier_plus_linear_regressors",
        "featureEncoder": encoder.to_dict(),
        "classifier": [
            {
                "label": label,
                "centroid": centroid,
            }
            for label, centroid in sorted(classifier.items())
        ],
        "linearClassifier": linear_classifier or [],
        "regressors": [
            {
                "target": target,
                "weights": model["weights"],
                "bias": model["bias"],
            }
            for target, model in sorted(regressors.items())
        ],
        "targetClassColumn": TARGET_CLASS_COLUMN,
        "targetRegressionColumns": list(TARGET_REGRESSION_COLUMNS),
        "classCounts": [
            {
                "label": label,
                "count": count,
            }
            for label, count in sorted(class_counts.items())
        ],
        "metrics": metrics,
    }
    path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def print_metrics(metrics, class_counts, model_path, predictions_path):
    print("AUI cue model training complete")
    print(f"Train rows: {metrics['train']['rows']}")
    print(f"Test rows: {metrics['test']['rows']}")
    print(f"Class counts: {dict(class_counts)}")
    print(f"Train cueType accuracy: {format_optional(metrics['train']['cueTypeAccuracy'])}")
    print(f"Test cueType accuracy: {format_optional(metrics['test']['cueTypeAccuracy'])}")
    if metrics["test"].get("cueTypePerClassAccuracy"):
        print("Test cueType per-class accuracy:")
        for label, value in metrics["test"]["cueTypePerClassAccuracy"].items():
            print(f"  {label}: {format_optional(value)}")
    print("Test MAE:")
    for column, value in metrics["test"]["regressionMae"].items():
        print(f"  {column}: {format_optional(value)}")
    print(f"Model: {model_path}")
    print(f"Predictions: {predictions_path}")


def format_optional(value):
    if value is None:
        return "n/a"
    return f"{value:.4f}"


def main():
    parser = argparse.ArgumentParser(description="Train the first lightweight AUI cue-control model.")
    parser.add_argument("--dataset", default=str(DEFAULT_DATASET), help="Input cue_training_dataset.csv path.")
    parser.add_argument("--model", default=str(DEFAULT_MODEL_PATH), help="Output model JSON path.")
    parser.add_argument("--predictions", default=str(DEFAULT_PREDICTIONS_PATH), help="Output test predictions CSV path.")
    parser.add_argument("--test-ratio", type=float, default=0.25, help="Held-out test split ratio.")
    parser.add_argument("--seed", type=int, default=7, help="Random seed for train/test split.")
    parser.add_argument("--epochs", type=int, default=80, help="Gradient-descent epochs for regression targets.")
    parser.add_argument("--classifier-epochs", type=int, default=160, help="Gradient-descent epochs for cueType classification.")
    parser.add_argument("--classifier", choices=("linear", "centroid"), default="linear", help="cueType classifier.")
    parser.add_argument("--class-weight", choices=("none", "balanced"), default="none", help="Class weighting for linear classifier.")
    parser.add_argument("--split-mode", choices=("random", "group"), default="random", help="Train/test split mode.")
    parser.add_argument("--group-columns", default="directionLabel,motionState", help="Comma-separated columns for group split.")
    args = parser.parse_args()

    dataset_path = Path(args.dataset)
    rows = load_rows(dataset_path)
    if not rows:
        raise ValueError(f"No rows found in {dataset_path}")

    group_columns = [column.strip() for column in args.group_columns.split(",") if column.strip()]
    if args.split_mode == "group":
        train_rows, test_rows = group_train_test_split(rows, args.test_ratio, args.seed, group_columns)
    else:
        train_rows, test_rows = train_test_split(rows, args.test_ratio, args.seed)
    encoder = FeatureEncoder()
    encoder.fit(train_rows)

    train_features = encoder.transform(train_rows)
    test_features = encoder.transform(test_rows)
    classifier = train_centroid_classifier(train_features, train_rows)
    linear_classifier = train_linear_classifier(
        train_features,
        train_rows,
        epochs=args.classifier_epochs,
        class_weight=args.class_weight,
    ) if args.classifier == "linear" else None
    regressors = train_regressors(train_features, train_rows, args.epochs)

    metrics = {
        "train": evaluate(classifier, regressors, train_features, train_rows, linear_classifier),
        "test": evaluate(classifier, regressors, test_features, test_rows, linear_classifier),
        "epochs": args.epochs,
        "classifier": args.classifier,
        "classifierEpochs": args.classifier_epochs,
        "classWeight": args.class_weight if args.classifier == "linear" else "none",
        "splitMode": args.split_mode,
        "groupColumns": group_columns if args.split_mode == "group" else [],
    }
    class_counts = Counter(row.get(TARGET_CLASS_COLUMN, "None") for row in train_rows)

    model_path = Path(args.model)
    predictions_path = Path(args.predictions)
    save_model(model_path, encoder, classifier, regressors, metrics, class_counts, linear_classifier)
    write_predictions(predictions_path, classifier, regressors, test_features, test_rows, linear_classifier)
    print_metrics(metrics, class_counts, model_path, predictions_path)


if __name__ == "__main__":
    main()
