import shutil
import sys
from pathlib import Path


LOG_PATTERNS = [
    "scene_tokens_*.csv",
    "scene_token_events_*.csv",
    "scene_token_metrics_*.csv",
]


def latest_file(root, pattern):
    files = sorted(root.glob(pattern), key=lambda path: path.stat().st_mtime, reverse=True)
    return files[0] if files else None


def main():
    if len(sys.argv) != 3:
        print("Usage: python Tools/collect_latest_scene_token_run.py <log_directory> <output_directory>")
        return 1

    root = Path(sys.argv[1])
    output = Path(sys.argv[2])

    if not root.exists() or not root.is_dir():
        print(f"Log directory not found: {root}")
        return 1

    output.mkdir(parents=True, exist_ok=True)

    copied = []
    missing = []
    for pattern in LOG_PATTERNS:
        source = latest_file(root, pattern)
        if source is None:
            missing.append(pattern)
            continue

        target = output / source.name
        shutil.copy2(source, target)
        copied.append(target)

    for path in copied:
        print(f"Copied {path}")

    if missing:
        print("Missing log types: " + ", ".join(missing))
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
