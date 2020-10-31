using HarmonyLib;

namespace AdminToolsSanitize
{
    class Patch
    {
        public static bool Debug = false;
        //Patches the Game wwith Custom Hooks
        public static void Game()
        {
            Harmony harmony = new Harmony("com.aviator.ServerToolsPatch");
            harmony.PatchAll();
        }
    }
}
