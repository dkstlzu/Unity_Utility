using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;

[Overlay(typeof(SceneView), "게임 시작", true)]
public class DevGameStartOverlay : ToolbarOverlay
{
    public DevGameStartOverlay() : base(DevGameStartData.Default.SceneList.Select(s => $"{s.name}").ToArray())
    {
    }
}

public class PlayOnSceneOverlayButton : EditorToolbarButton
{
    public PlayOnSceneOverlayButton(string sceneName, string buttonText, Action actionAfterPlay = null) : base(buttonText, () =>
    {
        SceneAsset sceneAsset = DevGameStartData.Default.SceneList.Find(scene => scene.name.Contains(sceneName));

        if (sceneAsset == null)
        {
            Printer.Print($"{buttonText}-{sceneName} not found from DevGameStartData.Default");
            return;
        }
        
        DevGameStartData.Play(sceneAsset);
        
        actionAfterPlay?.Invoke();
    })
    {
    }
}