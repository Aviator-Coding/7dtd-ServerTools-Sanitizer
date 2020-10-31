using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace AdminToolsSanitize
{
    [HarmonyPatch(typeof(AdminTools))]
    class AdminToolsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Save")]
        public static bool Save_CleanUp(AdminTools __instance,
                                      ref Dictionary<string, AdminToolsClientInfo> ___userPermissions,
                                      ref Dictionary<string, AdminToolsGroupPermissions> ___groupPermissions,
                                      ref Dictionary<string, AdminToolsClientInfo> ___whitelistedUsers,
                                      ref Dictionary<string, AdminToolsClientInfo> ___whitelistedGroups,
                                      ref Dictionary<string, AdminToolsClientInfo> ___bannedUsers,
                                      ref Dictionary<string, AdminToolsCommandPermissions> ___commands)
        {
            Log.Out("[MOD - AdminToolsSanitize] Sanitizing userPermissions");
            //Clean UserPermission
            foreach (string index in ___userPermissions.Keys.ToList())
            {
                ___userPermissions[index] = new AdminToolsClientInfo(SecurityElement.Escape(___userPermissions[index].Name), 
                                                                     ___userPermissions[index].SteamId,
                                                                     ___userPermissions[index].PermissionLevel);
            }

            Log.Out("[MOD - AdminToolsSanitize] Sanitizing groupPermissions");
            //Clean Group Permissions
            foreach (string index in ___groupPermissions.Keys.ToList())
            {
                ___groupPermissions[index] = new AdminToolsGroupPermissions(SecurityElement.Escape(___groupPermissions[index].Name), 
                                                                            ___groupPermissions[index].SteamIdGroup, 
                                                                            ___groupPermissions[index].PermissionLevelNormal,
                                                                            ___groupPermissions[index].PermissionLevelMods); 
            }

            Log.Out("[MOD - AdminToolsSanitize] Sanitizing whitelistedUsers");
            //Clean WhiteList Users
            foreach (string index in ___whitelistedUsers.Keys.ToList())
            {
                ___whitelistedUsers[index] = new AdminToolsClientInfo(SecurityElement.Escape(___whitelistedUsers[index].Name),
                                                                      ___whitelistedUsers[index].SteamId,
                                                                      ___whitelistedUsers[index].PermissionLevel);
            }

            Log.Out("[MOD - AdminToolsSanitize] Sanitizing whitelistedGroups");
            //Cleam Whitelis Groups
            foreach (string index in ___whitelistedGroups.Keys.ToList())
            {
                ___whitelistedGroups[index] = new AdminToolsClientInfo(SecurityElement.Escape(___whitelistedGroups[index].Name),
                                                                       ___whitelistedGroups[index].SteamId,
                                                                       ___whitelistedGroups[index].PermissionLevel);
            }

            Log.Out("[MOD - AdminToolsSanitize] Sanitizing bannedUsers");
            // Clean BannedUsers
            foreach (string index in ___bannedUsers.Keys.ToList())
            {
                ___bannedUsers[index] = new AdminToolsClientInfo(SecurityElement.Escape(___bannedUsers[index].Name),
                                                                 ___bannedUsers[index].SteamId,
                                                                 ___bannedUsers[index].BannedUntil,
                                                                 SecurityElement.Escape(___bannedUsers[index].BanReason));
            }

            return true;
        }
    }
}
