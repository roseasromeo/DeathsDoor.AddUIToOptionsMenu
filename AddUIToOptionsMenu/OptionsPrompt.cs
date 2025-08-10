using System;
using System.Linq;
using UnityEngine;

namespace DDoor.AddUIToOptionsMenu;

public class OptionsPrompt
{
    public string PromptText;
    public string GameObjectName;
    public string ActionId;
    public Action CloseAction;

    public OptionsPrompt(string promptText, string gameObjectName, Action closeAction)
    {
        PromptText = promptText;
        GameObjectName = gameObjectName;
        CloseAction = closeAction;
    }

    private UIPrompt GetUIPrompt(IngameUIManager.RelevantScene relevantScene) =>
        PathUtil.GetByPath(PathUtil.ParentScene(relevantScene), PathUtil.OptionsMenuPath(relevantScene) + GameObjectName)
        .GetComponent<UIPrompt>();

    internal void AddOptionPrompt(IngameUIManager.RelevantScene relevantScene)
    {
        string parentScene = PathUtil.ParentScene(relevantScene);
        string optionsMenuPath = PathUtil.OptionsMenuPath(relevantScene);
        // Check if already has been added to scene, if so return
        try
        {
            PathUtil.GetByPath(parentScene, optionsMenuPath + GameObjectName);
            return;
        }
        catch (InvalidOperationException)
        {
            // If don't find the prompt already, go on to create it
        }
        GameObject optionsMenuObject = PathUtil.GetByPath(parentScene, optionsMenuPath);
        GameObject exitToTitlePopup = PathUtil.GetByPath(parentScene, optionsMenuPath + "Popup_ExitToTitle");
        GameObject newPopup = GameObject.Instantiate(exitToTitlePopup, optionsMenuObject.transform);
        newPopup.name = GameObjectName;
        LocTextTMP newPopupPromptText = newPopup.transform.Cast<Transform>()
            .First((t) => t.name == "Prompt")
            .gameObject.GetComponentInChildren<LocTextTMP>();
        newPopupPromptText.locId = PromptText;
        if (!IngameUIManager.modifiedStrings.Contains(newPopupPromptText.locId))
        {
            IngameUIManager.modifiedStrings.Add(newPopupPromptText.locId);
        }
        UIPrompt newPrompt = newPopup.GetComponent<UIPrompt>();
        newPrompt.id = ActionId;
    }

    public void OpenPromptAction(IngameUIManager.RelevantScene relevantScene, UIAction callingAction)
    {
        UIPrompt uIPrompt = GetUIPrompt(relevantScene);
        callingAction.master.currentPrompt = uIPrompt;
        uIPrompt.GainFocus(true);
        callingAction.master.canMove = false;
    }

    public void ClosePromptAction(IngameUIManager.RelevantScene relevantScene, bool value)
    {
        UIPrompt uIPrompt = GetUIPrompt(relevantScene);
        UIMenuOptions optionsMenu = (UIMenuOptions)uIPrompt.master;
        if (optionsMenu.currentPrompt == uIPrompt)
        {
            optionsMenu.currentPrompt = null;
            uIPrompt.LoseFocus(true);
            optionsMenu.canMove = true;
            if (value)
            {
                UIMenuPauseController.instance.UnPause();
                CloseAction.Invoke();
            }
        }
    }
}