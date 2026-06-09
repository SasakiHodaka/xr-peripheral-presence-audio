using System;
using UnityEngine;

[Serializable]
public class PeripheralCueLearnedModelData
{
    public string modelType;
    public PeripheralCueFeatureEncoderData featureEncoder;
    public PeripheralCueClassCentroid[] classifier;
    public PeripheralCueLinearClass[] linearClassifier;
    public PeripheralCueRegressor[] regressors;
}

[Serializable]
public class PeripheralCueFeatureEncoderData
{
    public string[] categoricalColumns;
    public string[] booleanColumns;
    public string[] numericColumns;
    public PeripheralCueCategorySet[] categorySets;
    public PeripheralCueNumericStats[] numericStats;
}

[Serializable]
public class PeripheralCueCategorySet
{
    public string column;
    public string[] categories;
}

[Serializable]
public class PeripheralCueNumericStats
{
    public string column;
    public float mean;
    public float std;
}

[Serializable]
public class PeripheralCueClassCentroid
{
    public string label;
    public float[] centroid;
}

[Serializable]
public class PeripheralCueLinearClass
{
    public string label;
    public float[] weights;
    public float bias;
}

[Serializable]
public class PeripheralCueRegressor
{
    public string target;
    public float[] weights;
    public float bias;
}

public class PeripheralCueFeatureContext
{
    public string conditionLabel;
    public string cueCondition;
    public string materialClass;
    public string targetId;
    public string directionLabel;
    public float roomScale = 1f;
    public float environmentReverbAmount;
    public float environmentOcclusionStrength;
    public float environmentDistanceAttenuation;
    public float environmentRt60;
    public float environmentDrr;
    public bool outOfView;
    public bool approaching;
    public bool speaking;
    public bool gazing;
    public bool near;
    public bool crossing;
    public float distance;
    public float viewAngle;
    public float radialSpeed;
    public float lateralSpeed;
    public float localX;
    public float localY;
    public float localZ;
}

public class PeripheralCueLearnedModel
{
    private PeripheralCueLearnedModelData data;

    public bool IsLoaded
    {
        get { return data != null && data.featureEncoder != null && data.classifier != null; }
    }

    public static PeripheralCueLearnedModel FromJson(string json)
    {
        PeripheralCueLearnedModel model = new PeripheralCueLearnedModel();
        model.data = JsonUtility.FromJson<PeripheralCueLearnedModelData>(json);
        return model;
    }

    public PeripheralCuePrediction Predict(PeripheralCueFeatureContext context)
    {
        if (!IsLoaded)
            return EmptyPrediction();

        float[] features = Encode(context);
        string cueType = PredictClass(features);

        PeripheralCuePrediction prediction = EmptyPrediction();
        prediction.cueType = ParseCueType(cueType);
        prediction.presenceScore = prediction.cueType == PeripheralCueType.None ? 0f : Mathf.Clamp01(PredictRegression("presenceScore", features));
        prediction.volumeGain = prediction.cueType == PeripheralCueType.None ? 0f : Mathf.Clamp01(PredictRegression("volumeGain", features));
        prediction.lowPassHz = Mathf.Clamp(PredictRegression("cueLowPassHz", features), 250f, 22000f);
        prediction.reverbAmount = prediction.cueType == PeripheralCueType.None ? 0f : Mathf.Clamp01(PredictRegression("cueReverbAmount", features));
        prediction.occlusionGain = Mathf.Clamp01(PredictRegression("cueOcclusionGain", features));
        return prediction;
    }

    private float[] Encode(PeripheralCueFeatureContext context)
    {
        int width = GetFeatureWidth();
        float[] features = new float[width];
        int index = 0;

        string[] categoricalColumns = data.featureEncoder.categoricalColumns ?? Array.Empty<string>();
        for (int i = 0; i < categoricalColumns.Length; i++)
        {
            string column = categoricalColumns[i];
            string value = GetCategoricalValue(context, column);
            string[] categories = GetCategories(column);
            for (int j = 0; j < categories.Length; j++)
                features[index++] = value == categories[j] ? 1f : 0f;
        }

        string[] booleanColumns = data.featureEncoder.booleanColumns ?? Array.Empty<string>();
        for (int i = 0; i < booleanColumns.Length; i++)
            features[index++] = GetBooleanValue(context, booleanColumns[i]) ? 1f : 0f;

        string[] numericColumns = data.featureEncoder.numericColumns ?? Array.Empty<string>();
        for (int i = 0; i < numericColumns.Length; i++)
        {
            string column = numericColumns[i];
            PeripheralCueNumericStats stats = GetNumericStats(column);
            float std = stats != null && Mathf.Abs(stats.std) > 0.000001f ? stats.std : 1f;
            float mean = stats != null ? stats.mean : 0f;
            features[index++] = (GetNumericValue(context, column) - mean) / std;
        }

        return features;
    }

    private int GetFeatureWidth()
    {
        int width = 0;
        string[] categoricalColumns = data.featureEncoder.categoricalColumns ?? Array.Empty<string>();
        for (int i = 0; i < categoricalColumns.Length; i++)
            width += GetCategories(categoricalColumns[i]).Length;

        width += (data.featureEncoder.booleanColumns ?? Array.Empty<string>()).Length;
        width += (data.featureEncoder.numericColumns ?? Array.Empty<string>()).Length;
        return width;
    }

    private string[] GetCategories(string column)
    {
        PeripheralCueCategorySet[] sets = data.featureEncoder.categorySets ?? Array.Empty<PeripheralCueCategorySet>();
        for (int i = 0; i < sets.Length; i++)
        {
            if (sets[i].column == column)
                return sets[i].categories ?? Array.Empty<string>();
        }

        return Array.Empty<string>();
    }

    private PeripheralCueNumericStats GetNumericStats(string column)
    {
        PeripheralCueNumericStats[] stats = data.featureEncoder.numericStats ?? Array.Empty<PeripheralCueNumericStats>();
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].column == column)
                return stats[i];
        }

        return null;
    }

    private string PredictClass(float[] features)
    {
        if (data.linearClassifier != null && data.linearClassifier.Length > 0)
            return PredictLinearClass(features);

        PeripheralCueClassCentroid best = null;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < data.classifier.Length; i++)
        {
            PeripheralCueClassCentroid candidate = data.classifier[i];
            float distance = SquaredDistance(features, candidate.centroid);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best != null ? best.label : "None";
    }

    private string PredictLinearClass(float[] features)
    {
        PeripheralCueLinearClass best = null;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < data.linearClassifier.Length; i++)
        {
            PeripheralCueLinearClass candidate = data.linearClassifier[i];
            float score = Dot(candidate.weights, features) + candidate.bias;
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best != null ? best.label : "None";
    }

    private float PredictRegression(string target, float[] features)
    {
        PeripheralCueRegressor[] regressors = data.regressors ?? Array.Empty<PeripheralCueRegressor>();
        for (int i = 0; i < regressors.Length; i++)
        {
            PeripheralCueRegressor regressor = regressors[i];
            if (regressor.target == target)
                return Dot(regressor.weights, features) + regressor.bias;
        }

        if (target == "cueLowPassHz")
            return 22000f;
        if (target == "cueOcclusionGain")
            return 1f;
        return 0f;
    }

    private static float SquaredDistance(float[] left, float[] right)
    {
        if (left == null || right == null)
            return float.PositiveInfinity;

        int count = Mathf.Min(left.Length, right.Length);
        float sum = 0f;
        for (int i = 0; i < count; i++)
        {
            float diff = left[i] - right[i];
            sum += diff * diff;
        }

        return sum;
    }

    private static float Dot(float[] weights, float[] features)
    {
        if (weights == null || features == null)
            return 0f;

        int count = Mathf.Min(weights.Length, features.Length);
        float sum = 0f;
        for (int i = 0; i < count; i++)
            sum += weights[i] * features[i];

        return sum;
    }

    private static string GetCategoricalValue(PeripheralCueFeatureContext context, string column)
    {
        switch (column)
        {
            case "conditionLabel": return context.conditionLabel ?? string.Empty;
            case "cueCondition": return context.cueCondition ?? string.Empty;
            case "materialClass": return context.materialClass ?? string.Empty;
            case "targetId": return context.targetId ?? string.Empty;
            case "directionLabel": return context.directionLabel ?? string.Empty;
            default: return string.Empty;
        }
    }

    private static bool GetBooleanValue(PeripheralCueFeatureContext context, string column)
    {
        switch (column)
        {
            case "outOfView": return context.outOfView;
            case "approaching": return context.approaching;
            case "speaking": return context.speaking;
            case "gazing": return context.gazing;
            case "near": return context.near;
            case "crossing": return context.crossing;
            default: return false;
        }
    }

    private static float GetNumericValue(PeripheralCueFeatureContext context, string column)
    {
        switch (column)
        {
            case "roomScale": return context.roomScale;
            case "environmentReverbAmount": return context.environmentReverbAmount;
            case "environmentOcclusionStrength": return context.environmentOcclusionStrength;
            case "environmentDistanceAttenuation": return context.environmentDistanceAttenuation;
            case "environmentRt60": return context.environmentRt60;
            case "environmentDrr": return context.environmentDrr;
            case "distance": return context.distance;
            case "viewAngle": return context.viewAngle;
            case "radialSpeed": return context.radialSpeed;
            case "lateralSpeed": return context.lateralSpeed;
            case "localX": return context.localX;
            case "localY": return context.localY;
            case "localZ": return context.localZ;
            default: return 0f;
        }
    }

    private static PeripheralCueType ParseCueType(string value)
    {
        PeripheralCueType parsed;
        return Enum.TryParse(value, out parsed) ? parsed : PeripheralCueType.None;
    }

    private static PeripheralCuePrediction EmptyPrediction()
    {
        PeripheralCuePrediction prediction = new PeripheralCuePrediction();
        prediction.cueType = PeripheralCueType.None;
        prediction.directionLabel = PeripheralDirectionLabel.Front;
        prediction.lowPassHz = 22000f;
        prediction.occlusionGain = 1f;
        prediction.reason = "LearnedNoCue";
        return prediction;
    }
}
