using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupWorkPresenceAudio : MonoBehaviour
{
    public Transform player;
    public AudioSource audioSource;

    public AudioClip speakingClip;

    public bool isSpeaking = false;

    public float maxDistance = 10f;

    public float Distance { get; private set; }
    public float VolumeByDistance { get; private set; }
    public string CurrentState { get; private set; }

    private bool hasPlayedSpeaking = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            // 3D空間音響用
            audioSource.spatialBlend = 1.0f;
            audioSource.spatialize = true;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 20f;
            audioSource.dopplerLevel = 0f;
        }
    }

    void Update()
    {
        if (player == null || audioSource == null)
        {
            return;
        }

        Distance = Vector3.Distance(transform.position, player.position);

        VolumeByDistance = Mathf.Clamp01(1f - Distance / maxDistance);

        // 小さすぎると確認しづらいので最低音量を少し残す
        audioSource.volume = Mathf.Clamp(VolumeByDistance, 0.3f, 1f);

        if (isSpeaking)
        {
            CurrentState = "Speaking";

            if (!hasPlayedSpeaking)
            {
                PlaySpeakingOnce();
                hasPlayedSpeaking = true;
            }
        }
        else
        {
            CurrentState = "Idle";
            hasPlayedSpeaking = false;
        }
    }

    void PlaySpeakingOnce()
    {
        if (speakingClip == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = speakingClip;
        audioSource.loop = false;
        audioSource.pitch = 1.0f;
        audioSource.Play();
    }

    public void SetSpeaking(bool value)
    {
        isSpeaking = value;

        if (!value && audioSource != null)
        {
            audioSource.Stop();
            hasPlayedSpeaking = false;
        }
    }
}