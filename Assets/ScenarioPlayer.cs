using System.Collections;
using UnityEngine;

public sealed class ScenarioPlayer : MonoBehaviour
{
    [SerializeField]
    private GroundTruthLoader loader;

    [SerializeField]
    private GroundTruthSceneTokenGenerator tokenGenerator;

    [SerializeField]
    private ExperimentLogger experimentLogger;

    [SerializeField]
    private PrioritySelectionPolicy selectionPolicy;

    [SerializeField]
    private SemanticPacketBuilder packetBuilder;

    [SerializeField]
    private PresentationPolicy presentationPolicy;

    private Coroutine playRoutine;

    private void Reset()
    {
        loader = GetComponent<GroundTruthLoader>();
        tokenGenerator = GetComponent<GroundTruthSceneTokenGenerator>();
        experimentLogger = GetComponent<ExperimentLogger>();
        selectionPolicy = GetComponent<PrioritySelectionPolicy>();
        packetBuilder = GetComponent<SemanticPacketBuilder>();
        presentationPolicy = GetComponent<PresentationPolicy>();
    }

    private void Start()
    {
        if (loader == null)
        {
            loader = GetComponent<GroundTruthLoader>();
        }

        if (tokenGenerator == null)
        {
            tokenGenerator = GetComponent<GroundTruthSceneTokenGenerator>();
        }

        if (experimentLogger == null)
        {
            experimentLogger = GetComponent<ExperimentLogger>();
        }

        if (selectionPolicy == null)
        {
            selectionPolicy = GetComponent<PrioritySelectionPolicy>();
        }

        if (packetBuilder == null)
        {
            packetBuilder = GetComponent<SemanticPacketBuilder>();
        }

        if (presentationPolicy == null)
        {
            presentationPolicy = GetComponent<PresentationPolicy>();
        }

        if (loader == null || loader.CurrentScenario == null)
        {
            return;
        }

        playRoutine = StartCoroutine(PlayEvents(loader.CurrentScenario));
    }

    private IEnumerator PlayEvents(GroundTruthScenario scenario)
    {
        float startTime = Time.time;

        foreach (GroundTruthEvent scenarioEvent in scenario.events)
        {
            if (scenarioEvent == null)
            {
                continue;
            }

            float elapsed = Time.time - startTime;
            float waitTime = scenarioEvent.expectedTime - elapsed;

            if (waitTime > 0.0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            Debug.Log(
                $"Scenario Event Fired," +
                $"{scenario.scenarioId}," +
                $"{scenarioEvent.eventId}," +
                $"{scenarioEvent.expectedTime:F3}," +
                $"{scenarioEvent.speakerId}," +
                $"{scenarioEvent.objectId}"
            );

            if (tokenGenerator == null)
            {
                continue;
            }

            GeneratedSceneToken token = tokenGenerator.Generate(
                scenario.scenarioId,
                scenarioEvent
            );

            SelectionResult selection = null;
            if (selectionPolicy != null)
            {
                selection = selectionPolicy.Select(token);
            }

            SemanticPacket packet = null;
            if (packetBuilder != null)
            {
                packet = packetBuilder.Build(token, selection);
            }

            PresentationResult presentation = null;
            if (presentationPolicy != null)
            {
                presentation = presentationPolicy.Present(token, selection, packet);
            }

            if (experimentLogger != null)
            {
                experimentLogger.LogToken(token, selection, packet, presentation);
            }
        }

        playRoutine = null;
    }
}
