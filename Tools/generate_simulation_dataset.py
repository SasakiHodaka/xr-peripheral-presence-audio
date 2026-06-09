#!/usr/bin/env python
import argparse
import csv
import math
import random
from pathlib import Path


DEFAULT_OUTPUT = Path("cue_training_dataset.csv")

DISTANCES = (0.75, 1.5, 3.0, 5.0, 7.0)
ANGLES = (-150.0, -90.0, -45.0, 0.0, 45.0, 90.0, 150.0, 180.0)
MOTION_STATES = ("Static", "ApproachingSlow", "ApproachingFast", "Leaving", "Crossing")
BOOLS = (False, True)

FIELDNAMES = (
    "sampleId",
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
    "directionLabel",
    "viewState",
    "motionState",
    "labelSource",
    "cueType",
    "presenceScore",
    "volumeGain",
    "cueLowPassHz",
    "cueReverbAmount",
    "cueOcclusionGain",
)


def clamp01(value):
    return max(0.0, min(1.0, value))


def inverse_lerp(start, end, value):
    if abs(end - start) < 1e-9:
        return 0.0
    return clamp01((value - start) / (end - start))


def lerp(start, end, amount):
    return start + (end - start) * amount


def local_position(distance, angle_degrees):
    radians = math.radians(angle_degrees)
    return (
        math.sin(radians) * distance,
        0.0,
        math.cos(radians) * distance,
    )


def view_state(angle_degrees):
    angle = abs(delta_angle(0.0, angle_degrees))
    if angle <= 50.0:
        return "InView"
    if angle <= 85.0:
        return "Peripheral"
    return "OutOfView"


def delta_angle(current, target):
    return (target - current + 180.0) % 360.0 - 180.0


def direction_label(local_x, local_z):
    abs_x = abs(local_x)
    abs_z = abs(local_z)

    if abs_x < 0.35 and local_z >= 0.0:
        return "Front"
    if abs_x < 0.35 and local_z < 0.0:
        return "Back"
    if abs_z < 0.35:
        return "Left" if local_x < 0.0 else "Right"
    if local_z >= 0.0:
        return "FrontLeft" if local_x < 0.0 else "FrontRight"
    return "BackLeft" if local_x < 0.0 else "BackRight"


def radial_speed(motion_state):
    if motion_state == "ApproachingSlow":
        return 0.45
    if motion_state == "ApproachingFast":
        return 1.4
    if motion_state == "Leaving":
        return -0.6
    return 0.0


def objective_presence_score(distance, out_of_view, radial, speaking, crossing, gazing, weights):
    distance_urgency = 1.0 - inverse_lerp(0.5, 7.5, distance)
    approach_urgency = inverse_lerp(0.0, 1.6, radial)
    return clamp01(
        weights["distance"] * distance_urgency
        + weights["out_of_view"] * float(out_of_view)
        + weights["approach"] * approach_urgency
        + weights["speaking"] * float(speaking)
        + weights["crossing"] * float(crossing)
        + weights["gazing"] * float(gazing)
    )


def cue_type(score, speaking, approaching, crossing, out_of_view, near):
    if score <= 0.05:
        return "None"
    if speaking:
        return "Voice"
    if approaching:
        return "Footstep"
    if crossing or out_of_view or near:
        return "AmbientPresence"
    return "None"


def build_row(index, distance, angle, motion_state, speaking, gazing, args):
    x, y, z = local_position(distance, angle)
    state = view_state(angle)
    out_of_view = state == "OutOfView"
    near = distance <= args.near_distance
    crossing = motion_state == "Crossing"
    radial = radial_speed(motion_state)
    approaching = radial > 0.0
    lateral = (1.2 if x >= 0.0 else -1.2) if crossing else 0.0
    weights = {
        "distance": args.distance_weight,
        "out_of_view": args.out_of_view_weight,
        "approach": args.approach_weight,
        "speaking": args.speaking_weight,
        "crossing": args.crossing_weight,
        "gazing": args.gazing_weight,
    }
    score = objective_presence_score(distance, out_of_view, radial, speaking, crossing, gazing, weights)
    distance01 = inverse_lerp(0.5, 7.5, distance)
    occlusion01 = clamp01((0.55 if out_of_view else 0.0) + (0.45 if z < 0.0 else 0.0))
    occlusion_gain = clamp01(1.0 - args.environment_occlusion_strength * occlusion01)
    distance_attenuation = lerp(1.0, 1.0 - args.environment_distance_attenuation, distance01)
    selected_cue = cue_type(score, speaking, approaching, crossing, out_of_view, near)
    volume_gain = 0.0 if selected_cue == "None" else clamp01(score * distance_attenuation * occlusion_gain)
    low_pass = min(
        lerp(args.clear_low_pass_hz, args.far_low_pass_hz, distance01),
        lerp(args.clear_low_pass_hz, args.occluded_low_pass_hz, args.environment_occlusion_strength * occlusion01),
    )

    return {
        "sampleId": f"SIM_{index:05d}",
        "conditionLabel": "Simulation",
        "cueCondition": "SimulationObjective",
        "roomScale": f"{args.room_scale:.6f}",
        "materialClass": args.material_class,
        "environmentReverbAmount": f"{args.environment_reverb_amount:.6f}",
        "environmentOcclusionStrength": f"{args.environment_occlusion_strength:.6f}",
        "environmentDistanceAttenuation": f"{args.environment_distance_attenuation:.6f}",
        "environmentRt60": f"{args.environment_rt60:.6f}",
        "environmentDrr": f"{args.environment_drr:.6f}",
        "targetId": "SimTarget",
        "outOfView": str(out_of_view),
        "approaching": str(approaching),
        "speaking": str(speaking),
        "gazing": str(gazing),
        "near": str(near),
        "crossing": str(crossing),
        "distance": f"{distance:.6f}",
        "viewAngle": f"{abs(delta_angle(0.0, angle)):.6f}",
        "radialSpeed": f"{radial:.6f}",
        "lateralSpeed": f"{lateral:.6f}",
        "localX": f"{x:.6f}",
        "localY": f"{y:.6f}",
        "localZ": f"{z:.6f}",
        "directionLabel": direction_label(x, z),
        "viewState": state,
        "motionState": motion_state,
        "labelSource": "objective_simulation",
        "cueType": selected_cue,
        "presenceScore": f"{0.0 if selected_cue == 'None' else score:.6f}",
        "volumeGain": f"{volume_gain:.6f}",
        "cueLowPassHz": f"{low_pass:.6f}",
        "cueReverbAmount": f"{0.0 if selected_cue == 'None' else args.environment_reverb_amount:.6f}",
        "cueOcclusionGain": f"{occlusion_gain:.6f}",
    }


def iter_grid_rows(args):
    index = 0
    for distance in DISTANCES:
        for angle in ANGLES:
            for motion_state in MOTION_STATES:
                for speaking in BOOLS:
                    for gazing in BOOLS:
                        yield build_row(index, distance, angle, motion_state, speaking, gazing, args)
                        index += 1


def iter_random_rows(args):
    rng = random.Random(args.seed)
    for index in range(args.random_count):
        distance = rng.uniform(args.min_distance, args.max_distance)
        angle = rng.uniform(-180.0, 180.0)
        motion_state = rng.choice(MOTION_STATES)
        speaking = rng.random() < args.speaking_probability
        gazing = rng.random() < args.gazing_probability
        yield build_row(index, distance, angle, motion_state, speaking, gazing, args)


def write_rows(path, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    count = 0
    with path.open("w", encoding="utf-8-sig", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=FIELDNAMES)
        writer.writeheader()
        for row in rows:
            writer.writerow(row)
            count += 1
    return count


def main():
    parser = argparse.ArgumentParser(description="Generate objective simulation labels for peripheral cue learning.")
    parser.add_argument("--output", default=str(DEFAULT_OUTPUT), help="Output CSV path.")
    parser.add_argument("--mode", choices=("grid", "random"), default="grid", help="Generation mode.")
    parser.add_argument("--random-count", type=int, default=5000, help="Number of random samples.")
    parser.add_argument("--seed", type=int, default=13, help="Random seed.")
    parser.add_argument("--min-distance", type=float, default=0.5)
    parser.add_argument("--max-distance", type=float, default=8.0)
    parser.add_argument("--near-distance", type=float, default=1.5)
    parser.add_argument("--speaking-probability", type=float, default=0.25)
    parser.add_argument("--gazing-probability", type=float, default=0.25)
    parser.add_argument("--distance-weight", type=float, default=0.35)
    parser.add_argument("--out-of-view-weight", type=float, default=0.2)
    parser.add_argument("--approach-weight", type=float, default=0.25)
    parser.add_argument("--speaking-weight", type=float, default=0.15)
    parser.add_argument("--crossing-weight", type=float, default=0.15)
    parser.add_argument("--gazing-weight", type=float, default=0.1)
    parser.add_argument("--room-scale", type=float, default=1.0)
    parser.add_argument("--material-class", default="Neutral")
    parser.add_argument("--environment-reverb-amount", type=float, default=0.25)
    parser.add_argument("--environment-occlusion-strength", type=float, default=0.35)
    parser.add_argument("--environment-distance-attenuation", type=float, default=0.35)
    parser.add_argument("--environment-rt60", type=float, default=0.45)
    parser.add_argument("--environment-drr", type=float, default=6.0)
    parser.add_argument("--clear-low-pass-hz", type=float, default=22000.0)
    parser.add_argument("--occluded-low-pass-hz", type=float, default=4500.0)
    parser.add_argument("--far-low-pass-hz", type=float, default=9000.0)
    args = parser.parse_args()

    rows = iter_grid_rows(args) if args.mode == "grid" else iter_random_rows(args)
    output = Path(args.output)
    count = write_rows(output, rows)
    print(f"Generated {count} simulation rows: {output}")


if __name__ == "__main__":
    main()
