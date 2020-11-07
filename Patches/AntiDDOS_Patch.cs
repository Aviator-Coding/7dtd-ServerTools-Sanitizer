using HarmonyLib;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminToolsSanitize
{
    public class DDOSTracker
    {
#if DEBUG
        public static bool Debug = true;
#else
        public static bool Debug = false;
#endif
        /// <summary>
        /// Keeps track of the IP
        /// </summary>
        public static Dictionary<string, int> AdressTracker = new Dictionary<string, int>();

        /// <summary>
        /// Maximum Connections allowed from the same IP
        /// </summary>
        public static int MaxSameConnections = 10;

        /// <summary>
        /// Maximum Client Connection before rejection from every CLient
        /// </summary>
        public static int MaxClientConnection = 80;

        /// <summary>
        /// Once we passed 20 Conenction per IP
        /// The decreasing function is ignored the only way
        /// this will get unblocked is trough the Interval Timer
        /// this has been done to prevent really noisy ip to be unblocked
        /// early
        /// </summary>
        public static int MaxConnecctionBeforeReseIgnore = 20;

        /// <summary>
        /// Defines the Cleanup Time in Minutes between every Interval
        /// </summary>
        public static int CleanInterval = 5;

        /// <summary>
        /// Holds the Time when the Cleanup has been last time run
        /// </summary>
        public static DateTime TimeLastListWipe = DateTime.Now;

        /// <summary>
        /// Will Clean the IP Tracker based on the Interval Selected up top
        /// </summary>
        public static void TimeBaseCleanup()
        {
            if (DateTime.Now.Subtract(TimeLastListWipe).TotalMinutes > CleanInterval)
            {
                AdressTracker.Clear();
                TimeLastListWipe = DateTime.Now;
                if (Debug) Log.Out($"[DDOS-MOD] TimeBaseCleanup Clear IP List");
            }
            else { if (Debug) Log.Out($"[DDOS-MOD] TimeBaseCleanup not quiet time yet {DateTime.Now.Subtract(TimeLastListWipe).TotalMinutes - CleanInterval} Minutes left before cleanup"); }
        }

        /// <summary>
        /// Increases the Connection by one
        /// If not existings creates a new Object
        /// </summary>
        /// <param name="ClientIP">takes the CLient IP as String eg 8.8.8.8</param>
        public static void IncreaseCount(string ClientIP)
        {
            if (Debug) Log.Out($"[DDOS-MOD] Testing Address {ClientIP}");
            //Manages the Count of the Connection per IP
            if (AdressTracker.ContainsKey(ClientIP))
            {
                if (AdressTracker[ClientIP] > MaxSameConnections + 10)
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Counter is to High Skip Increasing {ClientIP}");
                }
                else
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Increase Address {ClientIP} From {AdressTracker[ClientIP]} ");
                    AdressTracker[ClientIP]++;
                    if (Debug) Log.Out($"[DDOS-MOD] Increase Address {ClientIP} To {AdressTracker[ClientIP]} ");
                }

            }
            else
            {
                AdressTracker.Add(ClientIP, 1);
                if (Debug) Log.Out($"[DDOS-MOD] Adress {ClientIP} Has none known Connection Add {AdressTracker[ClientIP]} ");
            }
        }

        /// <summary>
        /// Decreases the Amount of IP Connected 
        /// </summary>
        /// <param name="ClientIP">takes the CLient IP as String eg 8.8.8.8</param>
        public static void DecreaseCount(string ClientIP)
        {
            if (AdressTracker.ContainsKey(ClientIP))
            {
                if (AdressTracker[ClientIP] > MaxConnecctionBeforeReseIgnore) return;  //ignore Resets to the Counter on Excessive Spam we will use the Timer.
                if (Debug) Log.Out($"[DDOS-MOD] Decrease Address {ClientIP} for Disconnect! old Count {AdressTracker[ClientIP]}");
                AdressTracker[$"{ClientIP}"]--;
                if (Debug) Log.Out($"[DDOS-MOD] Decrease Address {ClientIP} for Disconnect! new Count {AdressTracker[ClientIP]}");
                if (AdressTracker[ClientIP] <= 0)
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Address {ClientIP} Count {AdressTracker[ClientIP]} is below 0 we remove the Entry");
                    AdressTracker.Remove(ClientIP);
                }
            }
            else
            {
                if (Debug) Log.Out($"[DDOS-MOD] DropClient Unable to find Address {ClientIP}");
            }
        }
    }

    [HarmonyPatch(typeof(NetworkServerLiteNetLib))]
    class AntiDDOS_Connect_Patch
    {

        /// <summary>
        /// There is an Issue where a CLient can Spam connect and disconnect request
        /// this will cause 
        ///     1. The Network Connection to lock up
        ///     2. The logs to flood where the System Interrupt are trough the roof
        /// Attack is been executed as follows:
        /// Open as multiple (1000) Connections to the Server with different source port
        ///     1. Send Login Message: \x08\x07\x00\x00\x00\x8e\x52\xd7\x6f\xdf\xd2\xd7\x08\x00\x00\x00\x00
        ///     2. Send Disconnet Message: \x0a\x8e\x52\xd7\x6f\xdf\xd2\xd7\x08
        /// This will essential lock up the Server and crash it
        /// This Mod does following. It Counts the Connection from a source IP
        /// if Connection from the same ip is greater than MaxSameConnection we will reject farther 
        /// connections.
        /// We Limit Max Connections to 120 this is in case are different Ips are used. This will allow the
        /// current Player to Play but not Crash the Game
        /// </summary>
        /// <param name="___serverPassword">Server Password if Set</param>
        /// <param name="_request">LiteNetLib Connection Request</param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch("ConnectionRequestCheck")]
        public static bool ConnectionRequestCheck_Patch(string ___serverPassword, ConnectionRequest _request)
        {
            // Clean List if needed
            DDOSTracker.TimeBaseCleanup();

            string text = _request.RemoteEndPoint.Address.ToString();
            ClientInfoCollection ClientInfoList = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients;
            string[] address = _request.RemoteEndPoint.Address.ToString().Split(':');
            string ClientIP = address[0];

            DDOSTracker.IncreaseCount(ClientIP);

            // Check if there are to many connections
            if (DDOSTracker.AdressTracker[ClientIP] > DDOSTracker.MaxSameConnections)
            {
                if (DDOSTracker.AdressTracker[ClientIP] < DDOSTracker.MaxSameConnections + 5)
                    Log.Out($"[DDOS-MOD] to Many Conenctions from the Same IP {ClientIP} exceeded {DDOSTracker.MaxSameConnections} Disconnecting now");
                _request.Reject();
                return false;
            }


            // Maximum Client Connection Exceeded
            if (ClientInfoList.Count > DDOSTracker.MaxClientConnection)
            {
                Log.Out($"[DDOS-MOD] Dropping Connection from {_request.RemoteEndPoint.Address} to Many total Connections to the Server {ClientInfoList.Count} / {DDOSTracker.MaxClientConnection} ");
                _request.Reject();
                return false;
            }

            foreach (ClientInfo clientInfo in ClientInfoList.List)
            {
                // Original Game Code
                if (!clientInfo.loginDone && clientInfo.ip == text)
                {
                    Log.Out("NET: Rejecting connection request from " + text + ": A connection attempt from that IP is currently being processed!");
                    _request.Reject();
                    return false;
                }
            }
            _request.AcceptIfKey(___serverPassword);
            return false;
        }


        /// <summary>
        /// DropClient is called when a Player Logs out of the Game
        /// We have decrease the Count of the Player with the Same
        /// IP.
        /// </summary>
        /// <param name="_clientInfo"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch("DropClient")]
        public static bool DropClient_Patch(ClientInfo _clientInfo)
        {
            if (_clientInfo != null)
            {
                string[] address = _clientInfo.ip.Split(':');
                DDOSTracker.DecreaseCount(address[0]);
            }
            return true;
        }

    }


    [HarmonyPatch(typeof(AuthorizationManager))]
    public class AntiDDOS_AuthorizationManager_Patch
    {
        /// <summary>
        /// Authorisation Denied is Triggered When a Player gets Kicked of the Server
        /// we will decrease our count of Connection
        /// </summary>
        /// <param name="_cInfo"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch("AuthorizationDenied")]
        public static bool AuthorizationDenied(IAuthorizer _authorizer, ClientInfo _clientInfo, GameUtils.KickPlayerData _kickPlayerData)
        {
            if (DDOSTracker.Debug) Log.Out($"[DDOS-MOD] Client:{_clientInfo.ToString()} Address:{_clientInfo.ip} KickReason:{_kickPlayerData.ToString()} ");
            string[] address = _clientInfo.ip.Split(':');
            DDOSTracker.DecreaseCount(address[0]);

            return true;
        }
    }

    /// <summary>
    /// This Ensures that Server which uses
    /// /Kick to leave get their Connection Reset
    /// else we could end up blocking them
    /// </summary>
    [HarmonyPatch(typeof(GameUtils))]
    public class AntiDDOS_GameUtils_Patch
    {
        /// <summary>
        /// Authorisation Denied is Triggered When a Player gets Kicked of the Server
        /// we will decrease our count of Connection
        /// </summary>
        /// <param name="_cInfo"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch("KickPlayerForClientInfo")]
        public static bool KickPlayerForClientInfo(ClientInfo _cInfo, GameUtils.KickPlayerData _kickData)
        {
            if (DDOSTracker.Debug) Log.Out($"[DDOS-MOD] Client:{_cInfo.ToString()} Address:{_cInfo.ip} KickReason:{_kickData.ToString()} ");
            string[] address = _cInfo.ip.Split(':');
            DDOSTracker.DecreaseCount(address[0]);

            return true;
        }
    }
}
