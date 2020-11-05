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

    [HarmonyPatch(typeof(GameManager))]
    class GameManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PlayerSpawnedInWorld")]
        public static bool PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
        {
			EntityAlive _entityAlive = GameManager.Instance.World.GetEntity(_entityId) as EntityAlive;
			PersistentPlayerData data = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_entityId);
			Log.Out($"[PlayerSpawnedInWorld] - entityID:{_cInfo.entityId} , PlayerID:{_cInfo.playerId} , SpawnPosition:{_pos.ToStringNoBlanks()} , RespawnType:{_respawnReason.ToString()}, PERS-Bedroll:{data.BedrollPos.ToStringNoBlanks()} Alive-Bedroll:{_entityAlive.SpawnPoints.GetPos().ToStringNoBlanks()}");
			return true;
        }

    }
}
