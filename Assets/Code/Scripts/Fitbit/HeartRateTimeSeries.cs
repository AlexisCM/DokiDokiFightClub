using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Root of the response from GET request to Fitbit's "Heart Rate Time Series by Date".
/// </summary>
[System.Serializable]
public class HeartRateTimeSeries
{
    [JsonProperty("activities-heart")]
    public List<ActivitiesHeart> activitiesHeart;

    public HeartRateTimeSeries(List<ActivitiesHeart> activitiesHeart)
    {
        this.activitiesHeart = activitiesHeart;
    }

    public override string ToString()
    {
        string str = $"size: {activitiesHeart.Count}\n";
        foreach (ActivitiesHeart activity in activitiesHeart)
            str += activity.ToString();
        return str;
    }
}

[System.Serializable]
public class ActivitiesHeart
{
    public string dateTime;
    public TimeSeriesValue value;

    public ActivitiesHeart(string dateTime, TimeSeriesValue value)
    {
        this.dateTime = dateTime;
        this.value = value;
    }

    public override string ToString()
    {
        return $"dateTime: {dateTime}\nvalue: {value}";
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

[System.Serializable]
public class TimeSeriesValue
{
    public List<HeartRateZone> customHeartRateZones;
    public List<HeartRateZone> heartRateZones;
    public int restingHeartRate;

    public TimeSeriesValue(List<HeartRateZone> customHeartRateZones, List<HeartRateZone> heartRateZones, int restingHeartRate)
    {
        this.customHeartRateZones = customHeartRateZones;
        this.heartRateZones = heartRateZones;
        this.restingHeartRate = restingHeartRate;
    }

    public override string ToString()
    {
        string str = $"resting heart rate: {restingHeartRate}\n";

        str += "custom heart rate zones:\n";
        foreach (HeartRateZone zone in customHeartRateZones)
            str +=  zone;

        str += "heart rate zones:\n";
        foreach (HeartRateZone zone in heartRateZones)
            str += zone;

        return str;
    }
}
