using HarmonyLib;
using Steamworks;
using System.Reflection;

namespace AdminToolsSanitize
{
	[HarmonyPatch(typeof(EntityBedrollPositionList))]
	class EntityBedrollPositionList_Patch
	{
		/// <summary>
		/// Sets the Bedroll Position
		/// </summary>
		/// <param name="__instance"></param>
		/// <param name="___theEntity"></param>
		/// <param name="_pos"></param>
		/// <returns></returns>
		[HarmonyPrefix]
		[HarmonyPatch("Set")]
		public static bool Set_Prefix(EntityBedrollPositionList __instance, EntityAlive ___theEntity, Vector3i _pos)
		{
			PersistentPlayerData data = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(___theEntity.entityId);
			if (data != null)
			{
				Log.Out($"[EntityBedrollPositionList.Set] SetBedroll EntityID:{data.EntityId} - PlayerID:{data.PlayerId}, Old BedrollPos:({data.BedrollPos.ToStringNoBlanks()}) New BedrollPos:({_pos.ToStringNoBlanks()})");
			}
			return true;
		}
	}

	/// <summary>
	/// Debugging Bedroll Glitch print Spawn and Bedroll Position 
	/// </summary>
    [HarmonyPatch(typeof(GameManager))]
    class GameManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PlayerSpawnedInWorld")]
        public static bool PlayerSpawnedInWorld_Prefix(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
        {
			EntityAlive _entityAlive = GameManager.Instance.World.GetEntity(_entityId) as EntityAlive;
			PersistentPlayerData data = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_entityId);
			Log.Out($"[PlayerSpawnedInWorld] - entityID:{_cInfo.entityId} , PlayerID:{_cInfo.playerId} , SpawnPosition:{_pos.ToStringNoBlanks()} , RespawnType:{_respawnReason.ToString()}, PERS-Bedroll:{data.BedrollPos.ToStringNoBlanks()} Alive-Bedroll:{_entityAlive.SpawnPoints.GetPos().ToStringNoBlanks()}");
			return true;
        }

    }

	[HarmonyPatch(typeof(NetPackagePlayerSpawnedInWorld))]
	class NetPackagePlayerSpawnedInWorld_Patch
	{
		static AccessTools.FieldRef<NetPackagePlayerSpawnedInWorld, RespawnType> __respawnReason = AccessTools.FieldRefAccess<NetPackagePlayerSpawnedInWorld, RespawnType>("respawnReason");
		static AccessTools.FieldRef<NetPackagePlayerSpawnedInWorld, Vector3i> __position = AccessTools.FieldRefAccess<NetPackagePlayerSpawnedInWorld, Vector3i>("position");
		static AccessTools.FieldRef<NetPackagePlayerSpawnedInWorld, int> __entityId = AccessTools.FieldRefAccess<NetPackagePlayerSpawnedInWorld, int>("entityId");

		[HarmonyPrefix]
		[HarmonyPatch("ProcessPackage")]
		public static bool ProcessPackage_Prefix(NetPackagePlayerSpawnedInWorld __instance, World _world, INetConnectionCallbacks _netConnectionCallback)
		{
			EntityAlive _entityAlive = GameManager.Instance.World.GetEntity(__entityId(__instance)) as EntityAlive;
			PersistentPlayerData data = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(__entityId(__instance));
			Log.Out($"[NetPackagePlayerSpawnedInWorld] - SenderEntityID:{__instance?.Sender.entityId} , EntityID:{__entityId(__instance)}, PlayerID:{__instance?.Sender.playerId} , SpawnPosition:{__position(__instance).ToStringNoBlanks()} , RespawnType:{__respawnReason(__instance).ToString()}, PERS-Bedroll:{data.BedrollPos.ToStringNoBlanks()} Alive-Bedroll:{_entityAlive.SpawnPoints.GetPos().ToStringNoBlanks()}");
			return true;
		}

	}
}
