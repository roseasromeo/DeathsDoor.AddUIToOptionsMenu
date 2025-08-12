using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDoor.AddUIToOptionsMenu;

#nullable enable
public class OptionsToggle
{
    public string ToggleText;
    public string GameObjectName;
    public string ToggleID;
    public List<IngameUIManager.RelevantScene> RelevantScenes = [];
    public Action<bool> ToggleAction;
    public Func<bool> ToggleValueInitializer;

    public OptionsToggle(string toggleText, string gameObjectName, string toggleID, List<IngameUIManager.RelevantScene> relevantScenes, Action<bool> toggleAction, Func<bool> toggleValueInitializer)
    {
        ToggleText = toggleText;
        GameObjectName = gameObjectName;
        ToggleID = toggleID;
        RelevantScenes = relevantScenes;
        ToggleAction = toggleAction;
        ToggleValueInitializer = toggleValueInitializer;
    }

    private UIToggle GetUIToggle(IngameUIManager.RelevantScene relevantScene) =>
        PathUtil.GetByPath(PathUtil.ParentScene(relevantScene), PathUtil.OptionsMenuPath(relevantScene) + PathUtil.pathToOptionPanel + GameObjectName)
        .GetComponent<UIToggle>();

    public GameObject AddOptionsToggle(IngameUIManager.RelevantScene relevantScene)
    {
        string parentScene = PathUtil.ParentScene(relevantScene);
        string optionsMenuPath = PathUtil.OptionsMenuPath(relevantScene);
        // Check if already has been added to scene, if so return
        try
        {
            return PathUtil.GetByPath(parentScene, optionsMenuPath + PathUtil.pathToOptionPanel + GameObjectName);
        }
        catch (InvalidOperationException)
        {
            // If don't find the toggle already, go on to create it
        }
        GameObject optionsPanel = PathUtil.GetByPath(parentScene, optionsMenuPath + PathUtil.pathToOptionPanel);
        GameObject baseToggle = PathUtil.GetByPath(parentScene, optionsMenuPath + "SubMenu_Access/ItemWindow/UI_ToggleBloodEffect");
        GameObject newToggleObject = GameObject.Instantiate(baseToggle, optionsPanel.transform);
        newToggleObject.name = GameObjectName;
        LocTextTMP newToggleText = newToggleObject.GetComponentInChildren<LocTextTMP>();
        newToggleText.locId = ToggleText;
        if (!IngameUIManager.modifiedStrings.Contains(newToggleText.locId))
        {
            IngameUIManager.modifiedStrings.Add(newToggleText.locId);
        }
        newToggleObject.GetComponent<UIToggle>().id = ToggleID;
        bool initialToggleValue = ToggleValueInitializer();
        newToggleObject.GetComponent<UIToggle>().master.toggleStates[ToggleID] = initialToggleValue;
        newToggleObject.GetComponent<UIToggle>().toggle.SetActive(initialToggleValue);
        IngameUIManager.registeredToggles[ToggleID] = Toggle;
        return newToggleObject;
    }

    public void Toggle(IngameUIManager.RelevantScene relevantScene)
    {
        UIToggle uIToggle = GetUIToggle(relevantScene);
        UIMenuOptions uIMenuOptions = uIToggle.master;
        bool currentState = uIMenuOptions.GetToggleState(ToggleID);
        uIMenuOptions.toggleStates[ToggleID] = !currentState;
        uIToggle.toggle.SetActive(!currentState);
        ToggleAction.Invoke(!currentState);
    }

    [HarmonyPatch]
    private class Patches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(UIToggle), nameof(UIToggle.init))]
        private static bool InitPatch(UIToggle __instance)
        {
            // If it's one of our toggles, we've done the init already
            return !IngameUIManager.registeredToggles.ContainsKey(__instance.id);
        }
    }
}