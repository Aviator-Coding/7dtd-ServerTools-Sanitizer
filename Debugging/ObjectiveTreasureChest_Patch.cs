using HarmonyLib;
using UnityEngine;

namespace AdminToolsSanitize
{
    [HarmonyPatch(typeof(ObjectiveTreasureChest))]
    class ObjectiveTreasureChest_Patch
    {
        /// <summary>
        /// If Enable will Print additional debug information this is used
        /// during testing and developing of the MOD
        /// </summary>
#if DEBUG
        static bool Debug = true;
#else
         static bool Debug = false;
#endif
        /// <summary>
        /// The World world.GetGameRandom().RandomFloat and the Playerposition
        /// Math is not oprimal if you sit at the Border at -3000 -1 -3000 it calucluates
        /// Points at -3500 -3500 on a Map Which has a size of 6K causing it sometimes to
        /// have like 40-50 attempts to spawn a treasure The World.GetWaterAt has already
        /// been Patched as well since it would trough an NRE trying to find water outside
        /// the actual map. This Function does not fix anything its there for Debug porpuses
        /// </summary>
        /// <param name="__instance">Holds the Instance of the Object ObjectiveTreasureChest</param>
        /// <param name="__result">Holds the Vector31 return value where to spawn a treasure</param>
        /// <param name="playerID">Holds the PlayerId which is trying to spawn a treasure</param>
        /// <param name="distance">Holds the disctance which should be used</param>
        /// <param name="offset">???</param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch("CalculateTreasurePoint")]
        public static bool backupCalculateTreasurePoint_prefix(ObjectiveTreasureChest __instance, ref Vector3i __result, int playerID, float distance, int offset)
        {
            if (Debug) Log.Out("[MOD - TreasureSanitze] - CalculateTreasurePoint - Start");
            World world = GameManager.Instance.World;
            EntityAlive entityAlive = world.GetEntity(playerID) as EntityAlive;
            float RandomGenX = world.GetGameRandom().RandomFloat;
            float RandomGenZ = world.GetGameRandom().RandomFloat;
            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - Random Point Generated " +
                               $"RandomGenX:{RandomGenX},RandomGenZ{RandomGenZ} ");
            Vector3 RandomVector3Location = new Vector3(RandomGenX * 2f + -1f, 0f, RandomGenZ * 2f + -1f);
            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - Random Point Generated " +
                   $"RandomLocation.x:{RandomVector3Location.x}, RandomLocation.y{RandomVector3Location.y},RandomLocation.z{RandomVector3Location.z}");
            RandomVector3Location.Normalize();
            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - Random Point Generated Normalized " +
                   $"RandomLocation.x:{RandomVector3Location.x}, RandomLocation.y{RandomVector3Location.y},RandomLocation.z{RandomVector3Location.z}");
            Vector3 TreasureSpawnPoint = entityAlive.position + RandomVector3Location * distance;
            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - entityAlive X:{entityAlive.position.x}, Y:{entityAlive.position.y} ,Z:{entityAlive.position.z}");
            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - Random Point Generated XZ Calculated " +
                   $"TreasureSpawnPoint.x:{TreasureSpawnPoint.x}, TreasureSpawnPoint.y{TreasureSpawnPoint.y},TreasureSpawnPoint.z{TreasureSpawnPoint.z}");
            if (!GameManager.Instance.World.CheckForLevelNearbyHeights(TreasureSpawnPoint.x, TreasureSpawnPoint.z, 5))
            {
                if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - CheckForLevelNearbyHeights({TreasureSpawnPoint.x}, {TreasureSpawnPoint.z}, 5) FAIL return Vector3i(0, -99999, 0)");
                __result = new Vector3i(0, -99999, 0);
                return false;
            }
            if (GameManager.Instance.World.GetWaterAt(TreasureSpawnPoint.x, TreasureSpawnPoint.z))
            {
                if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - GetWaterAt({TreasureSpawnPoint.x}, {TreasureSpawnPoint.z}) return Vector3i(0, -99999, 0)");
                __result = new Vector3i(0, -99999, 0);
                return false;
            }
            int x = (int)TreasureSpawnPoint.x;
            int z = (int)TreasureSpawnPoint.z;
            int y = (int)GameManager.Instance.World.GetHeightAt(TreasureSpawnPoint.x, TreasureSpawnPoint.z);
            Vector3i vector3i = new Vector3i(x, y, z);
            Vector3 vector2 = new Vector3((float)vector3i.x, (float)vector3i.y, (float)vector3i.z);

            if (world.IsPositionInBounds(vector2) && (!(entityAlive is EntityPlayer) || world.CanPlaceBlockAt(vector3i, GameManager.Instance.GetPersistentLocalPlayer(), false)) && !world.IsPositionWithinPOI(vector2, offset))
            {
                if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - FinalSpawn Point " +
                        $"TreasureSpawnPoint.x:{vector2.x}, TreasureSpawnPoint.y{vector2.y},TreasureSpawnPoint.z{vector2.z}");
                __result = vector3i;
                return false;
            }

            if (Debug) Log.Out($"[MOD - TreasureSanitze] - CalculateTreasurePoint - NOT IsPositionInBounds return Vector3i(0, -99999, 0)");
            __result = new Vector3i(0, -99999, 0);
            return false;
        }
    }
}
