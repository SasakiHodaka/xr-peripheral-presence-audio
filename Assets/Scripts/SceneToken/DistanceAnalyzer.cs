using UnityEngine;

namespace SceneTokens
{
    public static class DistanceAnalyzer
    {
        public static float CalculateHorizontalRange(Transform listener, Transform speaker)
        {
            if (listener == null || speaker == null)
            {
                return 0f;
            }

            var offset = speaker.position - listener.position;
            return Vector3.ProjectOnPlane(offset, Vector3.up).magnitude;
        }

        public static SceneTokenDistance QuantizeDistance(float range)
        {
            if (range < 1.5f) return SceneTokenDistance.NEAR;
            if (range < 3f) return SceneTokenDistance.MID;
            return SceneTokenDistance.FAR;
        }
    }
}
