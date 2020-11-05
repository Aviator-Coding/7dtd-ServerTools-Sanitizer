using HarmonyLib;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminToolsSanitize
{
    [HarmonyPatch(typeof(NetworkServerLiteNetLib))]
    class AntiDDOS_Patch
    {
#if DEBUG
        static bool Debug = true;
#else
         static bool Debug = false;
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
            string text = _request.RemoteEndPoint.Address.ToString();
            ClientInfoCollection ClientInfoList = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients;
            string[] address = _request.RemoteEndPoint.Address.ToString().Split(':');
            if (Debug) Log.Out($"[DDOS-MOD] Testing Address {address[0]}");
            if (AdressTracker.ContainsKey(address[0]))
            {
                if (AdressTracker[address[0]] > MaxSameConnections+10)
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Counter is to High Skip Increasing {address[0]}");
                }
                else
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Increase Address {address[0]} From {AdressTracker[address[0]]} ");
                    AdressTracker[address[0]]++;
                    if (Debug) Log.Out($"[DDOS-MOD] Increase Address {address[0]} To {AdressTracker[address[0]]} ");
                }

            }
            else
            {
                AdressTracker.Add(address[0], 1);
                if (Debug) Log.Out($"[DDOS-MOD] Adress {address[0]} Has none known Connection Add {AdressTracker[address[0]]} ");
            }


            // Check if there are to many connections
            if (AdressTracker[address[0]] > MaxSameConnections)
            {
                if(AdressTracker[address[0]] < MaxSameConnections + 5)
                    Log.Out($"[DDOS-MOD] to Many Conenctions from the Same IP {address[0]} exceeded {MaxSameConnections} Disconnecting now");
                _request.Reject();
                return false;
            }


            // Maximum Cleitn Connection Exceeded
            if (ClientInfoList.Count > MaxClientConnection)
            {
                Log.Out($"[DDOS-MOD] Dropping Connection from {_request.RemoteEndPoint.Address} to Many total Connections to the Server {ClientInfoList.Count} / {MaxClientConnection} ");
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

        [HarmonyPrefix]
        [HarmonyPatch("DropClient")]
        public static bool DropClient_Patch(ClientInfo _clientInfo)
        {
            if (_clientInfo != null)
            {
                string[] address = _clientInfo.ip.Split(':');
                if (AdressTracker.ContainsKey($"{address[0]}"))
                {
                    if (Debug) Log.Out($"[DDOS-MOD] Decrease Address {address[0]} for Disconnect! old Count {AdressTracker[$"{address[0]}"]}");
                    AdressTracker[$"{address[0]}"]--;
                    if (Debug) Log.Out($"[DDOS-MOD] Decrease Address {address[0]} for Disconnect! new Count {AdressTracker[$"{address[0]}"]}");
                    if (AdressTracker[$"{address[0]}"] <= 0)
                    {
                        if (Debug) Log.Out($"[DDOS-MOD] Address {address[0]} Count {AdressTracker[$"{address[0]}"]} is below 0 we remove the Entry");
                        AdressTracker.Remove($"{address[0]}");
                    }
                }

            }
            return true;
        }

    }
}
