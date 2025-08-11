using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDoor.AddUIToOptionsMenu;

#nullable enable
public class IngameUIManager
{
    public static readonly IngameUIManager instance = new();

    public static IngameUIManager Instance => instance;

    public static List<string> modifiedStrings = [];
    internal static Dictionary<string, Action<RelevantScene, UIAction>> registeredActions = [];
    internal static Dictionary<string, Action<RelevantScene, bool>> registeredPrompts = [];
    internal static Dictionary<string, Action<RelevantScene>> registeredToggles = [];

    private static readonly List<OptionsButton> addedOptionsButtons = [];
    private static readonly List<OptionsToggle> addedOptionsToggles = [];

    private static readonly List<Transform> activeOptionGameObjects = [];
    private static int currentSiblingIndex = 0;
    private static int addedItemsCount = 0;
    private static readonly float menuEntryHeight = 55f;

    private static UIMenuOptions uIMenuOptions = PathUtil.GetUIMenuOptions(RelevantScene.TitleScreen);

    public static void AddOptionsButton(OptionsButton optionsButton)
    {
        addedOptionsButtons.Add(optionsButton);
    }

    public static void AddOptionsToggle(OptionsToggle optionsToggle)
    {
        addedOptionsToggles.Add(optionsToggle);
    }

    public static void RetriggerModifyingOptionsMenuTitleScreen()
    {
        // If a mod add items after Title Screen loads, allow them to trigger the modification again
        ModifyOptionsMenu(RelevantScene.TitleScreen);
    }

    private static void GetActiveOptionGameObjects(RelevantScene relevantScene)
    {
        GameObject optionsMenu = PathUtil.GetByPath(PathUtil.ParentScene(relevantScene), PathUtil.OptionsMenuPath(relevantScene) + PathUtil.pathToOptionPanel);
        activeOptionGameObjects.Clear();
        foreach (Transform child in optionsMenu.transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeOptionGameObjects.Add(child);
            }
            if (child.name == "UI_ExitSession")
            {
                currentSiblingIndex = child.transform.GetSiblingIndex();
            }
        }
    }


    private static void ModifyOptionsMenu(RelevantScene relevantScene)
    {
        GetActiveOptionGameObjects(relevantScene);
        addedItemsCount = 0;
        foreach (OptionsToggle optionsToggle in addedOptionsToggles.Where(ot => ot.RelevantScenes.Contains(relevantScene) && !activeOptionGameObjects.Exists(actOb => actOb.name == ot.GameObjectName)))
        {
            ProcessOptionsToggle(optionsToggle, relevantScene);
        }
        foreach (OptionsButton optionsButton in addedOptionsButtons.Where(ob => ob.RelevantScenes.Contains(relevantScene) && !activeOptionGameObjects.Exists(actOb => actOb.name == ob.GameObjectName)))
        {
            ProcessOptionsButton(optionsButton, relevantScene);
        }
        uIMenuOptions = PathUtil.GetUIMenuOptions(relevantScene);
        ModifyOptionsMenuNavigation(relevantScene);
    }

    private static void ProcessOptionsToggle(OptionsToggle optionsToggle, RelevantScene relevantScene)
    {
        GameObject toggle = optionsToggle.AddOptionsToggle(relevantScene);
        toggle.transform.SetSiblingIndex(currentSiblingIndex);
        currentSiblingIndex += 1; //Increment the sibling index
        addedItemsCount += 1;
    }

    private static void ProcessOptionsButton(OptionsButton optionsButton, RelevantScene relevantScene)
    {
        GameObject button = optionsButton.AddOptionsButton(relevantScene);
        button.transform.SetSiblingIndex(currentSiblingIndex);
        currentSiblingIndex += 1; //Increment the sibling index
        optionsButton.OptionsPrompt?.AddOptionsPrompt(relevantScene);
        addedItemsCount += 1;
    }

    private static void ModifyOptionsMenuNavigation(RelevantScene relevantScene)
    {
        GetActiveOptionGameObjects(relevantScene);
        UIButton[] newGrid = new UIButton[activeOptionGameObjects.Count];
        int accessibilityMenuIndex = activeOptionGameObjects.FindIndex(t => t.gameObject.name == "UI_AccessMenu");
        for (int i = 0; i < activeOptionGameObjects.Count; i++)
        {
            if (accessibilityMenuIndex < i && i <= accessibilityMenuIndex + addedItemsCount)
            {
                newGrid[i] = activeOptionGameObjects[i].GetComponent<UIButton>();
            }
            else if (i > accessibilityMenuIndex + addedItemsCount)
            {
                newGrid[i] = uIMenuOptions.grid[i - addedItemsCount];
            }
            else
            {
                newGrid[i] = uIMenuOptions.grid[i];
            }
        }
        uIMenuOptions.grid = newGrid;

        string[] newCtxt = new string[10];
        for (int i = 0; i < activeOptionGameObjects.Count; i++)
        {
            if (accessibilityMenuIndex < i && i <= accessibilityMenuIndex + addedItemsCount)
            {
                newCtxt[i] = "cts_" + activeOptionGameObjects[i].gameObject.name;
            }
            else if (i > accessibilityMenuIndex + addedItemsCount)
            {
                newCtxt[i] = uIMenuOptions.ctxt[i - addedItemsCount];
            }
            else
            {
                newCtxt[i] = uIMenuOptions.ctxt[i];
            }
        }
        uIMenuOptions.ctxt = newCtxt;

        RectTransform optionsRectTransform = PathUtil.GetByPath(PathUtil.ParentScene(relevantScene), PathUtil.OptionsMenuPath(relevantScene) + "ItemWindow_9slice/").GetComponent<RectTransform>();
        optionsRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, activeOptionGameObjects.Count * menuEntryHeight);
    }

    public enum RelevantScene
    {
        TitleScreen,
        InGame,
    }

    [HarmonyPatch]
    private class Patches()
    {
        /// <summary>
        /// Modifies the options menu on scene load
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(GameSceneManager), nameof(GameSceneManager.OnSceneLoaded))]
        private static void PostOnSceneLoaded(Scene scene)
        {
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
            if (scene.name == "_PLAYER")
            {
                ModifyOptionsMenu(RelevantScene.InGame);
            }
            else if (scene.name == "TitleScreen")
            {
                ModifyOptionsMenu(RelevantScene.TitleScreen);
            }
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
        }

        /// <summary>
        /// Displays custom strings instead of original text
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(LocTextTMP), nameof(LocTextTMP.check))]
        private static bool PreCheck(LocTextTMP __instance)
        {
            if (modifiedStrings.Contains(__instance.locId))
            {
                __instance.text.text = __instance.locId;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Executes custom UIAction actionIDs
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(UIAction), nameof(UIAction.Action))]
        private static bool PreAction(UIAction __instance)
        {
            if (registeredActions.ContainsKey(__instance.actionId))
            {
                RelevantScene relevantScene = RelevantScene.InGame;
                if (SceneManager.GetActiveScene().name == "TitleScreen")
                {
                    relevantScene = RelevantScene.TitleScreen;
                }
                registeredActions[__instance.actionId].Invoke(relevantScene, __instance);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Closes custom prompt
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(UIMenuOptions), nameof(UIMenuOptions.Close))]
        private static bool PreClose(string id, bool value)
        {
            if (registeredPrompts.ContainsKey(id))
            {
                RelevantScene relevantScene = RelevantScene.InGame;
                if (SceneManager.GetActiveScene().name == "TitleScreen")
                {
                    relevantScene = RelevantScene.TitleScreen;
                }
                registeredPrompts[id].Invoke(relevantScene, value);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Intercepts toggle action for custom toggles
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(UIMenuOptions), nameof(UIMenuOptions.Toggle))]
        private static bool PreToggle(string id)
        {
            if (registeredToggles.ContainsKey(id))
            {
                RelevantScene relevantScene = RelevantScene.InGame;
                if (SceneManager.GetActiveScene().name == "TitleScreen")
                {
                    relevantScene = RelevantScene.TitleScreen;
                }
                registeredToggles[id].Invoke(relevantScene);
                return false;
            }
            return true;
        }
    }

}

#nullable disable