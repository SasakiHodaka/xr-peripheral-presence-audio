import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


EXPECTED_CONDITIONS = [
    "TRADITIONAL",
    "DIRECTION_ONLY",
    "DIRECTION_DISTANCE",
    "DIRECTION_DISTANCE_SPEAKING",
    "FULL_SCENE_TOKEN",
]


def safe_float(value, default=0.0):
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def average(values):
    return sum(values) / len(values) if values else 0.0


def pct(numerator, denominator):
    if denominator == 0:
        return 0.0
    return 100.0 * numerator / denominator


def parse_payload(value):
    result = {}
    if not value:
        return result
    for part in value.split(";"):
        if "=" not in part:
            continue
        key, item_value = part.split("=", 1)
        result[key] = item_value
    return result


def most_common(counter):
    if not counter:
        return "-"
    key, count = counter.most_common(1)[0]
    return f"{key} ({count})"


def collect_token_stats(root):
    files = sorted(root.glob("scene_tokens_*.csv"))
    stats = defaultdict(
        lambda: {
            "rows": 0,
            "speaking": 0,
            "semantic": 0,
            "turn_holder": 0,
            "overlap": 0,
            "sessions": Counter(),
            "participants": Counter(),
            "speakers": Counter(),
            "directions": Counter(),
            "distances": Counter(),
            "turns": Counter(),
            "semantics": Counter(),
        }
    )

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = row.get("condition", "") or "(none)"
                item = stats[condition]
                item["rows"] += 1

                for field, counter_name in (
                    ("sessionId", "sessions"),
                    ("participantId", "participants"),
                    ("speakerId", "speakers"),
                    ("direction", "directions"),
                    ("distance", "distances"),
                    ("turnState", "turns"),
                    ("semanticToken", "semantics"),
                ):
                    value = row.get(field, "")
                    if value:
                        item[counter_name][value] += 1

                if row.get("speakingState") == "SPEAKING":
                    item["speaking"] += 1
                if row.get("semanticToken") not in ("", "NONE", None):
                    item["semantic"] += 1
                if row.get("turnState") == "TURN_HOLDER":
                    item["turn_holder"] += 1
                if row.get("turnState") == "OVERLAPPER":
                    item["overlap"] += 1

    return files, stats


def collect_metric_stats(root):
    files = sorted(root.glob("scene_token_metrics_*.csv"))
    stats = defaultdict(lambda: defaultdict(list))

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = row.get("condition", "") or "(none)"
                for field in (
                    "tokensPerSecond",
                    "jsonBytesPerSecond",
                    "compactBytesPerSecond",
                    "objectMetadataBytesPerSecond",
                    "compactSavingsRatio",
                ):
                    stats[condition][field].append(safe_float(row.get(field)))

    return files, stats


def collect_event_stats(root):
    files = sorted(root.glob("scene_token_events_*.csv"))
    stats = defaultdict(
        lambda: {
            "events": 0,
            "event_types": Counter(),
            "direction_responses": 0,
            "direction_scored": 0,
            "direction_correct": 0,
            "direction_latency": [],
            "speaker_responses": 0,
            "speaker_scored": 0,
            "speaker_correct": 0,
            "speaker_latency": [],
            "ambiguous": 0,
        }
    )

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                event_type = row.get("eventType", "")
                payload = parse_payload(row.get("value", ""))
                condition = payload.get("condition", "(none)")
                item = stats[condition]
                item["events"] += 1
                item["event_types"][event_type] += 1

                ambiguous = payload.get("ambiguous", "").lower() == "true"
                is_correct = payload.get("isCorrect", "").lower() == "true"
                latency = safe_float(payload.get("responseLatency", "-1"), -1.0)

                if event_type == "response_direction":
                    item["direction_responses"] += 1
                    if ambiguous:
                        item["ambiguous"] += 1
                    else:
                        item["direction_scored"] += 1
                        if is_correct:
                            item["direction_correct"] += 1
                        if latency >= 0:
                            item["direction_latency"].append(latency)

                if event_type == "response_speaker":
                    item["speaker_responses"] += 1
                    if ambiguous:
                        item["ambiguous"] += 1
                    else:
                        item["speaker_scored"] += 1
                        if is_correct:
                            item["speaker_correct"] += 1
                        if latency >= 0:
                            item["speaker_latency"].append(latency)

    return files, stats


def quality_checks(token_stats, metric_stats, event_stats):
    found = set(token_stats) | set(metric_stats)
    missing = [condition for condition in EXPECTED_CONDITIONS if condition not in found]
    total_token_rows = sum(item["rows"] for item in token_stats.values())
    speaking_rows = sum(item["speaking"] for item in token_stats.values())
    semantic_rows = sum(item["semantic"] for item in token_stats.values())
    direction_responses = sum(item["direction_responses"] for item in event_stats.values())
    speaker_responses = sum(item["speaker_responses"] for item in event_stats.values())

    checks = [
        ("All five conditions", not missing, "missing=" + "|".join(missing) if missing else "ok"),
        ("Token rows", total_token_rows > 0, f"rows={total_token_rows}"),
        ("Speaking rows", speaking_rows > 0, f"speaking={speaking_rows}"),
        ("Semantic rows", semantic_rows > 0, f"semantic={semantic_rows}"),
        ("Direction responses", direction_responses > 0, f"responses={direction_responses}"),
        ("Speaker responses", speaker_responses > 0, f"responses={speaker_responses}"),
    ]
    return checks


def render_markdown(root, token_files, token_stats, metric_files, metric_stats, event_files, event_stats):
    lines = []
    lines.append("# Scene Token Experiment Summary")
    lines.append("")
    lines.append(f"- Log directory: `{root}`")
    lines.append(f"- Token log files: {len(token_files)}")
    lines.append(f"- Metrics log files: {len(metric_files)}")
    lines.append(f"- Event log files: {len(event_files)}")
    lines.append("")

    lines.append("## Quality Checks")
    lines.append("")
    lines.append("| Check | Result | Details |")
    lines.append("| --- | --- | --- |")
    for name, ok, details in quality_checks(token_stats, metric_stats, event_stats):
        lines.append(f"| {name} | {'OK' if ok else 'CHECK'} | {details} |")
    lines.append("")

    lines.append("## Token Summary")
    lines.append("")
    lines.append("| Condition | Rows | Speaking % | Semantic % | Turn holder | Overlap | Top direction | Top semantic |")
    lines.append("| --- | ---: | ---: | ---: | ---: | ---: | --- | --- |")
    for condition in sorted(token_stats):
        item = token_stats[condition]
        lines.append(
            "| {0} | {1} | {2:.1f} | {3:.1f} | {4} | {5} | {6} | {7} |".format(
                condition,
                item["rows"],
                pct(item["speaking"], item["rows"]),
                pct(item["semantic"], item["rows"]),
                item["turn_holder"],
                item["overlap"],
                most_common(item["directions"]),
                most_common(item["semantics"]),
            )
        )
    lines.append("")

    lines.append("## Response Summary")
    lines.append("")
    lines.append("| Condition | Direction acc. % | Direction latency s | Speaker acc. % | Speaker latency s | Ambiguous |")
    lines.append("| --- | ---: | ---: | ---: | ---: | ---: |")
    for condition in sorted(event_stats):
        item = event_stats[condition]
        lines.append(
            "| {0} | {1:.1f} | {2:.3f} | {3:.1f} | {4:.3f} | {5} |".format(
                condition,
                pct(item["direction_correct"], item["direction_scored"]),
                average(item["direction_latency"]),
                pct(item["speaker_correct"], item["speaker_scored"]),
                average(item["speaker_latency"]),
                item["ambiguous"],
            )
        )
    lines.append("")

    lines.append("## Communication Metrics")
    lines.append("")
    lines.append("| Condition | Tokens/s | JSON B/s | Compact B/s | Object metadata B/s | Compact saving |")
    lines.append("| --- | ---: | ---: | ---: | ---: | ---: |")
    for condition in sorted(metric_stats):
        item = metric_stats[condition]
        lines.append(
            "| {0} | {1:.2f} | {2:.2f} | {3:.2f} | {4:.2f} | {5:.1f}% |".format(
                condition,
                average(item["tokensPerSecond"]),
                average(item["jsonBytesPerSecond"]),
                average(item["compactBytesPerSecond"]),
                average(item["objectMetadataBytesPerSecond"]),
                100.0 * average(item["compactSavingsRatio"]),
            )
        )
    lines.append("")

    lines.append("## Weekly Report Draft")
    lines.append("")
    lines.append(
        "Scene Token実験ログを集計し，各条件におけるToken生成状況，参加者回答，通信量指標を確認した。"
        "Tokenログでは発話状態，TurnState，SemanticTokenが条件ごとに記録され，Eventログでは方向回答・話者回答の正答率と反応時間を算出できるようになった。"
        "今後はUnity Editor上で実ログを取得し，5条件間で話者把握および会話理解に差が出るかを分析する。"
    )
    lines.append("")
    return "\n".join(lines)


def main():
    if len(sys.argv) not in (2, 3):
        print("Usage: python Tools/summarize_experiment_run.py <log_directory> [output_md]")
        return 1

    root = Path(sys.argv[1])
    if not root.exists() or not root.is_dir():
        print(f"Log directory not found: {root}")
        return 1

    token_files, token_stats = collect_token_stats(root)
    metric_files, metric_stats = collect_metric_stats(root)
    event_files, event_stats = collect_event_stats(root)
    markdown = render_markdown(root, token_files, token_stats, metric_files, metric_stats, event_files, event_stats)

    if len(sys.argv) == 3:
        output_path = Path(sys.argv[2])
        output_path.write_text(markdown, encoding="utf-8")
        print(f"Wrote {output_path}")
    else:
        print(markdown)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
