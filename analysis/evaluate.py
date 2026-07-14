import csv
import sys
from collections import Counter
from pathlib import Path


DEFAULT_LOG = (
    Path.home()
    / "AppData"
    / "LocalLow"
    / "DefaultCompany"
    / "SemanticSpatialAudio"
    / "scene_token_ground_truth"
)


def safe_int(value, default=0):
    try:
        return int(value)
    except (TypeError, ValueError):
        return default


def safe_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def pct(numerator, denominator):
    if denominator == 0:
        return 0.0
    return 100.0 * numerator / denominator


def load_rows(path):
    with path.open(newline="", encoding="utf-8-sig") as handle:
        return list(csv.DictReader(handle))


def resolve_input_path(path):
    if path.is_file():
        return path

    if path.is_dir():
        candidates = sorted(path.glob("generated_scene_tokens_*.csv"))
        if not candidates:
            return path / "generated_scene_tokens_v3.csv"
        return max(candidates, key=lambda candidate: candidate.stat().st_mtime)

    return path


def evaluate(path):
    rows = load_rows(path)
    total_events = len(rows)
    selected_rows = [row for row in rows if row.get("selected", "").lower() == "true"]

    levels = Counter(row.get("communicationLevel", "") or "(none)" for row in rows)
    flags = Counter(row.get("packetFlags", "") or "(none)" for row in rows)
    scenarios = Counter(row.get("scenarioId", "") or "(none)" for row in rows)
    directions = Counter(row.get("direction", "") or "(none)" for row in rows)
    presentations = Counter(row.get("presentationMode", "") or "(none)" for row in rows)

    packet_bytes = [safe_int(row.get("packetBytes")) for row in rows if row.get("packetBytes")]
    total_packet_bytes = sum(packet_bytes)
    avg_packet_bytes = total_packet_bytes / len(packet_bytes) if packet_bytes else 0.0

    priorities = Counter(row.get("priority", "") or "(none)" for row in rows)
    high_priority = [row for row in rows if safe_int(row.get("priority")) >= 2]
    high_priority_selected = [
        row for row in high_priority if row.get("selected", "").lower() == "true"
    ]

    expire_times = [safe_float(row.get("expireTime")) for row in rows if row.get("expireTime")]

    return {
        "path": path,
        "total_events": total_events,
        "selected_events": len(selected_rows),
        "selection_rate": pct(len(selected_rows), total_events),
        "total_packet_bytes": total_packet_bytes,
        "avg_packet_bytes": avg_packet_bytes,
        "levels": levels,
        "flags": flags,
        "scenarios": scenarios,
        "directions": directions,
        "presentations": presentations,
        "priorities": priorities,
        "high_priority_events": len(high_priority),
        "high_priority_selected": len(high_priority_selected),
        "high_priority_selection_rate": pct(len(high_priority_selected), len(high_priority)),
        "min_expire_time": min(expire_times) if expire_times else 0.0,
        "max_expire_time": max(expire_times) if expire_times else 0.0,
    }


def print_counter(title, counter):
    print(title)
    if not counter:
        print("  -")
        return
    for key, count in sorted(counter.items()):
        print(f"  {key}: {count}")


def print_report(stats):
    print("Evaluation Summary")
    print(f"Input: {stats['path']}")
    print(f"Total events: {stats['total_events']}")
    print(f"Selected events: {stats['selected_events']}")
    print(f"Selection rate: {stats['selection_rate']:.1f}%")
    print(f"Total packet bytes: {stats['total_packet_bytes']}")
    print(f"Average packet bytes: {stats['avg_packet_bytes']:.1f}")
    print(f"High-priority selected: {stats['high_priority_selected']}/{stats['high_priority_events']}")
    print(f"High-priority selection rate: {stats['high_priority_selection_rate']:.1f}%")
    print(f"Expire time range: {stats['min_expire_time']:.3f} - {stats['max_expire_time']:.3f}")
    print()
    print_counter("By scenario:", stats["scenarios"])
    print_counter("By communication level:", stats["levels"])
    print_counter("By packet flags:", stats["flags"])
    print_counter("By priority:", stats["priorities"])
    print_counter("By direction:", stats["directions"])
    print_counter("By presentation:", stats["presentations"])


def main():
    path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_LOG
    path = resolve_input_path(path)

    if not path.exists():
        print(f"Log file not found: {path}", file=sys.stderr)
        return 1

    stats = evaluate(path)
    print_report(stats)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
