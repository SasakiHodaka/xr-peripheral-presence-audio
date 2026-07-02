import csv
import sys
from collections import Counter, defaultdict
from pathlib import Path


EXPECTED_CONDITIONS = [
    "TRADITIONAL",
    "DIRECTION_DISTANCE",
    "FULL_SCENE_TOKEN",
]

MAIN_CONDITION_ORDER = EXPECTED_CONDITIONS

EXPECTED_SCRIPTED_SEMANTICS = {
    "QUESTION",
    "ANSWER",
    "INSTRUCTION",
    "WARNING",
    "AGREEMENT",
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
                condition = row.get("condition", "") or "(none)"
                for field in (
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
    direction_scored = sum(item["direction_scored"] for item in event_stats.values())
    speaker_scored = sum(item["speaker_scored"] for item in event_stats.values())
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
    ]


def render_markdown(root, token_files, token_stats, metric_files, metric_stats, event_files, event_stats):
    lines = [
        "# Scene Token Experiment Summary",
        "",
        f"- Log directory: `{root}`",
        f"- Token log files: {len(token_files)}",
        f"- Metrics log files: {len(metric_files)}",
        f"- Event log files: {len(event_files)}",
        "",
        "## Quality Checks",
        "",
        "| Check | Result | Details |",
        "| --- | --- | --- |",
    ]

    for name, ok, details in quality_checks(token_stats, metric_stats, event_stats):
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
            "| Condition | Tokens/s | JSON B/s | Compact B/s | Object metadata B/s | Compact saving |",
            "| --- | ---: | ---: | ---: | ---: | ---: |",
        ]
    )
    for condition in ordered_conditions(metric_stats):
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

    lines.extend(
        [
            "",
            "## Weekly Report Draft",
            "",
            "本週は、Scene Token実験ログを集計するための解析パイプラインを整理した。"
            "Tokenログでは、各条件における発話状態、TurnState、SemanticTokenの出現状況を確認できる。"
            "Eventログでは、方向回答と話者回答の正答率、反応時間、曖昧回答数を条件ごとに集計できる。"
            "MetricsログからはJSON表現、Compact表現、Object Metadata相当の通信量指標を比較できる。",
            "",
            "今後はUnity Editor上で5条件すべての実ログを取得し、Scene Tokenの追加情報が"
            "話者把握、方向把握、会話理解に与える影響を分析する。"
            "現時点では通信量削減を主張の中心に置かず、空間情報と会話状態を統合した離散表現が"
            "VR音声コミュニケーション理解を支援するかを評価対象とする。",
            "",
        ]
    )
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
