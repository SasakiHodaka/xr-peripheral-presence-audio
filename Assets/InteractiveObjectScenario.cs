using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

public sealed class InteractiveObjectScenario : MonoBehaviour
{
    private const string ScenarioId = "S4_AUTOMATED_TRANSFER";
    private ScenarioPlayer player;
    private Camera sceneCamera;
    private UserAdaptationController adaptation;
    private ScenarioComparisonDemo comparisonDemo;
    private GameObject workpiece;
    private GameObject targetZone;
    private GameObject wrongZone;
    private Coroutine scenarioRoutine;
    private int sequence;
    private float taskStartTime;
    private string interactionLogPath;
    private bool completed;
    public bool CanInteract => !completed;
    private string status = "Preparing automated object-transfer scenario...";

    private IEnumerator Start()
    {
        player = GetComponent<ScenarioPlayer>();
        sceneCamera = Camera.main;
        adaptation = GetComponent<UserAdaptationController>();
        comparisonDemo = GetComponent<ScenarioComparisonDemo>();
        yield return null;
        if (player != null) player.StopGroundTruthPlayback();
        RestartAndPlay();
    }

    public void RestartAndPlay()
    {
        if (scenarioRoutine != null) StopCoroutine(scenarioRoutine);
        DestroyScenarioObjects();
        sequence = 0;
        completed = false;
        CreateScenarioObjects();
        taskStartTime = Time.time;
        CreateInteractionLog();
        scenarioRoutine = StartCoroutine(PlayScenario());
    }

    private IEnumerator PlayScenario()
    {
        status = "Drag the ORANGE cube to RED once, then to GREEN";
        yield return new WaitForSeconds(0.6f);

        status = "STEP: drag Workpiece01 to the green target";
        Emit("MOVE_TO_TARGET", 1);
        yield return new WaitForSeconds(0.8f);

        status = "REPEAT: drag the orange cube with the left mouse button";
        Emit("MOVE_TO_TARGET_REPEAT", 1);
        yield return new WaitForSeconds(0.8f);

        status = "TASK ACTIVE: try RED, then correct it by placing on GREEN";
        Emit("BACKGROUND_UPDATE", 0, "UnrelatedMachine", "OtherTask");
        scenarioRoutine = null;
    }

    public void NotifyGrab(Vector3 position)
    {
        if (completed) return;
        status = "DRAGGING: release over RED or GREEN";
        LogInteraction("grab", "None", position);
    }

    public void NotifyRelease(Vector3 position)
    {
        if (completed) return;
        LogInteraction("release", "None", position);

        if (IsInsideZone(position, wrongZone))
        {
            Vector3 snap = wrongZone.transform.position + Vector3.up * 0.57f;
            workpiece.transform.position = snap;
            workpiece.GetComponent<Renderer>().material.color = new Color(1f, 0.15f, 0.1f);
            status = "WRONG: guidance need increased. Now drag the cube to GREEN";
            LogInteraction("wrong_placement", "WrongZone", snap);
            if (adaptation != null) adaptation.RecordError("user_wrong_placement");
            Emit("WRONG_PLACEMENT", 2);
            return;
        }

        if (IsInsideZone(position, targetZone))
        {
            Vector3 snap = targetZone.transform.position + Vector3.up * 0.57f;
            workpiece.transform.position = snap;
            workpiece.GetComponent<Renderer>().material.color = new Color(0.15f, 0.95f, 0.25f);
            status = "SUCCESS: task completed | F9 replay";
            LogInteraction("correct_placement", "CorrectZone", snap);
            if (adaptation != null) adaptation.RecordSuccess("user_correct_placement");
            Emit("CORRECT_PLACEMENT", 2);
            completed = true;
            if (comparisonDemo != null) comparisonDemo.CompleteCurrentRun();
            return;
        }

        status = "NOT IN A ZONE: drag the cube onto RED or GREEN";
        LogInteraction("outside_zone", "None", position);
    }

    private void CreateScenarioObjects()
    {
        Vector3 origin = sceneCamera != null ? sceneCamera.transform.position : Vector3.zero;
        Vector3 forward = sceneCamera != null ? sceneCamera.transform.forward : Vector3.forward;
        Vector3 right = sceneCamera != null ? sceneCamera.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.1f) forward = Vector3.forward;
        forward.Normalize();
        right.Normalize();
        Vector3 floorOrigin = new Vector3(origin.x, 0f, origin.z);

        workpiece = CreatePrimitive("AutomatedWorkpiece01", PrimitiveType.Cube,
            floorOrigin + forward * 2.4f + Vector3.up * 0.65f,
            new Vector3(0.7f, 0.7f, 0.7f), new Color(1f, 0.45f, 0.05f));
        MouseDragTaskObject drag = workpiece.AddComponent<MouseDragTaskObject>();
        drag.Configure(this, sceneCamera);
        Rigidbody body = workpiece.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = false;
        QuestGrabTaskObject questGrab = workpiece.AddComponent<QuestGrabTaskObject>();
        questGrab.Configure(this);
        targetZone = CreatePrimitive("CorrectTargetZone", PrimitiveType.Cylinder,
            floorOrigin + forward * 4.0f + right * 1.5f + Vector3.up * 0.08f,
            new Vector3(0.9f, 0.08f, 0.9f), new Color(0.1f, 0.85f, 0.2f));
        wrongZone = CreatePrimitive("WrongTargetZone", PrimitiveType.Cylinder,
            floorOrigin + forward * 3.5f - right * 1.4f + Vector3.up * 0.08f,
            new Vector3(0.9f, 0.08f, 0.9f), new Color(0.9f, 0.15f, 0.1f));
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type,
        Vector3 position, Vector3 scale, Color color)
    {
        GameObject value = GameObject.CreatePrimitive(type);
        value.name = name;
        value.transform.position = position;
        value.transform.localScale = scale;
        value.GetComponent<Renderer>().material.color = color;
        Collider collider = value.GetComponent<Collider>();
        if (type == PrimitiveType.Cylinder && collider != null) collider.enabled = false;
        return value;
    }

    private void DestroyScenarioObjects()
    {
        if (workpiece != null) Destroy(workpiece);
        if (targetZone != null) Destroy(targetZone);
        if (wrongZone != null) Destroy(wrongZone);
    }

    private static bool IsInsideZone(Vector3 position, GameObject zone)
    {
        if (zone == null) return false;
        Vector2 point = new Vector2(position.x, position.z);
        Vector2 center = new Vector2(zone.transform.position.x, zone.transform.position.z);
        float radius = Mathf.Max(zone.transform.localScale.x, zone.transform.localScale.z) * 0.75f;
        return Vector2.Distance(point, center) <= radius;
    }

    private void CreateInteractionLog()
    {
        string directory = Path.Combine(Application.persistentDataPath, "scene_token_ground_truth");
        Directory.CreateDirectory(directory);
        interactionLogPath = Path.Combine(directory,
            "interaction_events_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".csv");
        File.WriteAllText(interactionLogPath,
            "timestamp,taskElapsed,eventType,zone,x,y,z\n");
    }

    private void LogInteraction(string eventType, string zone, Vector3 position)
    {
        if (string.IsNullOrEmpty(interactionLogPath)) return;
        File.AppendAllText(interactionLogPath, string.Format(CultureInfo.InvariantCulture,
            "{0:F3},{1:F3},{2},{3},{4:F3},{5:F3},{6:F3}\n",
            Time.time, Time.time - taskStartTime, eventType, zone,
            position.x, position.y, position.z));
    }

    private void Emit(string action, int priority,
        string objectId = "Workpiece01", string taskState = "WorkpieceWork")
    {
        if (player == null || workpiece == null) return;
        Vector3 source = workpiece.transform.position;
        Vector3 listener = sceneCamera != null ? sceneCamera.transform.position : Vector3.zero;
        GroundTruthEvent scenarioEvent = new GroundTruthEvent
        {
            eventId = string.Format("I{0:000}_{1}", ++sequence, action),
            sequence = sequence,
            expectedTime = Time.time,
            speakerId = "AutomationController",
            speakerPosition = ToSerializable(source),
            listenerPosition = ToSerializable(listener),
            listenerRotationY = sceneCamera != null ? sceneCamera.transform.eulerAngles.y : 0f,
            utteranceId = action,
            objectId = objectId,
            objectPosition = ToSerializable(source),
            taskState = taskState,
            priority = priority
        };
        player.ProcessInteractiveEvent(ScenarioId, scenarioEvent);
    }

    private void OnGUI()
    {
        float width = Mathf.Min(700f, Screen.width - 40f);
        Rect area = new Rect((Screen.width - width) * 0.5f, Screen.height - 92f, width, 72f);
        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(area.x + 12f, area.y + 8f, area.width - 24f, area.height - 16f));
        GUILayout.Label("MOUSE TASK: left-drag ORANGE cube | RED = error | GREEN = success | F9 Replay");
        GUILayout.Label(status);
        GUILayout.EndArea();
    }

    private static SerializableVector3 ToSerializable(Vector3 value)
    {
        return new SerializableVector3 { x = value.x, y = value.y, z = value.z };
    }
}
