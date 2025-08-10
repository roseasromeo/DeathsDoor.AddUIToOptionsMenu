using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDoor.AddUIToOptionsMenu;

#nullable enable
public class OptionsButton
{
    public string ButtonText;
    public string GameObjectName;
    public string ActionID;
    public List<IngameUIManager.RelevantScene> RelevantScenes = [];
    public OptionsPrompt? OptionsPrompt;
    public Action<IngameUIManager.RelevantScene,UIAction>? ButtonAction; //Mutually exclusive with optionsPrompt

    public OptionsButton(string buttonText, string gameObjectName, string actionID, List<IngameUIManager.RelevantScene> relevantScenes, OptionsPrompt optionsPrompt)
    {
        ButtonText = buttonText;
        GameObjectName = gameObjectName;
        ActionID = actionID;
        RelevantScenes = relevantScenes;
        OptionsPrompt = optionsPrompt;
    }

    public OptionsButton(string buttonText, string gameObjectName, string actionID, List<IngameUIManager.RelevantScene> relevantScenes, Action<IngameUIManager.RelevantScene,UIAction> buttonAction)
    {
        ButtonText = buttonText;
        GameObjectName = gameObjectName;
        ActionID = actionID;
        RelevantScenes = relevantScenes;
        ButtonAction = buttonAction;
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
        GameObject baseButton = relevantScene switch
        {
            IngameUIManager.RelevantScene.InGame => PathUtil.GetByPath(parentScene, optionsMenuPath + PathUtil.pathToOptionPanel + "UI_ExitSession"), // Use Exit To Title as base if in game
            IngameUIManager.RelevantScene.TitleScreen => PathUtil.GetByPath(parentScene, "UI_PauseCanvas/BGMask/OptionsPanels/MENU_KeyBindings_NEW/FirstMenu/ItemWindow/UI_Keyboard"), // Use Keyboard and Mouse controls button if on title screen
            _ => throw new System.NotImplementedException("Invalid RelevantScene value"),
        };
        GameObject newButtonObject = GameObject.Instantiate(baseButton, optionsPanel.transform);
        newButtonObject.name = GameObjectName;
        LocTextTMP newButtonText = newButtonObject.GetComponentInChildren<LocTextTMP>();
        newButtonText.locId = ButtonText;
        if (!IngameUIManager.modifiedStrings.Contains(newButtonText.locId))
        {
            IngameUIManager.modifiedStrings.Add(newButtonText.locId);
        }

        if (OptionsPrompt != null)
        {
            IngameUIManager.registeredActions[ActionID] = OptionsPrompt.OpenPromptAction;
            OptionsPrompt.ActionId = ActionID;
            IngameUIManager.registeredPrompts[ActionID] = OptionsPrompt.ClosePromptAction;
        }
        else if (ButtonAction != null)
        {
            IngameUIManager.registeredActions[ActionID] = ButtonAction;
        }
        newButtonObject.GetComponent<UIAction>().actionId = ActionID;
        return newButtonObject;
    }
}
#nullable disable