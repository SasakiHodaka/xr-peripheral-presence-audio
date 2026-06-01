using System.Collections.Generic;
using UnityEngine;

public struct PeripheralCuePlaybackState
{
    public string targetId;
    public PeripheralCueType cueType;
    public bool playbackActive;
    public float outputVolume;
    public float lowPassHz;
    public float reverbAmount;
    public float footstepInterval;
}

public class PeripheralCueAudioEmitter : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralCueModel cueModel;

    [Header("Cue Clips")]
    public AudioClip footstepClip;
    public AudioClip voiceClip;
    public AudioClip ambientPresenceClip;

    [Header("Playback")]
    public bool enablePlayback = true;
    public float baseVolume = 0.75f;
    public float footstepBaseInterval = 0.55f;
    public float footstepMinInterval = 0.22f;
    public float silentVolumeThreshold = 0.02f;

    [Header("Spatial Audio")]
    public float minDistance = 0.5f;
    public float maxDistance = 12f;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Filtering")]
    public float clearLowPassHz = 22000f;
    public float rearLowPassHz = 5500f;
    public float farLowPassHz = 9000f;

    [Header("Smoothing")]
    [Range(0.01f, 20f)] public float volumeLerpSpeed = 8f;

    private readonly Dictionary<PeripheralTarget, TargetAudioState> targetStates = new Dictionary<PeripheralTarget, TargetAudioState>();
    private readonly Dictionary<string, PeripheralCuePlaybackState> latestPlaybackStates = new Dictionary<string, PeripheralCuePlaybackState>();

    private void Awake()
    {
        if (detector == null)
            detector = GetComponent<PeripheralStateDetector>();

        if (cueModel == null)
            cueModel = GetComponent<PeripheralCueModel>();
    }

    private void Update()
    {
        latestPlaybackStates.Clear();

        if (detector == null || cueModel == null)
            return;

        IReadOnlyList<PeripheralDetectionResult> results = detector.LatestResults;
        for (int i = 0; i < results.Count; i++)
        {
            UpdateTargetAudio(results[i]);
        }
    }

    public PeripheralCuePlaybackState GetPlaybackState(string targetId)
    {
        if (!string.IsNullOrEmpty(targetId) && latestPlaybackStates.TryGetValue(targetId, out PeripheralCuePlaybackState state))
            return state;

        PeripheralCuePlaybackState empty = new PeripheralCuePlaybackState();
        empty.targetId = targetId;
        empty.cueType = PeripheralCueType.None;
        empty.lowPassHz = clearLowPassHz;
        return empty;
    }

    private void UpdateTargetAudio(PeripheralDetectionResult result)
    {
        if (result.target == null)
            return;

        TargetAudioState audioState = GetOrCreateTargetAudio(result.target);
        PeripheralCuePrediction prediction = cueModel.Predict(result);
        bool playbackActive = enablePlayback && prediction.cueType != PeripheralCueType.None && prediction.volumeGain > silentVolumeThreshold;
        float targetVolume = playbackActive ? Mathf.Clamp01(prediction.volumeGain * baseVolume) : 0f;
        float lowPassHz = prediction.lowPassHz > 0f ? prediction.lowPassHz : CalculateLowPassHz(result);

        audioState.SetLowPass(lowPassHz);
        audioState.SetReverb(prediction.reverbAmount);
        audioState.UpdateContinuousSource(PeripheralCueType.Voice, prediction.cueType == PeripheralCueType.Voice ? targetVolume : 0f, volumeLerpSpeed);
        audioState.UpdateContinuousSource(PeripheralCueType.AmbientPresence, prediction.cueType == PeripheralCueType.AmbientPresence ? targetVolume : 0f, volumeLerpSpeed);

        float footstepInterval = CalculateFootstepInterval(result);
        if (prediction.cueType == PeripheralCueType.Footstep && playbackActive)
            audioState.UpdateFootstep(footstepClip, targetVolume, footstepInterval);

        PeripheralCuePlaybackState playbackState = new PeripheralCuePlaybackState();
        playbackState.targetId = result.targetId;
        playbackState.cueType = prediction.cueType;
        playbackState.playbackActive = playbackActive;
        playbackState.outputVolume = targetVolume;
        playbackState.lowPassHz = lowPassHz;
        playbackState.reverbAmount = prediction.reverbAmount;
        playbackState.footstepInterval = footstepInterval;
        latestPlaybackStates[result.targetId] = playbackState;
    }

    private TargetAudioState GetOrCreateTargetAudio(PeripheralTarget target)
    {
        if (targetStates.TryGetValue(target, out TargetAudioState state))
            return state;

        state = new TargetAudioState(target.transform, this);
        targetStates[target] = state;
        return state;
    }

    private float CalculateLowPassHz(PeripheralDetectionResult result)
    {
        bool outOfView = HasState(result.state, PeripheralState.OutOfView);
        float distance01 = Mathf.InverseLerp(cueModel.nearDistance, cueModel.farDistance, result.distance);
        float distanceCutoff = Mathf.Lerp(clearLowPassHz, farLowPassHz, distance01);
        float rearCutoff = outOfView && result.userLocalPosition.z < 0f ? rearLowPassHz : clearLowPassHz;
        return Mathf.Min(distanceCutoff, rearCutoff);
    }

    private float CalculateFootstepInterval(PeripheralDetectionResult result)
    {
        float speed01 = Mathf.InverseLerp(0.1f, 2.5f, Mathf.Abs(result.radialSpeed) + Mathf.Abs(result.lateralSpeed));
        return Mathf.Lerp(footstepBaseInterval, footstepMinInterval, speed01);
    }

    private AudioSource CreateSource(Transform parent, string name, AudioClip clip, bool loop)
    {
        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(parent, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = rolloffMode;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;

        sourceObject.AddComponent<AudioLowPassFilter>();
        return source;
    }

    private static bool HasState(PeripheralState value, PeripheralState state)
    {
        return (value & state) != 0;
    }

    private class TargetAudioState
    {
        private readonly PeripheralCueAudioEmitter owner;
        private readonly AudioSource footstepSource;
        private readonly AudioSource voiceSource;
        private readonly AudioSource ambientSource;
        private float footstepTimer;

        public TargetAudioState(Transform parent, PeripheralCueAudioEmitter owner)
        {
            this.owner = owner;
            footstepSource = owner.CreateSource(parent, "PeripheralFootstepSource", owner.footstepClip, false);
            voiceSource = owner.CreateSource(parent, "PeripheralVoiceSource", owner.voiceClip, true);
            ambientSource = owner.CreateSource(parent, "PeripheralAmbientSource", owner.ambientPresenceClip, true);
        }

        public void SetLowPass(float cutoffHz)
        {
            SetLowPass(footstepSource, cutoffHz);
            SetLowPass(voiceSource, cutoffHz);
            SetLowPass(ambientSource, cutoffHz);
        }

        public void SetReverb(float reverbAmount)
        {
            SetReverb(footstepSource, reverbAmount);
            SetReverb(voiceSource, reverbAmount);
            SetReverb(ambientSource, reverbAmount);
        }

        public void UpdateContinuousSource(PeripheralCueType cueType, float targetVolume, float lerpSpeed)
        {
            AudioSource source = cueType == PeripheralCueType.Voice ? voiceSource : ambientSource;
            if (source == null)
                return;

            source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * lerpSpeed);

            if (source.clip != null && source.volume > owner.silentVolumeThreshold && !source.isPlaying)
                source.Play();

            if (source.isPlaying && source.volume <= owner.silentVolumeThreshold && targetVolume <= owner.silentVolumeThreshold)
                source.Stop();
        }

        public void UpdateFootstep(AudioClip clip, float volume, float interval)
        {
            if (footstepSource == null || clip == null)
                return;

            footstepTimer += Time.deltaTime;
            if (footstepTimer < interval)
                return;

            footstepSource.pitch = Mathf.Lerp(0.92f, 1.18f, Mathf.InverseLerp(owner.footstepBaseInterval, owner.footstepMinInterval, interval));
            footstepSource.PlayOneShot(clip, volume);
            footstepTimer = 0f;
        }

        private static void SetLowPass(AudioSource source, float cutoffHz)
        {
            if (source == null)
                return;

            AudioLowPassFilter filter = source.GetComponent<AudioLowPassFilter>();
            if (filter != null)
                filter.cutoffFrequency = cutoffHz;
        }

        private static void SetReverb(AudioSource source, float reverbAmount)
        {
            if (source != null)
                source.reverbZoneMix = Mathf.Clamp01(reverbAmount);
        }
    }
}
