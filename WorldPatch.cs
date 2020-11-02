using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdminToolsSanitize
{
    [HarmonyPatch(typeof(World))]
    class WorldPatch
    {
		/// <summary>
		/// If Enable will Print additional debug information this is used
		/// during testing and developing of the MOD
		/// </summary>
		static bool Debug = true;
		/// <summary>
		/// This Patch will Help reduce the Exceptions Error Read Treasure Map
		/// It will make sure that we check the Water inside the world and not outside
		/// the Boundary of it.
		/// </summary>
		/// <param name="__instance">Holds the Class Instance of World</param>
		/// <param name="__result">Bool - Overrides the Original Function Return. True if water false if not</param>
		/// <param name="worldX">Holds the X Coordinate where we try to find Water</param>
		/// <param name="worldZ">Holds the Y Coordinate where we try to find Water</param>
		/// <returns>
		/// True  - Will Execute Original Game Function
		/// False - Block the Original Gam Function and overrites it return Value with result
		/// </returns>
        [HarmonyPrefix]
        [HarmonyPatch("GetWaterAt")]
        public static bool GetWaterAt_prefix(World __instance, ref bool __result, float worldX, float worldZ)
        {
			
			IChunkProvider chunkProvider = GameManager.Instance.World.ChunkClusters[0].ChunkProvider;
			Vector2i worldSize = chunkProvider.GetWorldSize();

			if (Debug) Log.Out($"[MOD - TreasureSanitze] - GetWaterAt - At worldX:{worldX} worldZ(Y):{worldZ} with WorldsizeX:{worldSize.x}, WorldsizeY(Z):{worldSize.y}");

			if (Debug) Log.Out($"[MOD - TreasureSanitze] - if {worldSize.x / 2} < {Math.Abs(worldX)} OR { worldSize.y / 2} < {Math.Abs(worldZ)}");
			//check that x and z are actually inside the World
			if ((worldSize.x / 2) < Math.Abs(worldX) || (worldSize.y / 2) < Math.Abs(worldZ))
            {
				if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE - Prevented Water Check out of the Worlds Border At  - worldX:{worldX} worldZ(Y):{worldZ}");
				__result = false;
				return false;
            }


			ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = __instance.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
			if (chunkProviderGenerateWorldFromRaw == null)
			{
				if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE - Could not find chunkProviderGenerateWorldFromRaw Object is null");
				__result = false;
				return false;
			}
			WorldDecoratorPOIFromImage poiFromImage = chunkProviderGenerateWorldFromRaw.poiFromImage;
			if (poiFromImage == null)
			{
				if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE - Could not find poiFromImage Object is null");
				__result = false;
				return false;
			}
			try
			{
				byte b = poiFromImage.m_Poi[(int)worldX, (int)worldZ];
				if (b == 0)
				{
					if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE - b==0 ");
					__result = false;
					return false;
				}
				PoiMapElement poiForColor = __instance.Biomes.getPoiForColor((uint)b);
				if(poiForColor == null)
                {
					if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE - poiForColor is a NullReference Object");
					__result = false;
					return false;
				}

				if (Debug) Log.Out($"[MOD - TreasureSanitze] Result TRUE - - IsLiquid:{Block.list[poiForColor.m_BlockValue.type].blockMaterial.IsLiquid}");
				__result = Block.list[poiForColor.m_BlockValue.type].blockMaterial.IsLiquid;
				return true;
			}
			catch (Exception e)
			{
				if (Debug) Log.Out($"[MOD - TreasureSanitze] Result FALSE  - Exception poiFromImage.m_Poi - {e.ToString()}");
				__result = false;
				return false;
			}
			
		}

	}
}
