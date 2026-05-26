using UnityEngine;

public class PresenceAudioManager : MonoBehaviour
{
    [Header("Player")]
    public Transform localPlayerHead;
    public Transform localPlayerBody;

    [Header("Targets")]
    public PresenceTarget[] targets;

    [Header("Presence Weights")]
    [Range(0f, 3f)] public float distanceWeight = 1.2f;
    [Range(0f, 3f)] public float approachWeight = 1.0f;
    [Range(0f, 3f)] public float gazeWeight = 0.9f;
    [Range(0f, 3f)] public float roleWeight = 0.8f;
    [Range(0f, 3f)] public float speakingWeight = 0.5f;

    [Header("Distance Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 10f;

    [Header("Attention Control")]
    [Tooltip("視野外の相手を強調する")]
    [Range(0f, 2f)] public float outOfViewBoost = 0.5f;

    [Tooltip("同時に強く提示する人数")]
    [Range(1, 8)] public int topKTargets = 3;

    private void Reset()
    {
        localPlayerHead = Camera.main ? Camera.main.transform : transform;
        localPlayerBody = transform;
    }

    private void Update()
    {
        if (localPlayerHead == null) return;
        if (targets == null || targets.Length == 0) return;

        float[] scores = new float[targets.Length];

        // まず全員の存在感を計算
        for (int i = 0; i < targets.Length; i++)
        {
            PresenceTarget t = targets[i];
            if (t == null) continue;

            scores[i] = ComputePresenceScore(t);
        }

        // Top-K以外は抑制する
        bool[] activeMask = BuildTopKMask(scores, topKTargets);

        for (int i = 0; i < targets.Length; i++)
        {
            PresenceTarget t = targets[i];
            if (t == null) continue;

            PresenceAudioEmitter emitter = t.GetComponent<PresenceAudioEmitter>();
            if (emitter == null) continue;

            float score = activeMask[i] ? scores[i] : scores[i] * 0.25f;

            Vector3 toTarget = t.bodyTransform.position - localPlayerHead.position;
            float dist = toTarget.magnitude;

            float distance01 = Mathf.InverseLerp(minDistance, maxDistance, dist);
            float move01 = Mathf.InverseLerp(0.05f, 2.0f, t.velocity.magnitude);
            float gaze01 = ComputeGazeAttention(t);

            emitter.SetPresenceAudio(
                Mathf.Clamp01(score),
                Mathf.Clamp01(move01),
                Mathf.Clamp01(gaze01),
                Mathf.Clamp01(distance01)
            );
        }
    }

    private float ComputePresenceScore(PresenceTarget target)
    {
        Vector3 toTarget = target.bodyTransform.position - localPlayerHead.position;
        float dist = toTarget.magnitude;

        // 1) 距離要因
        float distanceFactor = 1f - Mathf.InverseLerp(minDistance, maxDistance, dist);
        distanceFactor = Mathf.Clamp01(distanceFactor);

        // 2) 接近要因（相手がこちらに近づいてくるほど大きい）
        Vector3 dirToPlayer = (localPlayerHead.position - target.bodyTransform.position).normalized;
        float approachSpeed = Vector3.Dot(target.velocity, dirToPlayer);
        float approachFactor = Mathf.InverseLerp(-1.0f, 1.5f, approachSpeed);
        approachFactor = Mathf.Clamp01(approachFactor);

        // 3) 視線要因（見られている感）
        float gazeFactor = ComputeGazeAttention(target);

        // 4) 役割要因
        float socialFactor = target.GetRoleWeight();

        // 5) 発話要因
        float speakingFactor = target.isSpeaking ? 1f : 0f;

        // 6) 視野外ブースト
        Vector3 localForward = localPlayerHead.forward;
        float viewDot = Vector3.Dot(localForward.normalized, toTarget.normalized);
        float outOfViewFactor = (viewDot < 0.2f) ? outOfViewBoost : 0f;

        float raw =
            distanceWeight * distanceFactor +
            approachWeight * approachFactor +
            gazeWeight * gazeFactor +
            roleWeight * (socialFactor / 1.2f) +
            speakingWeight * speakingFactor +
            outOfViewFactor;

        // 0～1に正規化
        float normalized = raw / (
            distanceWeight +
            approachWeight +
            gazeWeight +
            roleWeight +
            speakingWeight +
            Mathf.Max(outOfViewBoost, 0f)
        );

        return Mathf.Clamp01(normalized);
    }

    private float ComputeGazeAttention(PresenceTarget target)
    {
        if (target.headTransform == null) return 0f;

        Vector3 toPlayer = (localPlayerHead.position - target.headTransform.position).normalized;
        Vector3 gazeForward = target.headTransform.forward.normalized;

        float dot = Vector3.Dot(gazeForward, toPlayer);

        // 正面に近いほど 1
        return Mathf.InverseLerp(0.2f, 0.95f, dot);
    }

    private bool[] BuildTopKMask(float[] scores, int k)
    {
        bool[] mask = new bool[scores.Length];
        if (scores.Length == 0) return mask;

        for (int count = 0; count < Mathf.Min(k, scores.Length); count++)
        {
            float maxValue = float.MinValue;
            int maxIndex = -1;

            for (int i = 0; i < scores.Length; i++)
            {
                if (mask[i]) continue;
                if (scores[i] > maxValue)
                {
                    maxValue = scores[i];
                    maxIndex = i;
                }
            }

            if (maxIndex >= 0)
                mask[maxIndex] = true;
        }

        return mask;
    }
}
