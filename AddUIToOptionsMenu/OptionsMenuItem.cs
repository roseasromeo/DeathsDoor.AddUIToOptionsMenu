using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace DDoor.AddUIToOptionsMenu;

public abstract class OptionsMenuItem
{
    public string GameObjectName;
    public string ItemText;
    public string ID;
    public string ContextText; // Text that is displayed at the bottom of the menu, can use BUTTON:LEFTRIGHT for Left/right buttons, BUTTON:BACK for back button, BUTTON:CONFIRM for confirm button
    public List<IngameUIManager.RelevantScene> RelevantScenes = [];

    private static string LookUpOptionsMenuContext(string GameObjectName)
    {
        if (IngameUIManager.addedOptionsButtons.Exists(ob => ob.GameObjectName == GameObjectName))
        {
            return IngameUIManager.addedOptionsButtons.First(ob => ob.GameObjectName == GameObjectName).ContextText;
        }
        if (IngameUIManager.addedOptionsToggles.Exists(ob => ob.GameObjectName == GameObjectName))
        {
            return IngameUIManager.addedOptionsToggles.First(ob => ob.GameObjectName == GameObjectName).ContextText;
        }
        return "No context string found for this menu item";
    }


    [HarmonyPatch]
    private class Patches
    {
        /// <summary>
        /// When the UIMenuOptions tries to run populateContextData (which adds the text at the bottom of the screen in the options menu), intercept if it is from our game objects and set our text instead.
        /// </summary>
        /// <returns> False if the context string is populated by this mod, so that our function runs instead </returns>
        [HarmonyPrefix, HarmonyPatch(typeof(UIMenu), nameof(UIMenu.populateContextData))]
        private static bool PopulateContextDataPatch(UIMenu __instance, string strId)
        {
            if (strId.Contains("cts_") && IngameUIManager.menuItemGameObjectNames.Contains(strId.Substring(4)))
            {
                __instance.contextData.UpdateText(LookUpOptionsMenuContext(strId.Substring(4)));
                return false;
            }
            return true;
        }
    }

}