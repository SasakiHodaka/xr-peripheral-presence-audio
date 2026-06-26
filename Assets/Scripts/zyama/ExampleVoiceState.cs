using UnityEngine;

public class ExampleVoiceState : MonoBehaviour
{
    public PresenceTarget target;

    void Update()
    {
        // 仮: キー入力で発話状態を切替
        if (Input.GetKeyDown(KeyCode.V) && target != null)
        {
            target.isSpeaking = !target.isSpeaking;
        }
    }
}
