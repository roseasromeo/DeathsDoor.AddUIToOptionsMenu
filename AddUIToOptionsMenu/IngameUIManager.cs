using HarmonyLib;
using UnityEngine.SceneManagement;

namespace DDoor.AddUITOOptionsMenu;

internal class IngameUIManager
{
    public static readonly IngameUIManager instance = new();

    public static IngameUIManager Instance => instance;

    public static void ModifyOptionsMenuInGame()
    {
        
    }
    public static void ModifyOptionsMenuOnTitleScreen()
    {
        
    }
    
    [HarmonyPatch]
    private class Patches()
    {
        /// <summary>
        /// Displays custom strings instead of original text
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(GameSceneManager), nameof(GameSceneManager.OnSceneLoaded))]
        private static void PostOnSceneLoaded(Scene scene)
        {
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
            if (scene.name == "_PLAYER")
            {
                ModifyOptionsMenuInGame();
            }
            else if (scene.name == "TitleScreen")
            {
                ModifyOptionsMenuOnTitleScreen();
            }
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
        }
    }
    
}