using UnityEngine;

public static class ScenarioBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateScenarioManagerIfMissing()
    {
        GroundTruthLoader existingLoader = Object.FindObjectOfType<GroundTruthLoader>();

        if (existingLoader != null)
        {
            EnsureScenarioComponents(existingLoader.gameObject);
            return;
        }

        GameObject scenarioManager = new GameObject("ScenarioManager");
        scenarioManager.AddComponent<GroundTruthLoader>();
        EnsureScenarioComponents(scenarioManager);

        Debug.Log("ScenarioManager was created automatically for Ground Truth playback.");
    }

    private static void EnsureScenarioComponents(GameObject target)
    {
        if (target.GetComponent<GroundTruthSceneTokenGenerator>() == null)
        {
            target.AddComponent<GroundTruthSceneTokenGenerator>();
        }

        if (target.GetComponent<ExperimentLogger>() == null)
        {
            target.AddComponent<ExperimentLogger>();
        }

        if (target.GetComponent<PrioritySelectionPolicy>() == null)
        {
            target.AddComponent<PrioritySelectionPolicy>();
        }

        if (target.GetComponent<SemanticPacketBuilder>() == null)
        {
            target.AddComponent<SemanticPacketBuilder>();
        }

        if (target.GetComponent<PresentationPolicy>() == null)
        {
            target.AddComponent<PresentationPolicy>();
        }

        if (target.GetComponent<ScenarioPlayer>() == null)
        {
            target.AddComponent<ScenarioPlayer>();
        }

        if (target.GetComponent<ScenarioComparisonDemo>() == null)
        {
            target.AddComponent<ScenarioComparisonDemo>();
        }
        if (target.GetComponent<InteractiveObjectScenario>() == null)
        {
            target.AddComponent<InteractiveObjectScenario>();
        }
        if (target.GetComponent<UserAdaptationController>() == null)
        {
            target.AddComponent<UserAdaptationController>();
        }
    }
}
