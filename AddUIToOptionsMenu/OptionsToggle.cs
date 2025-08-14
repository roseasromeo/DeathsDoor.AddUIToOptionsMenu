using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDoor.AddUIToOptionsMenu;

#nullable enable
public class OptionsToggle : OptionsMenuItem
{
    public Action<bool> ToggleAction;
    public Func<bool> ToggleValueInitializer;

    public OptionsToggle(string itemText, string gameObjectName, string id, List<IngameUIManager.RelevantScene> relevantScenes, Action<bool> toggleAction, Func<bool> toggleValueInitializer, string contextText = "")
    {
        ItemText = itemText;
        GameObjectName = gameObjectName;
        ID = id;
        RelevantScenes = relevantScenes;
        ToggleAction = toggleAction;
        ToggleValueInitializer = toggleValueInitializer;
        ContextText = contextText;
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
        newToggleText.locId = ItemText;
        if (!IngameUIManager.modifiedStrings.Contains(newToggleText.locId))
        {
            IngameUIManager.modifiedStrings.Add(newToggleText.locId);
        }
        newToggleObject.GetComponent<UIToggle>().id = ID;
        bool initialToggleValue = ToggleValueInitializer();
        newToggleObject.GetComponent<UIToggle>().master.toggleStates[ID] = initialToggleValue;
        newToggleObject.GetComponent<UIToggle>().toggle.SetActive(initialToggleValue);
        IngameUIManager.registeredToggles[ID] = Toggle;
        return newToggleObject;
    }

    public void Toggle(IngameUIManager.RelevantScene relevantScene)
    {
        UIToggle uIToggle = GetUIToggle(relevantScene);
        UIMenuOptions uIMenuOptions = uIToggle.master;
        bool currentState = uIMenuOptions.GetToggleState(ID);
        uIMenuOptions.toggleStates[ID] = !currentState;
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