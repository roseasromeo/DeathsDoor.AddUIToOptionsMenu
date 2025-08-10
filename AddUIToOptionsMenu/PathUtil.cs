using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDoor.AddUIToOptionsMenu;

public static class PathUtil
{
    public static GameObject GetByPath(string parentScene, string path)
    {
        string[] elements = path.Trim('/').Split('/');
        Scene activeScene = SceneManager.GetSceneByName(parentScene);
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        GameObject root = rootObjects.First((go) => go.name == elements[0]);
        GameObject current = root;
        foreach (string element in elements.Skip(1))
        {
            current = current.transform.Cast<Transform>()
            .First((t) => t.name == element)
            .gameObject;
        }
        return current;
    }

    internal static readonly string pathToOptionPanel = "ItemWindow_9slice/ItemWindow/";

    public static string ParentScene(IngameUIManager.RelevantScene relevantScene) =>
        relevantScene switch
        {
            IngameUIManager.RelevantScene.InGame => "_PLAYER",
            IngameUIManager.RelevantScene.TitleScreen => "TitleScreen",
            _ => throw new System.NotImplementedException("Invalid RelevantScene value"),
        };

    public static string OptionsMenuPath(IngameUIManager.RelevantScene relevantScene) =>
        relevantScene switch
        {
            IngameUIManager.RelevantScene.InGame => "UI_PauseCanvas/MENU_Pause/Content/Panels/MENU_Options/",
            IngameUIManager.RelevantScene.TitleScreen => "UI_PauseCanvas/BGMask/OptionsPanels/MENU_Options_NEW/",
            _ => throw new System.NotImplementedException("Invalid RelevantScene value"),
        };

    public static UIMenuOptions GetUIMenuOptions(IngameUIManager.RelevantScene relevantScene) => GetByPath(ParentScene(relevantScene), OptionsMenuPath(relevantScene)).GetComponent<UIMenuOptions>();
}