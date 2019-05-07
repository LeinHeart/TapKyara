using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Xml;
using System.Text;

public class GameUtility {
    public const float ResolutionDelayTime = 1;
    
    //Save HighScore
    public const string SavePrefKey = "Game_Highscore_Value"; //HighScore per Series
    public const string LevelUnlock = "Level"; //Check if the Series is unlock or not 
    public const string ProgessTotalScore = "Progress_Total_Score"; //Total Score from all Series
    public const string LastStageLoad = "Last_Stage_Load"; //Last Stage Load to check What Series is currently user see
    public const string CurrentMusicPlaying = "Current_Music_Playing"; //Save Current Music

    public const string FileName = "Q";
    public static string fileDir
    {
        get
        {
            return Application.dataPath + "/";
        }
    }
}

[System.Serializable()]
public class Data
{
    public Question[] Questions = new Question[0];

    public Data() { }

    public static void Write(Data data, string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Data));

        using (Stream stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, data);
        }
    }

    //public static Data Fetch(string filePath)
    //{
    //    return Fetch(out bool result,filePath);
    //}

    //public static Data Fetch(out bool result, string filePath)
    //{
    //    if (!File.Exists(filePath))
    //    {
    //        result = false;  return new Data();
    //    }
    //    XmlSerializer deserializer = new XmlSerializer(typeof(Data));
    //    var encoding = Encoding.GetEncoding("UTF-8");
    //    using (Stream stream = new FileStream(filePath, FileMode.Open))
    //    {
    //        var data = (Data)deserializer.Deserialize(stream);

    //        result = true;

    //        return data;
    //    }
    //    //StreamWriter stream = new StreamWriter(filePath, false, encoding);
    //    //deserializer.Serialize(stream, quest);
    //}

    public static Data ReadFromXml(string path)
    {
        if (!BetterStreamingAssets.FileExists(path))
        {
            Debug.LogErrorFormat("Streaming asset not found: {0}", path);
            return null;
        }

        using (var stream = BetterStreamingAssets.OpenRead(path))
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Data));
            return (Data)serializer.Deserialize(stream);
        }
    }
}