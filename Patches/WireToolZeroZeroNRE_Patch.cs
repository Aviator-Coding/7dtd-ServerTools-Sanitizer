using HarmonyLib;
using UnityEngine;

namespace AdminToolsSanitize
{
    [HarmonyPatch(typeof(ItemActionConnectPower))]
    class WireToolZeroZeroNRE_Patch
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
        /// Multiple Issue Happening Here.
        /// 1. As of know the Wiring tool is major broken the VectorI3 always refers to
        ///    0 0 0 no matter which position you are at in the world.
        /// 2. connectPowerData.invData.world.CanPlaceBlockAt is always false unless you are in chunk 0
        ///    and hold the wiring tool it will refer to an active placed LCB since the wiring tool always refers
        ///    to 0 0 0 
        /// 3. connectPowerData.playerUI.nguiWindowManager.SetLabelText() causes a Null Ref since its not checked
        ///    for null pay close attention i added the ? to connectPowerData.playerUI thsi resolves the NRE
        /// </summary>
        /// <param name="_actionData"></param>
        /// <returns>
        /// False - Will not run OnHoldingUpdate
        /// True - Will run OnHoldingUpdate
        /// </returns>
        [HarmonyPrefix]
        [HarmonyPatch("OnHoldingUpdate")]
        public static bool OnHoldingUpdate_prefix(ItemActionData _actionData)
        {
            if (Debug) Log.Out($"Start OnHoldingUpdate_prefix");
            ItemActionConnectPower.ConnectPowerData connectPowerData = (ItemActionConnectPower.ConnectPowerData)_actionData;
            Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;

            if (connectPowerData.invData.holdingEntity is EntityPlayerLocal && connectPowerData.playerUI == null)
            {
                connectPowerData.playerUI = LocalPlayerUI.GetUIForPlayer(connectPowerData.invData.holdingEntity as EntityPlayerLocal);
            }

            if (Debug) Log.Out($"blockPos - blockPos.x:{blockPos.x} blockPos.y:{blockPos.y} blockPos.z:{blockPos.z}");

            // This is bugged or more a result of the original Code trying to run the ui without checking the for nil
            if (!connectPowerData.invData.world.CanPlaceBlockAt(blockPos, connectPowerData.invData.world.gameManager.GetPersistentLocalPlayer(), false))
            {
                connectPowerData.isFriendly = false;
                connectPowerData.playerUI?.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null);
                return false;
            }
            return true;
        }
    }
}
