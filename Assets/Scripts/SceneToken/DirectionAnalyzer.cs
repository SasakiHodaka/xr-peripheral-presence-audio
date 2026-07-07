using UnityEngine;

namespace SceneTokens
{
    public static class DirectionAnalyzer
    {
        public static float CalculateSignedAzimuth(Transform listener, Transform speaker)
        {
            if (listener == null || speaker == null)
            {
                return 0f;
            }

            var offset = speaker.position - listener.position;
            var flatOffset = Vector3.ProjectOnPlane(offset, Vector3.up);
            if (flatOffset.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            var listenerForward = Vector3.ProjectOnPlane(listener.forward, Vector3.up).normalized;
            var listenerRight = Vector3.ProjectOnPlane(listener.right, Vector3.up).normalized;
            var forwardDot = Vector3.Dot(listenerForward, flatOffset.normalized);
            var rightDot = Vector3.Dot(listenerRight, flatOffset.normalized);
            return Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;
        }

        public static SceneTokenDirection QuantizeDirection(float azimuth)
        {
            if (azimuth >= -22.5f && azimuth < 22.5f) return SceneTokenDirection.FRONT;
            if (azimuth >= 22.5f && azimuth < 67.5f) return SceneTokenDirection.FRONT_RIGHT;
            if (azimuth >= 67.5f && azimuth < 112.5f) return SceneTokenDirection.RIGHT;
            if (azimuth >= 112.5f && azimuth < 157.5f) return SceneTokenDirection.BACK_RIGHT;
            if (azimuth >= -67.5f && azimuth < -22.5f) return SceneTokenDirection.FRONT_LEFT;
            if (azimuth >= -112.5f && azimuth < -67.5f) return SceneTokenDirection.LEFT;
            if (azimuth >= -157.5f && azimuth < -112.5f) return SceneTokenDirection.BACK_LEFT;
            return SceneTokenDirection.BACK;
        }
    }
}
