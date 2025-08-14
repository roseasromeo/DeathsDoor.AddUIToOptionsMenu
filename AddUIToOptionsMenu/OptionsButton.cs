using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDoor.AddUIToOptionsMenu;

#nullable enable
public class OptionsButton : OptionsMenuItem
{
    public OptionsPrompt? OptionsPrompt;
    public Action<IngameUIManager.RelevantScene, UIAction>? ButtonAction; //Mutually exclusive with optionsPrompt

    public OptionsButton(string itemText, string gameObjectName, string id, List<IngameUIManager.RelevantScene> relevantScenes, OptionsPrompt optionsPrompt, string contextText = "")
    {
        ItemText = itemText;
        GameObjectName = gameObjectName;
        ID = id;
        RelevantScenes = relevantScenes;
        OptionsPrompt = optionsPrompt;
        ContextText = contextText;
    }

    public OptionsButton(string itemText, string gameObjectName, string id, List<IngameUIManager.RelevantScene> relevantScenes, Action<IngameUIManager.RelevantScene, UIAction> buttonAction, string contextText = "")
    {
        ItemText = itemText;
        GameObjectName = gameObjectName;
        ID = id;
        RelevantScenes = relevantScenes;
        ButtonAction = buttonAction;
        ContextText = contextText;
    }

    public GameObject AddOptionsButton(IngameUIManager.RelevantScene relevantScene)
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
            // If don't find the button already, go on to create it
        }
        GameObject optionsPanel = PathUtil.GetByPath(parentScene, optionsMenuPath + PathUtil.pathToOptionPanel);
        GameObject baseButton = PathUtil.GetByPath(parentScene, optionsMenuPath + PathUtil.pathToOptionPanel + "UI_ExitSession"); // Use Exit To Title as base
        GameObject newButtonObject = GameObject.Instantiate(baseButton, optionsPanel.transform);
        newButtonObject.name = GameObjectName;
        newButtonObject.SetActive(true); // In case you are on title screen, Exit to Title would not have been active
        LocTextTMP newButtonText = newButtonObject.GetComponentInChildren<LocTextTMP>();
        newButtonText.locId = ItemText;
        if (!IngameUIManager.modifiedStrings.Contains(newButtonText.locId))
        {
            IngameUIManager.modifiedStrings.Add(newButtonText.locId);
        }

        if (OptionsPrompt != null)
        {
            IngameUIManager.registeredActions[ID] = OptionsPrompt.OpenPromptAction;
            OptionsPrompt.ActionId = ID;
            IngameUIManager.registeredPrompts[ID] = OptionsPrompt.ClosePromptAction;
        }
        else if (ButtonAction != null)
        {
            IngameUIManager.registeredActions[ID] = ButtonAction;
        }
        newButtonObject.GetComponent<UIAction>().actionId = ID;
        return newButtonObject;
    }
}
#nullable disable