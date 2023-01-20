using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Root of the response from GET request to Fitbit's "Heart Rate Time Series by Date".
/// </summary>
[System.Serializable]
public class FitbitHeartRateData
{
    [JsonProperty("activities-heart")]
    public List<ActivitiesHeart> ActivitiesHearts;

    [JsonProperty("activities-heart-intraday")]
    public ActivitiesHeartIntraday ActivitiesHeartIntradays;

    public FitbitHeartRateData(List<ActivitiesHeart> activitiesHearts, ActivitiesHeartIntraday activitiesHeartIntradays)
    {
        ActivitiesHearts = activitiesHearts;
        ActivitiesHeartIntradays = activitiesHeartIntradays;
    }

    public override string ToString()
    {
        string str = $"size: {ActivitiesHearts.Count}\n";
        foreach (ActivitiesHeart activity in ActivitiesHearts)
            str += activity.ToString();
        return str;
    }
}

#region ActivitiesHeart
[System.Serializable]
public class ActivitiesHeart
{
    public string dateTime;
    public List<HeartRateZone> customHeartRateZones;
    public List<HeartRateZone> heartRateZones;
    public string value;

    public ActivitiesHeart(string dateTime, List<HeartRateZone> customHeartRateZones, List<HeartRateZone> heartRateZones, string value)
    {
        this.dateTime = dateTime;
        this.customHeartRateZones = customHeartRateZones;
        this.heartRateZones = heartRateZones;
        this.value = value;
    }

    public override string ToString()
    {
        string str = $"dateTime: {dateTime}\nvalue: {value}\n";

        str += "custom heart rate zones:\n";
        foreach (HeartRateZone zone in customHeartRateZones)
            str += zone;

        str += "heart rate zones:\n";
        foreach (HeartRateZone zone in heartRateZones)
            str += zone;

        return str;
    }
}

[System.Serializable]
public class HeartRateZone
{
    public double caloriesOut;
    public int max;
    public int min;
    public int minutes;
    public string name;

    public HeartRateZone(double caloriesOut, int max, int min, int minutes, string name)
    {
        this.caloriesOut = caloriesOut;
        this.max = max;
        this.min = min;
        this.minutes = minutes;
        this.name = name;
    }

    public override string ToString()
    {
        string str = "";
        str += "\t{\n";
        str += $"\t\tcalories out: {caloriesOut}\n";
        str += $"\t\tmax: {max}, min: {min}\n";
        str += $"\t\tminutes: {minutes}\n";
        str += $"\t\tname: {name}\n";
        str += "\t},\n";
        return str;
    }
}
#endregion

#region ActivitiesHeartIntraday
/// <summary>
/// The heart rate intraday time series data on a specific date range for a 24 hour period.
/// </summary>
[System.Serializable]
public class ActivitiesHeartIntraday
{
    public List<IntradayDataset> dataset;   // List of intraday data recorded in the given interval
    public int datasetInterval;             // The requested detail-level numerical interval (1sec, 1min, 5min, 15min)
    public string datasetType;              // The requested detail-level unit of measure (second or minute)

    public ActivitiesHeartIntraday(List<IntradayDataset> dataset, int datasetInterval, string datasetType)
    {
        this.dataset = dataset;
        this.datasetInterval = datasetInterval;
        this.datasetType = datasetType;
    }

    public override string ToString()
    {
        string str = $"dataset interval: {datasetInterval}\n";
        str += $"datasetType: {datasetType}\n";
        str += $"dataset:\n";
        foreach (IntradayDataset ds in dataset)
            str += ds;
        return str;
    }
}

[System.Serializable]
public class IntradayDataset
{
    public string time; // The time the intraday heart rate value was recorded
    public int value;   // The intraday heart rate value

    public IntradayDataset(string time, int value)
    {
        this.time = time;
        this.value = value;
    }

    public override string ToString()
    {
        return $"\ttime: {time}, value: {value}\n";
    }
}
#endregion
