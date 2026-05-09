using UnityEngine;
using UnityEditor;

public class ResetSceneView
{
    public static string Execute()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return "No active scene view";

        sceneView.cameraMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.Textured);
        sceneView.sceneLighting = true;
        sceneView.sceneViewState.showSkybox = true;
        sceneView.sceneViewState.showFog = true;
        sceneView.sceneViewState.showImageEffects = true;
        sceneView.Repaint();

        return $"Mode: {sceneView.cameraMode.name}, Lighting: {sceneView.sceneLighting}";
    }
}
