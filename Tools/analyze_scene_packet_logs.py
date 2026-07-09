import csv
import sys
from collections import defaultdict
from pathlib import Path


NUMERIC_FIELDS = (
    "generatedTokenCount",
    "selectedTokenCount",
    "droppedTokenCount",
    "importantTokenCount",
    "importantTokenKeptCount",
    "payloadBytes",
    "estimatedBytes",
    "packetImportance",
    "packetPriority",
)

CONDITION_ALIASES = {
    "TRADITIONAL": "C1_TRADITIONAL",
    "DIRECTION_DISTANCE": "C2_DIRECTION_DISTANCE",
    "FULL_SCENE_TOKEN": "C3_FULL_SCENE_TOKEN",
    "SELECTED_SCENE_TOKEN": "C4_SELECTED_SCENE_TOKEN",
}


def normalize_condition(value):
    condition = value or "(none)"
    return CONDITION_ALIASES.get(condition, condition)


def safe_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def average(values):
    return sum(values) / len(values) if values else 0.0


def pct(numerator, denominator):
    return 100.0 * numerator / denominator if denominator else 0.0


def duration(item):
    first_time = item["first_time"]
    last_time = item["last_time"]
    if first_time is None or last_time is None:
        return 0.0
    return max(0.0, last_time - first_time)


def main():
    if len(sys.argv) != 2:
        print("Usage: python Tools/analyze_scene_packet_logs.py <scene_packets_csv_or_directory>")
        return 1

    target = Path(sys.argv[1])
    files = sorted(target.glob("scene_packets_*.csv")) if target.is_dir() else [target]
    if not files:
        print("No scene_packets_*.csv files found.")
        return 1

    by_condition = defaultdict(
        lambda: {
            "packets": 0,
            "first_time": None,
            "last_time": None,
            "packetImportance": [],
            "packetPriority": [],
            "generatedTokenCount": 0.0,
            "selectedTokenCount": 0.0,
            "droppedTokenCount": 0.0,
            "importantTokenCount": 0.0,
            "importantTokenKeptCount": 0.0,
            "payloadBytes": 0.0,
            "estimatedBytes": 0.0,
        }
    )

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = normalize_condition(row.get("condition", ""))
                item = by_condition[condition]
                item["packets"] += 1

                timestamp = safe_float(row.get("timestamp"))
                if item["first_time"] is None or timestamp < item["first_time"]:
                    item["first_time"] = timestamp
                if item["last_time"] is None or timestamp > item["last_time"]:
                    item["last_time"] = timestamp

                for field in NUMERIC_FIELDS:
                    value = safe_float(row.get(field))
                    if field in ("packetImportance", "packetPriority"):
                        item[field].append(value)
                    else:
                        item[field] += value

    header = [
        "condition",
        "packets",
        "packetsPerSecond",
        "estimatedBytesPerSecond",
        "payloadBytesPerSecond",
        "tokensPerPacket",
        "dropRatio",
        "importantTokenKeptRatio",
        "avgPacketImportance",
        "avgPacketPriority",
    ]
    print(",".join(header))

    for condition in sorted(by_condition):
        item = by_condition[condition]
        elapsed = duration(item)
        packets = item["packets"]
        row = [
            condition,
            str(packets),
            f"{packets / elapsed if elapsed > 0 else 0.0:.2f}",
            f"{item['estimatedBytes'] / elapsed if elapsed > 0 else 0.0:.2f}",
            f"{item['payloadBytes'] / elapsed if elapsed > 0 else 0.0:.2f}",
            f"{item['selectedTokenCount'] / packets if packets > 0 else 0.0:.2f}",
            f"{pct(item['droppedTokenCount'], item['generatedTokenCount']) / 100.0:.4f}",
            f"{pct(item['importantTokenKeptCount'], item['importantTokenCount']) / 100.0:.4f}",
            f"{average(item['packetImportance']):.4f}",
            f"{average(item['packetPriority']):.4f}",
        ]
        print(",".join(row))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
