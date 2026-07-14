using System;
using System.IO;
using UnityEngine;

public sealed class GroundTruthLoader : MonoBehaviour
{
    private const string PhaseDefaultScenarioFileName = "scenario_03.json";

    [SerializeField]
    private string scenarioFileName = PhaseDefaultScenarioFileName;

    [SerializeField]
    private bool usePhaseDefaultScenario = true;

    public GroundTruthScenario CurrentScenario { get; private set; }

    private void Awake()
    {
        if (usePhaseDefaultScenario)
        {
            scenarioFileName = PhaseDefaultScenarioFileName;
        }

        LoadScenario();
    }

    public void LoadScenario()
    {
        string filePath = Path.Combine(
            Application.streamingAssetsPath,
            "Scenarios",
            scenarioFileName
        );

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Scenario file not found: {filePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError($"Scenario file is empty: {filePath}");
                return;
            }

            CurrentScenario = JsonUtility.FromJson<GroundTruthScenario>(json);

            if (CurrentScenario == null)
            {
                Debug.LogError("Failed to deserialize scenario JSON.");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentScenario.scenarioId))
            {
                Debug.LogError("Scenario ID is missing.");
                return;
            }

            if (CurrentScenario.events == null)
            {
                Debug.LogError("Scenario events are missing.");
                return;
            }

            if (CurrentScenario.events.Length == 0)
            {
                Debug.LogWarning("Scenario contains no events.");
            }

            Array.Sort(
                CurrentScenario.events,
                (left, right) => left.sequence.CompareTo(right.sequence)
            );

            PrintScenario(CurrentScenario, filePath);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to load scenario.\n" +
                $"Path: {filePath}\n" +
                $"Error: {exception}"
            );
        }
    }

    private static void PrintScenario(
        GroundTruthScenario scenario,
        string filePath
    )
    {
        Debug.Log(
            "Scenario Loaded\n" +
            $"Loaded From : {filePath}\n" +
            $"Scenario ID : {scenario.scenarioId}\n" +
            $"Event Count : {scenario.events.Length}"
        );

        for (int index = 0; index < scenario.events.Length; index++)
        {
            GroundTruthEvent scenarioEvent = scenario.events[index];

            if (scenarioEvent == null)
            {
                Debug.LogWarning($"Event at index {index} is null.");
                continue;
            }

            Debug.Log(
                $"Event {index + 1}\n" +
                $"Event ID : {scenarioEvent.eventId}\n" +
                $"Sequence : {scenarioEvent.sequence}\n" +
                $"Time : {scenarioEvent.expectedTime:F3}\n" +
                $"Speaker : {scenarioEvent.speakerId}\n" +
                $"Object : {scenarioEvent.objectId}"
            );
        }
    }
}
