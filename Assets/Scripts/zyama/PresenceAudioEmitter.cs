using UnityEngine;

public class PresenceAudioEmitter : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepSource;
    public AudioSource breathingSource;
    public AudioSource clothSource;

    [Header("Footstep Settings")]
    public float footstepBaseInterval = 0.55f;
    public float footstepMinInterval = 0.22f;

    [Header("Smoothing")]
    [Range(0.01f, 20f)] public float volumeLerpSpeed = 8f;

    private float footstepTimer = 0f;

    private float targetFootstepVolume = 0f;
    private float targetBreathingVolume = 0f;
    private float targetClothVolume = 0f;

    private PresenceTarget target;

    private void Awake()
    {
        target = GetComponent<PresenceTarget>();
    }

    private void Start()
    {
        ConfigureSource(footstepSource);
        ConfigureSource(breathingSource);
        ConfigureSource(clothSource);

        if (breathingSource != null && breathingSource.clip != null && !breathingSource.isPlaying)
            breathingSource.Play();

        if (clothSource != null && clothSource.clip != null && !clothSource.isPlaying)
            clothSource.Play();
    }

    private void Update()
    {
        UpdateVolumes();
        UpdateFootsteps();
    }

    private void ConfigureSource(AudioSource src)
    {
        if (src == null) return;

        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = 0.5f;
        src.maxDistance = 12f;
        src.playOnAwake = false;
    }

    public void SetPresenceAudio(float presenceScore, float movementAmount, float gazeAttention, float distance01)
    {
        // presenceScore: 0～1想定
        // movementAmount: 0～1
        // gazeAttention: 0～1
        // distance01: 0(近)～1(遠)

        targetFootstepVolume = Mathf.Clamp01(presenceScore * movementAmount * 0.9f);

        // 近いほど呼吸が分かりやすい
        float nearFactor = 1f - distance01;
        targetBreathingVolume = Mathf.Clamp01(presenceScore * nearFactor * 0.6f + gazeAttention * 0.15f);

        // 衣擦れは移動と存在感の中間
        targetClothVolume = Mathf.Clamp01(presenceScore * (0.3f + 0.7f * movementAmount) * 0.45f);

        // 遠いほど少しこもる感じを簡易的に再現
        float lowPassLike = Mathf.Lerp(22000f, 3500f, distance01);

        if (breathingSource != null)
            breathingSource.pitch = Mathf.Lerp(0.95f, 1.08f, gazeAttention);

        if (clothSource != null)
            clothSource.pitch = Mathf.Lerp(0.9f, 1.05f, movementAmount);

        // 実際にローパスをかけるなら AudioLowPassFilter を追加して制御してもよい
        AudioLowPassFilter lp1 = breathingSource ? breathingSource.GetComponent<AudioLowPassFilter>() : null;
        AudioLowPassFilter lp2 = clothSource ? clothSource.GetComponent<AudioLowPassFilter>() : null;

        if (lp1 != null) lp1.cutoffFrequency = lowPassLike;
        if (lp2 != null) lp2.cutoffFrequency = lowPassLike;
    }

    private void UpdateVolumes()
    {
        if (footstepSource != null)
        {
            footstepSource.volume = Mathf.Lerp(
                footstepSource.volume,
                targetFootstepVolume,
                Time.deltaTime * volumeLerpSpeed
            );
        }

        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.Lerp(
                breathingSource.volume,
                targetBreathingVolume,
                Time.deltaTime * volumeLerpSpeed
            );
        }

        if (clothSource != null)
        {
            clothSource.volume = Mathf.Lerp(
                clothSource.volume,
                targetClothVolume,
                Time.deltaTime * volumeLerpSpeed
            );
        }
    }

    private void UpdateFootsteps()
    {
        if (target == null || footstepSource == null || footstepSource.clip == null) return;
        if (!target.isMoving) return;
        if (footstepSource.volume < 0.03f) return;

        float speed = target.velocity.magnitude;
        float t = Mathf.InverseLerp(0.1f, 2.5f, speed);
        float interval = Mathf.Lerp(footstepBaseInterval, footstepMinInterval, t);

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= interval)
        {
            footstepSource.pitch = Mathf.Lerp(0.9f, 1.2f, t);
            footstepSource.PlayOneShot(footstepSource.clip, footstepSource.volume);
            footstepTimer = 0f;
        }
    }
}
