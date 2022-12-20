using System;
using System.Collections.Generic;

public class FitbitData
{
    public Dictionary<string, string> PlayerFitbitData;

    public float HeartRate;
    public float RestingHeartRate;

    public DateTime LastSyncTime;

    public FitbitData()
    {
        PlayerFitbitData = new Dictionary<string, string>();

        PlayerFitbitData.Add("heartRate", "");
        PlayerFitbitData.Add("restingHeartRate", "");
    }
}
