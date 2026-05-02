using UnityEngine;
using UnityEditor;

public class TakeScreenshot
{
    public static string Execute()
    {
        string path = Application.dataPath + "/ss.png";
        ScreenCapture.CaptureScreenshot(path, 1);
        return path;
    }
}
