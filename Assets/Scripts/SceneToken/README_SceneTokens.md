# Scene Token Prototype

This prototype implements the first research step for spatial conversation tokens:

`Speaker Object -> Scene Token -> CSV log -> Scene Token Decoder -> Spatial Audio Renderer`

## Unity Usage

1. Open `My project` in Unity.
2. Run `Tools > Scene Tokens > Create Spatial Conversation Mock Scene`.
3. Press Play.
4. Press `A`, `B`, or `C` to toggle each avatar's speaking state.
5. Press `Q`, `W`, or `E` to cycle each avatar's semantic token.
6. Press `1`-`5` to switch evaluation conditions.
7. Press `Space` to start/stop an experiment session.
8. Press `N` to advance to the next condition, or wait for the timer.
9. Watch the `Scene Tokens` debug HUD.
10. CSV logs are written to `Application.persistentDataPath`.

## Token Fields

CSV columns:

`timestamp,sessionId,participantId,trialIndex,trialElapsed,speakerId,azimuth,range,direction,distance,speakingState,turnState,semanticToken,utteranceText,semanticConfidence,condition`

Discrete fields are used for the research token representation.
Continuous fields are kept for analysis and later comparison with MASA/Object metadata.

## Current Scope

- Direction: 8 classes
- Distance: `NEAR`, `MID`, `FAR`
- Speaking state: keyboard/audio playback based
- Turn state: single speaker as `TURN_HOLDER`, multiple speakers as `OVERLAPPER`
- Semantic token: manual label first, Whisper/LLM later
- Metrics: JSON-style bytes/s, compact token bytes/s, Object metadata bytes/s
- Rendering: token direction and distance are decoded into AudioSource position and volume
- Experiment session: fixed condition order, trial start/stop events, manual or timed condition advance

If `SpeakerObject.speakingClip` is empty, the component generates a simple looping tone so spatial playback can be tested without recorded voice files.
