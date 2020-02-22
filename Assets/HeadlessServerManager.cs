using System;
using Bolt.Matchmaking;
using Bolt.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bolt.Samples.HeadlessServer
{
    public class HeadlessServerManager : Bolt.GlobalEventListener
    {
        [SerializeField]
        bool HeadlessTesting;

        public string Map = "";
        public string GameType = "";
        public string RoomID = "";

        public override void BoltStartBegin()
        {
            // Register any Protocol Token that are you using
            BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
        }


        void ConnectivityCheck()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("NotReachable");
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                Debug.Log("ReachableViaCarrierDataNetwork");
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                Debug.Log("ReachableViaLocalAreaNetwork");
            }
        }




        public override void BoltStartDone()
        {
            if (BoltNetwork.IsServer)
            {
                Application.targetFrameRate = 60;

                if (IsHeadlessMode() == true)
                    Bolt.ConsoleWriter.Open();


                // Create some room custom properties
                PhotonRoomProperties roomProperties = new PhotonRoomProperties();

                roomProperties.AddRoomProperty("t", GameType); // ex: game type
                roomProperties.AddRoomProperty("m", Map); // ex: map id

                roomProperties.IsOpen = true;
                roomProperties.IsVisible = true;

                // If RoomID was not set, create a random one
                if (RoomID.Length == 0)
                {
                    RoomID = Guid.NewGuid().ToString();
                }

                // Create the Photon Room
                BoltMatchmaking.CreateSession(
                    sessionID: RoomID,
                    token: roomProperties,
                    sceneToLoad: Map
                );

                // BoltNetwork.SetServerInfo(RoomID, roomProperties);
                // BoltNetwork.LoadScene(Map);
            }
        }


        private void Start()
        {
            StartHeadlessServer();
        }

        void Update()
        {



        }

        // Use this for initialization
        void StartHeadlessServer()
        {
            if (IsHeadlessMode() == false && HeadlessTesting == false)
                return;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Internet Not Reachable");
                Invoke("StartHeadlessServer", 1f);
                return;
            }

            // Get custom arguments from command line
            Map = GetArg("-m", "-map") ?? Map;
            GameType = GetArg("-t", "-gameType") ?? GameType; // ex: get game type from command line
            RoomID = GetArg("-r", "-room") ?? RoomID;

            // Validate the requested Level
            var validMap = false;

            foreach (string value in BoltScenes.AllScenes)
            {
                if (SceneManager.GetActiveScene().name != value)
                {
                    if (Map == value)
                    {
                        validMap = true;
                        break;
                    }
                }
            }

            if (!validMap)
            {
                BoltLog.Error("Invalid configuration: please verify level name");
                Application.Quit();
            }

            // Start the Server
            BoltLauncher.StartServer();

        }

        /// <summary>
        /// Utility function to detect if the game instance was started in headless mode.
        /// </summary>
        /// <returns><c>true</c>, if headless mode was ised, <c>false</c> otherwise.</returns>
        public static bool IsHeadlessMode()
        {
            return Environment.CommandLine.Contains("-batchmode") && Environment.CommandLine.Contains("-nographics");
        }

        static string GetArg(params string[] names)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                foreach (var name in names)
                {
                    if (args[i] == name && args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }
    }
}