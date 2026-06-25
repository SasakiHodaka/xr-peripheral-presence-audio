using System.Collections.Generic;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenDecoderRenderer : MonoBehaviour
    {
        public Transform listener;
        public SpeakerObject[] speakers;
        public float nearRadius = 1.1f;
        public float midRadius = 2.2f;
        public float farRadius = 3.6f;
        public bool repositionAudioSources = true;
        public SceneTokenRenderCondition renderCondition = SceneTokenRenderCondition.FULL_SCENE_TOKEN;

        private void Reset()
        {
            listener = Camera.main != null ? Camera.main.transform : null;
            speakers = FindObjectsOfType<SpeakerObject>();
        }

        private void Awake()
        {
            if (listener == null && Camera.main != null)
            {
                listener = Camera.main.transform;
            }
        }

        public void Render(IReadOnlyList<SceneToken> tokens)
        {
            if (listener == null || speakers == null || tokens == null)
            {
                return;
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var speaker = FindSpeaker(token.speakerId);
                if (speaker == null || speaker.audioSource == null)
                {
                    continue;
                }

                if (repositionAudioSources)
                {
                    speaker.audioSource.transform.position = DecodePosition(token);
                }

                speaker.audioSource.volume = DecodeVolume(token);
                speaker.audioSource.pitch = DecodePitch(token);
            }
        }

        private SpeakerObject FindSpeaker(string speakerId)
        {
            for (var i = 0; i < speakers.Length; i++)
            {
                if (speakers[i] != null && speakers[i].speakerId == speakerId)
                {
                    return speakers[i];
                }
            }

            return null;
        }

        private Vector3 DecodePosition(SceneToken token)
        {
            if (renderCondition == SceneTokenRenderCondition.TRADITIONAL)
            {
                return FindSpeaker(token.speakerId).transform.position;
            }

            var direction = DecodeDirectionVector(token.direction);
            var radius = DecodeRadius(token.distance);
            return listener.position + direction * radius;
        }

        private Vector3 DecodeDirectionVector(string direction)
        {
            var forward = Vector3.ProjectOnPlane(listener.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(listener.right, Vector3.up).normalized;

            if (direction == SceneTokenDirection.FRONT_RIGHT.ToString()) return (forward + right).normalized;
            if (direction == SceneTokenDirection.RIGHT.ToString()) return right;
            if (direction == SceneTokenDirection.BACK_RIGHT.ToString()) return (-forward + right).normalized;
            if (direction == SceneTokenDirection.BACK.ToString()) return -forward;
            if (direction == SceneTokenDirection.BACK_LEFT.ToString()) return (-forward - right).normalized;
            if (direction == SceneTokenDirection.LEFT.ToString()) return -right;
            if (direction == SceneTokenDirection.FRONT_LEFT.ToString()) return (forward - right).normalized;
            return forward;
        }

        private float DecodeRadius(string distance)
        {
            if (renderCondition == SceneTokenRenderCondition.DIRECTION_ONLY)
            {
                return midRadius;
            }

            if (distance == SceneTokenDistance.NEAR.ToString()) return nearRadius;
            if (distance == SceneTokenDistance.FAR.ToString()) return farRadius;
            return midRadius;
        }

        private float DecodeVolume(SceneToken token)
        {
            if (renderCondition >= SceneTokenRenderCondition.DIRECTION_DISTANCE_SPEAKING &&
                token.speakingState != SceneSpeakingState.SPEAKING.ToString())
            {
                return 0f;
            }

            var baseVolume = 0.7f;

            if (renderCondition >= SceneTokenRenderCondition.DIRECTION_DISTANCE)
            {
                if (token.distance == SceneTokenDistance.NEAR.ToString()) baseVolume = 1f;
                if (token.distance == SceneTokenDistance.FAR.ToString()) baseVolume = 0.45f;
            }

            if (renderCondition >= SceneTokenRenderCondition.FULL_SCENE_TOKEN)
            {
                if (token.turnState == SceneTurnState.TURN_HOLDER.ToString()) baseVolume *= 1.2f;
                if (token.turnState == SceneTurnState.OVERLAPPER.ToString()) baseVolume *= 0.8f;
                if (token.semanticToken == SceneSemanticToken.WARNING.ToString()) baseVolume *= 1.25f;
                if (token.semanticToken == SceneSemanticToken.INSTRUCTION.ToString()) baseVolume *= 1.1f;
            }

            return Mathf.Clamp01(baseVolume);
        }

        private float DecodePitch(SceneToken token)
        {
            if (renderCondition < SceneTokenRenderCondition.FULL_SCENE_TOKEN)
            {
                return 1f;
            }

            if (token.semanticToken == SceneSemanticToken.QUESTION.ToString()) return 1.05f;
            if (token.semanticToken == SceneSemanticToken.WARNING.ToString()) return 1.1f;
            if (token.semanticToken == SceneSemanticToken.DISAGREEMENT.ToString()) return 0.95f;
            return 1f;
        }
    }
}
