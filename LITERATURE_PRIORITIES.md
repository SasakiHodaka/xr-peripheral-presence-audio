# Literature Priorities

This document organizes related work by relevance to the project.

The project axis is:

```text
CPS / Human-Robot Collaboration / Spatial Audio / Multimodal Learning / XR
-> SoundSpaces / RIR / Environment Acoustics / Spatial Computing
-> Unity peripheral presence audio cues
```

## Reading Priority

### Highest Priority

1. **Few-Shot Audio-Visual Learning of Environment Acoustics**  
   Why: closest match to the planned `environmentAcousticProfile` estimator.  
   Use for:
   - RGB-D + echo + pose input design
   - few-shot acoustic prediction
   - arbitrary source-receiver RIR prediction
   - environment acoustics profile estimation  
   Link: https://proceedings.neurips.cc/paper_files/paper/2022/hash/113ae3a9762ca2168f860a8501d6ae25-Abstract-Conference.html

2. **SoundSpaces / SoundSpaces 2.0**  
   Why: closest match to the planned large-scale audio-visual simulation pipeline.  
   Use for:
   - random source/listener placement
   - dry-to-wet audio rendering
   - RIR dataset generation
   - embodied audio-visual learning  
   Links:
   - SoundSpaces: https://arxiv.org/abs/1912.11474
   - SoundSpaces 2.0: https://arxiv.org/abs/2206.08312
   - Project: https://soundspaces.org/

3. **Spatial Computing and Intuitive Interaction: Bringing Mixed Reality and Robotics Together**  
   Why: strong framing for XR, spatial computing, and human-robot collaboration.  
   Use for:
   - CPS / HRC motivation
   - mixed reality as a spatial interaction interface
   - human-aware systems in shared spaces  
   Links:
   - Microsoft Research: https://www.microsoft.com/en-us/research/publication/spatial-computing-and-intuitive-interaction-bringing-mixed-reality-and-robotics-together/
   - arXiv: https://arxiv.org/abs/2202.01493

4. **VRBubble: Enhancing Peripheral Awareness of Avatars for People with Visual Impairments in Social Virtual Reality**  
   Why: very close to the project's peripheral awareness and spatial audio cue design.  
   Use for:
   - peripheral avatar awareness
   - spatial audio feedback
   - social distance / proxemic cue design
   - evaluation of audio-based awareness cues  
   Link: https://arxiv.org/abs/2208.11071

5. **The Role of Consequential and Functional Sound in Human-Robot Interaction: Toward Audio Augmented Reality Interfaces**  
   Why: directly supports the idea that functional sound can shape awareness and collaboration in HRI.  
   Use for:
   - presence and awareness through sound
   - functional auditory cue design
   - HRI sound design framing  
   Link: https://arxiv.org/abs/2511.15956

### Next Priority

6. **Learning Neural Acoustic Fields**  
   Why: source-receiver acoustic field modeling background.  
   Use for:
   - neural acoustic representation
   - RIR prediction from emitter/listener position
   - conceptual basis for future learned acoustic fields  
   Link: https://arxiv.org/abs/2204.00628

7. **AST: Audio Spectrogram Transformer**  
   Why: candidate audio encoder for self-supervised pre-training and downstream environment estimation.  
   Use for:
   - spectrogram-based audio representation
   - transformer audio encoder baseline  
   Link: https://arxiv.org/abs/2104.01778

8. **AudioMiXR: Spatial Audio Object Manipulation with 6DoF for Sound Design in Augmented Reality**  
   Why: useful reference for XR spatial audio interaction and manipulation interfaces.  
   Use for:
   - spatial audio object interaction
   - XR audio interface design
   - Unity-based spatial audio manipulation  
   Link: https://www.cs.ucf.edu/~jjl/pubs/woodward2025.pdf

## Topic Map

### A. Spatial Acoustics / RIR / NAF

High-confidence references:

- Learning Neural Acoustic Fields
- Few-Shot Audio-Visual Learning of Environment Acoustics

Candidate references to verify:

- Neural Acoustic Fields Grow on Trees
- Neural Sound Field Rendering

How this maps to the project:

```text
source position + listener position + scene/acoustic context
-> RIR / RT60 / DRR / acoustic profile
-> environment-adaptive cue parameters
```

### B. SoundSpaces / Embodied Audio

High-confidence references:

- SoundSpaces
- SoundSpaces 2.0
- SoundSpaces project and Habitat-based tools

How this maps to the project:

```text
3D environment
-> random source/listener sampling
-> dry/wet audio + RIR + RGB-D + pose
-> self-supervised audio-visual learning
```

### C. Human-Robot Collaboration / CPS

High-confidence references:

- Spatial Computing and Intuitive Interaction: Bringing Mixed Reality and Robotics Together

Candidate references to verify:

- Engineering Human-in-the-Loop Interactions in CPS
- Human-Robot Collaboration in Mixed Reality
- Multimodal Perception-Driven Decision-Making for HRI

How this maps to the project:

```text
human-centered CPS / HRC
-> shared-space awareness
-> multimodal XR interface
-> peripheral presence audio cues
```

### D. XR / Spatial Computing / Spatial Audio Interfaces

High-confidence references:

- Spatial Computing and Intuitive Interaction
- VRBubble
- AudioMiXR

Candidate references to verify:

- XR-DT: XR Enhanced Digital Twin for Mobile Robots

How this maps to the project:

```text
spatial computing + XR
-> user-centered awareness interface
-> adaptive sound cues in Unity
```

### E. Presence / Awareness / Functional Sound

High-confidence references:

- VRBubble
- The Role of Consequential and Functional Sound in HRI

Candidate references to verify:

- Sonic Interaction Design

How this maps to the project:

```text
sound cue design
-> peripheral awareness
-> social / robot / avatar presence
-> less visual-load-dependent interaction
```

### F. Self-Supervised / Multimodal Learning

High-confidence references:

- AST: Audio Spectrogram Transformer
- CLIP, as a general contrastive multimodal learning concept

Candidate references to verify:

- AVID-CMA
- SSAST
- MAST / SS-MAST

How this maps to the project:

```text
audio encoder + visual encoder + position encoder
-> shared acoustic environment representation
-> environmentAcousticProfile
```

## Implementation References

High-confidence implementation references:

- Microsoft spatialaudio-unity: https://github.com/microsoft/spatialaudio-unity
- Microsoft Spatial Sound documentation: https://learn.microsoft.com/en-us/windows/win32/coreaudio/spatial-sound
- SoundSpaces project: https://soundspaces.org/
- Habitat: https://github.com/facebookresearch/habitat-sim

Project-local implementation target:

```text
PeripheralCueModel
-> PeripheralCueAudioEmitter
-> EnvironmentAcousticProfile
-> experiment condition comparison
```

## Current Working Interpretation

The most important center of gravity is:

```text
SoundSpaces 2.0 for data generation
Few-Shot Audio-Visual Environment Acoustics for environment estimation
VRBubble + functional HRI sound for cue design
Spatial Computing / MR Robotics for CPS framing
Unity PeripheralCueModel for implementation
```

Audio2Face remains a later optional extension for the `Speaking` condition, not the core environment acoustics pipeline.

## Method Selection Decision

NAF-style methods are currently background references only. The project should not use NAF as the first AI training target.

Selected core method:

```text
Few-Shot Audio-Visual Learning of Environment Acoustics
```

Reason:

- It is the closest match to the desired estimator:

```text
RGB-D / echo / pose / source-listener query
-> environment acoustics
-> compact environmentAcousticProfile
```

- It supports few-shot adaptation to new environments, which matches a realistic XR setting better than training a full scene-specific acoustic field.
- Its target output, RIR or RIR-derived acoustic parameters, can be simplified into Unity-facing values:
  - `rt60`
  - `drr`
  - `reverbAmount`
  - `occlusionStrength`
  - `distanceAttenuation`
  - `lowPassHz`

Selected data generation method:

```text
SoundSpaces 2.0
```

Reason:

- It provides the most suitable simulation path for creating paired visual-acoustic data.
- It supports arbitrary source/listener sampling, material configuration, and geometry-based audio rendering.
- It can generate the training labels needed by the selected estimator, including RIR-like targets and acoustic metadata.

Selected cue-design reference:

```text
VRBubble + Functional Sound in HRI
```

Reason:

- VRBubble directly supports peripheral avatar awareness through spatial audio.
- Functional HRI sound work supports the argument that designed spatial sound can communicate task-relevant presence information without relying only on visual attention.

Practical project choice:

```text
SoundSpaces 2.0 dataset
-> Few-Shot-style environment estimator
-> EnvironmentAcousticProfile
-> Unity PeripheralCueModel / PeripheralCueAudioEmitter
```

First AI version should not predict full RIR in Unity. It should predict compact cue-control parameters first:

```text
distance, direction, state flags, room/acoustic metadata
-> cueType, presenceScore, volumeGain, lowPassHz, reverbAmount, occlusionStrength
```

This gives an early trainable system while preserving a clean path toward full RIR or richer acoustic prediction later.

