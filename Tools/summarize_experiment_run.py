import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


EXPECTED_CONDITIONS = [
    "C1_TRADITIONAL",
    "C2_DIRECTION_DISTANCE",
    "C3_FULL_SCENE_TOKEN",
    "C4_SELECTED_SCENE_TOKEN",
]

MAIN_CONDITION_ORDER = EXPECTED_CONDITIONS

EXPECTED_SCRIPTED_SEMANTICS = {
    "QUESTION",
    "ANSWER",
    "INSTRUCTION",
    "WARNING",
    "AGREEMENT",
}

CONDITION_ALIASES = {
    "TRADITIONAL": "C1_TRADITIONAL",
    "DIRECTION_DISTANCE": "C2_DIRECTION_DISTANCE",
    "FULL_SCENE_TOKEN": "C3_FULL_SCENE_TOKEN",
    "SELECTED_SCENE_TOKEN": "C4_SELECTED_SCENE_TOKEN",
}


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


def normalize_condition(value):
    condition = value or "(none)"
    return CONDITION_ALIASES.get(condition, condition)


def most_common(counter):
    if not counter:
        return "-"
    key, count = counter.most_common(1)[0]
    return f"{key} ({count})"


def ordered_conditions(mapping):
    seen = set()
    ordered = []
    for condition in MAIN_CONDITION_ORDER:
        if condition in mapping:
            ordered.append(condition)
            seen.add(condition)

    for condition in sorted(mapping):
        if condition not in seen:
            ordered.append(condition)

    return ordered


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
            "by_speaker": defaultdict(
                lambda: {
                    "rows": 0,
                    "speaking": 0,
                    "semantic": 0,
                    "directions": Counter(),
                    "distances": Counter(),
                    "turns": Counter(),
                    "semantics": Counter(),
                }
            ),
        }
    )

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = normalize_condition(row.get("condition", ""))
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

                speaker_id = row.get("speakerId", "")
                if speaker_id:
                    speaker_item = item["by_speaker"][speaker_id]
                    speaker_item["rows"] += 1
                    for field, counter_name in (
                        ("direction", "directions"),
                        ("distance", "distances"),
                        ("turnState", "turns"),
                        ("semanticToken", "semantics"),
                    ):
                        value = row.get(field, "")
                        if value:
                            speaker_item[counter_name][value] += 1
                    if row.get("speakingState") == "SPEAKING":
                        speaker_item["speaking"] += 1
                    if row.get("semanticToken") not in ("", "NONE", None):
                        speaker_item["semantic"] += 1

    return files, stats


def collect_metric_stats(root):
    files = sorted(root.glob("scene_token_metrics_*.csv"))
    stats = defaultdict(lambda: defaultdict(list))

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = normalize_condition(row.get("condition", ""))
                for field in (
                    "tokenCount",
                    "selectedTokenCount",
                    "importantTokenCount",
                    "importantTokenKept",
                    "tokensPerSecond",
                    "jsonBytesPerSecond",
                    "compactBytesPerSecond",
                    "objectMetadataBytesPerSecond",
                    "compactSavingsRatio",
                    "generatedTokensPerSecond",
                    "selectedTokensPerSecond",
                    "selectedJsonBytesPerSecond",
                    "selectedCompactBytesPerSecond",
                    "tokenDropRatio",
                    "importantTokenSendRatio",
                    "selectionSavingsRatio",
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
                condition = normalize_condition(payload.get("condition", "(none)"))
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


def collect_packet_stats(root):
    files = sorted(root.glob("scene_packets_*.csv"))
    stats = defaultdict(
        lambda: {
            "packets": 0,
            "first_time": None,
            "last_time": None,
            "estimated_bytes": 0.0,
            "payload_bytes": 0.0,
            "generated_tokens": 0.0,
            "selected_tokens": 0.0,
            "dropped_tokens": 0.0,
            "important_tokens": 0.0,
            "important_kept": 0.0,
            "importance": [],
            "priority": [],
        }
    )

    for path in files:
        with path.open(newline="", encoding="utf-8-sig") as handle:
            reader = csv.DictReader(handle)
            for row in reader:
                condition = normalize_condition(row.get("condition", ""))
                item = stats[condition]
                item["packets"] += 1

                timestamp = safe_float(row.get("timestamp"))
                if item["first_time"] is None or timestamp < item["first_time"]:
                    item["first_time"] = timestamp
                if item["last_time"] is None or timestamp > item["last_time"]:
                    item["last_time"] = timestamp

                item["estimated_bytes"] += safe_float(row.get("estimatedBytes"))
                item["payload_bytes"] += safe_float(row.get("payloadBytes"))
                item["generated_tokens"] += safe_float(row.get("generatedTokenCount"))
                item["selected_tokens"] += safe_float(row.get("selectedTokenCount"))
                item["dropped_tokens"] += safe_float(row.get("droppedTokenCount"))
                item["important_tokens"] += safe_float(row.get("importantTokenCount"))
                item["important_kept"] += safe_float(row.get("importantTokenKeptCount"))
                item["importance"].append(safe_float(row.get("packetImportance")))
                item["priority"].append(safe_float(row.get("packetPriority")))

    return files, stats


def packet_duration(item):
    first_time = item["first_time"]
    last_time = item["last_time"]
    if first_time is None or last_time is None:
        return 0.0
    return max(0.0, last_time - first_time)


def quality_checks(token_stats, metric_stats, event_stats, packet_stats):
    found = set(token_stats) | set(metric_stats) | set(packet_stats)
    missing = [condition for condition in EXPECTED_CONDITIONS if condition not in found]
    total_token_rows = sum(item["rows"] for item in token_stats.values())
    speaking_rows = sum(item["speaking"] for item in token_stats.values())
    semantic_rows = sum(item["semantic"] for item in token_stats.values())
    direction_responses = sum(item["direction_responses"] for item in event_stats.values())
    speaker_responses = sum(item["speaker_responses"] for item in event_stats.values())
    direction_scored = sum(item["direction_scored"] for item in event_stats.values())
    speaker_scored = sum(item["speaker_scored"] for item in event_stats.values())
    total_packets = sum(item["packets"] for item in packet_stats.values())
    found_semantics = {
        semantic
        for item in token_stats.values()
        for semantic in item["semantics"]
        if semantic and semantic != "NONE"
    }
    missing_semantics = sorted(EXPECTED_SCRIPTED_SEMANTICS - found_semantics)

    return [
        ("All main conditions", not missing, "missing=" + "|".join(missing) if missing else "ok"),
        ("Token rows", total_token_rows > 0, f"rows={total_token_rows}"),
        ("Speaking rows", speaking_rows > 0, f"speaking={speaking_rows}"),
        ("Semantic rows", semantic_rows > 0, f"semantic={semantic_rows}"),
        (
            "Scripted semantic labels",
            not missing_semantics,
            "missing=" + "|".join(missing_semantics) if missing_semantics else "ok",
        ),
        ("Direction responses", direction_responses > 0, f"responses={direction_responses}"),
        ("Speaker responses", speaker_responses > 0, f"responses={speaker_responses}"),
        ("Scored direction responses", direction_scored > 0, f"scored={direction_scored}"),
        ("Scored speaker responses", speaker_scored > 0, f"scored={speaker_scored}"),
        ("Scene packets", total_packets > 0, f"packets={total_packets}"),
    ]


def render_markdown(root, token_files, token_stats, metric_files, metric_stats, event_files, event_stats, packet_files, packet_stats):
    total_token_rows = sum(item["rows"] for item in token_stats.values())
    total_direction_responses = sum(item["direction_responses"] for item in event_stats.values())
    total_speaker_responses = sum(item["speaker_responses"] for item in event_stats.values())
    main_conditions = [condition for condition in EXPECTED_CONDITIONS if condition in token_stats or condition in metric_stats or condition in packet_stats]
    condition_text = ", ".join(main_conditions) if main_conditions else "none"
    has_participant_responses = total_direction_responses > 0 and total_speaker_responses > 0

    lines = [
        "# Scene Token Experiment Summary",
        "",
        f"- Log directory: `{root}`",
        f"- Token log files: {len(token_files)}",
        f"- Metrics log files: {len(metric_files)}",
        f"- Event log files: {len(event_files)}",
        f"- Packet log files: {len(packet_files)}",
        "",
        "## Quality Checks",
        "",
        "| Check | Result | Details |",
        "| --- | --- | --- |",
    ]

    for name, ok, details in quality_checks(token_stats, metric_stats, event_stats, packet_stats):
        lines.append(f"| {name} | {'OK' if ok else 'CHECK'} | {details} |")

    lines.extend(
        [
            "",
            "## Token Summary",
            "",
            "| Condition | Rows | Speaking % | Semantic % | Turn holder | Overlap | Top direction | Top semantic |",
            "| --- | ---: | ---: | ---: | ---: | ---: | --- | --- |",
        ]
    )
    for condition in ordered_conditions(token_stats):
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

    lines.extend(
        [
            "",
            "## Speaker Token Summary",
            "",
            "| Condition | Speaker | Rows | Speaking % | Semantic % | Top direction | Top distance | Top turn | Top semantic |",
            "| --- | --- | ---: | ---: | ---: | --- | --- | --- | --- |",
        ]
    )
    for condition in ordered_conditions(token_stats):
        for speaker_id in sorted(token_stats[condition]["by_speaker"]):
            item = token_stats[condition]["by_speaker"][speaker_id]
            lines.append(
                "| {0} | {1} | {2} | {3:.1f} | {4:.1f} | {5} | {6} | {7} | {8} |".format(
                    condition,
                    speaker_id,
                    item["rows"],
                    pct(item["speaking"], item["rows"]),
                    pct(item["semantic"], item["rows"]),
                    most_common(item["directions"]),
                    most_common(item["distances"]),
                    most_common(item["turns"]),
                    most_common(item["semantics"]),
                )
            )

    lines.extend(
        [
            "",
            "## Response Summary",
            "",
            "| Condition | Direction acc. % | Direction latency s | Speaker acc. % | Speaker latency s | Ambiguous |",
            "| --- | ---: | ---: | ---: | ---: | ---: |",
        ]
    )
    for condition in ordered_conditions(event_stats):
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

    lines.extend(
        [
            "",
            "## Communication Metrics",
            "",
            "| Condition | Avg bytes/s | Token count | Selected count | Drop % | Important kept % | Compact saving |",
            "| --- | ---: | ---: | ---: | ---: | ---: | ---: |",
        ]
    )
    for condition in ordered_conditions(metric_stats):
        item = metric_stats[condition]
        lines.append(
            "| {0} | {1:.2f} | {2:.0f} | {3:.0f} | {4:.1f}% | {5:.1f}% | {6:.1f}% |".format(
                condition,
                average(item["selectedJsonBytesPerSecond"]),
                sum(item["tokenCount"]),
                sum(item["selectedTokenCount"]),
                100.0 * average(item["tokenDropRatio"]),
                100.0 * average(item["importantTokenSendRatio"]),
                100.0 * average(item["compactSavingsRatio"]),
            )
        )

    has_selection_metrics = any(
        average(item["generatedTokensPerSecond"]) > 0 or average(item["selectedTokensPerSecond"]) > 0
        for item in metric_stats.values()
    )

    if has_selection_metrics:
        lines.extend(
            [
                "",
                "## Token Selection Metrics",
                "",
                "| Condition | Generated tokens/s | Selected tokens/s | Selected JSON B/s | Token drop | Important send | Selection saving |",
                "| --- | ---: | ---: | ---: | ---: | ---: | ---: |",
            ]
        )
        for condition in ordered_conditions(metric_stats):
            item = metric_stats[condition]
            lines.append(
                "| {0} | {1:.2f} | {2:.2f} | {3:.2f} | {4:.1f}% | {5:.1f}% | {6:.1f}% |".format(
                    condition,
                    average(item["generatedTokensPerSecond"]),
                    average(item["selectedTokensPerSecond"]),
                    average(item["selectedJsonBytesPerSecond"]),
                    100.0 * average(item["tokenDropRatio"]),
                    100.0 * average(item["importantTokenSendRatio"]),
                    100.0 * average(item["selectionSavingsRatio"]),
                )
            )

    if packet_stats:
        lines.extend(
            [
                "",
                "## Scene Packet Metrics",
                "",
                "| Condition | Packets | Packets/s | Bytes/s | Tokens/packet | Drop % | Important kept % | Avg packet importance |",
                "| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |",
            ]
        )
        for condition in ordered_conditions(packet_stats):
            item = packet_stats[condition]
            duration = packet_duration(item)
            packets = item["packets"]
            lines.append(
                "| {0} | {1} | {2:.2f} | {3:.2f} | {4:.2f} | {5:.1f}% | {6:.1f}% | {7:.3f} |".format(
                    condition,
                    packets,
                    packets / duration if duration > 0 else 0.0,
                    item["estimated_bytes"] / duration if duration > 0 else 0.0,
                    item["selected_tokens"] / packets if packets > 0 else 0.0,
                    pct(item["dropped_tokens"], item["generated_tokens"]),
                    pct(item["important_kept"], item["important_tokens"]),
                    average(item["importance"]),
                )
            )

    lines.extend(
        [
            "",
            "## Weekly Report Draft",
            "",
            (
                "\u4eca\u9031\u306f\u3001Scene Token \u5b9f\u9a13\u30ed\u30b0\u3092\u7814\u7a76\u8a55\u4fa1\u306b\u4f7f\u3048\u308b\u5f62\u3067\u96c6\u8a08\u3059\u308b\u305f\u3081\u306e\u89e3\u6790\u30d1\u30a4\u30d7\u30e9\u30a4\u30f3\u3092\u6574\u7406\u3057\u305f\u3002"
                f"\u73fe\u5728\u306e\u30ed\u30b0\u3067\u306f {condition_text} \u306e\u4e3b\u6761\u4ef6\u3092\u78ba\u8a8d\u3067\u304d\u3001Token \u30ed\u30b0\u306b\u306f\u5408\u8a08 {total_token_rows} \u884c\u304c\u8a18\u9332\u3055\u308c\u3066\u3044\u308b\u3002"
                "\u5404\u6761\u4ef6\u306b\u3064\u3044\u3066\u3001\u767a\u8a71\u72b6\u614b\u3001TurnState\u3001SemanticToken\u3001\u8a71\u8005\u3054\u3068\u306e\u51fa\u73fe\u72b6\u6cc1\u3092\u96c6\u8a08\u3067\u304d\u308b\u3088\u3046\u306b\u306a\u3063\u305f\u3002"
                "\u307e\u305f Metrics \u30ed\u30b0\u304b\u3089 JSON \u8868\u73fe\u3001Compact \u8868\u73fe\u3001Object Metadata \u76f8\u5f53\u306e\u901a\u4fe1\u91cf\u6307\u6a19\u3092\u6bd4\u8f03\u3067\u304d\u308b\u305f\u3081\u3001"
                "Scene Token \u304c\u7a7a\u9593\u60c5\u5831\u3068\u4f1a\u8a71\u72b6\u614b\u3092\u3069\u306e\u7a0b\u5ea6\u306e\u8868\u73fe\u91cf\u3067\u6271\u3048\u308b\u304b\u3092\u78ba\u8a8d\u3067\u304d\u308b\u3002"
            ),
            "",
            render_weekly_response_paragraph(
                has_participant_responses,
                total_direction_responses,
                total_speaker_responses,
                condition_text,
            ),
            "",
        ]
    )
    return "\n".join(lines)


def render_weekly_response_paragraph(has_participant_responses, total_direction_responses, total_speaker_responses, condition_text):
    if has_participant_responses:
        return (
            "\u53c2\u52a0\u8005\u5fdc\u7b54\u306b\u3064\u3044\u3066\u3082\u3001"
            f"Direction response \u3092 {total_direction_responses} \u4ef6\u3001Speaker response \u3092 {total_speaker_responses} \u4ef6\u8a18\u9332\u3067\u304d\u305f\u3002"
            f"\u3053\u308c\u306b\u3088\u308a\u3001{condition_text} \u306e\u6761\u4ef6\u306b\u3064\u3044\u3066\u3001"
            "\u65b9\u5411\u56de\u7b54\u7cbe\u5ea6\u3001\u8a71\u8005\u56de\u7b54\u7cbe\u5ea6\u3001\u53cd\u5fdc\u6642\u9593\u3092\u6761\u4ef6\u3054\u3068\u306b\u96c6\u8a08\u3067\u304d\u308b\u72b6\u614b\u306b\u306a\u3063\u305f\u3002"
            "\u4e00\u65b9\u3067\u3001\u4e00\u90e8\u306e\u5fdc\u7b54\u306f\u767a\u8a71\u8005\u304c\u660e\u78ba\u3067\u306a\u3044\u6642\u70b9\u306b\u884c\u308f\u308c\u305f\u305f\u3081 ambiguous \u3068\u3057\u3066\u8a18\u9332\u3055\u308c\u305f\u3002"
            "\u6b21\u306e\u30d1\u30a4\u30ed\u30c3\u30c8\u3067\u306f\u3001HUD \u4e0a\u306e\u5fdc\u7b54\u30bf\u30a4\u30df\u30f3\u30b0\u8868\u793a\u3084\u6307\u793a\u6587\u3092\u6539\u5584\u3057\u3001"
            "\u66d6\u6627\u5fdc\u7b54\u3092\u6e1b\u3089\u3059\u3002"
        )

    return (
        "\u4e00\u65b9\u3067\u3001\u73fe\u5728\u306e\u4ee3\u8868\u30ed\u30b0\u306b\u306f\u53c2\u52a0\u8005\u5fdc\u7b54\u304c\u307e\u3060\u542b\u307e\u308c\u3066\u3044\u306a\u3044\u3002"
        f"Direction response \u306f {total_direction_responses} \u4ef6\u3001Speaker response \u306f {total_speaker_responses} \u4ef6\u3067\u3042\u308a\u3001"
        "\u65b9\u5411\u56de\u7b54\u7cbe\u5ea6\u3001\u8a71\u8005\u56de\u7b54\u7cbe\u5ea6\u3001\u53cd\u5fdc\u6642\u9593\u3092\u8a55\u4fa1\u3059\u308b\u306b\u306f Unity Editor \u4e0a\u3067\u518d\u53ce\u9332\u304c\u5fc5\u8981\u3067\u3042\u308b\u3002"
    )

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
    packet_files, packet_stats = collect_packet_stats(root)
    markdown = render_markdown(root, token_files, token_stats, metric_files, metric_stats, event_files, event_stats, packet_files, packet_stats)

    if len(sys.argv) == 3:
        output_path = Path(sys.argv[2])
        output_path.write_text(markdown, encoding="utf-8")
        print(f"Wrote {output_path}")
    else:
        print(markdown)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

