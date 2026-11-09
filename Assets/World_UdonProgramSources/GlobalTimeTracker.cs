
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling; 

// [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GlobalTimeTracker : UdonSharpBehaviour
{
    private int[] playerIds = new int[80];
    private float[] playerJoinTimes = new float[80];

    void Start()
    {
        
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        // Find an empty slot in the tracking array and fill in with player deets
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == 0) 
            {
                playerIds[i] = player.playerId;
                playerJoinTimes[i] = Time.realtimeSinceStartup; // Reset to local clock
                
                Debug.Log($"[Time Debug] Tracking started for {player.displayName}.");
                break;
            }
        }
    }

    private VRCPlayerApi[] playersArray;

    public override void Interact() 
    {
        VRCPlayerApi thisPlayer = Networking.LocalPlayer;  
        if (Utilities.IsValid(Networking.InstanceOwner)) 
        {
            thisPlayer = Networking.InstanceOwner;
        } 
        else 
        {
            int playerCount = VRCPlayerApi.GetPlayerCount();
            playersArray = new VRCPlayerApi[playerCount];
            VRCPlayerApi.GetPlayers(playersArray);
            for (int i = 0; i < playerCount; i++)
            {
                thisPlayer = playersArray[i];
                if (thisPlayer.displayName == "leviofliccat" || thisPlayer.displayName == "jocondev" || thisPlayer.displayName == "Lemonss") 
                {
                    Debug.Log("found team member " + thisPlayer.playerId );
                    break;
                }
            } //find the first player called mod name
        }

        // Debug.Log("local player: " + Networking.LocalPlayer.displayName + " local id: " + Networking.LocalPlayer.playerId);
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(_LogTime), thisPlayer.playerId);
    }

    [NetworkCallable]
    public void _LogTime(int targetPlayerId)
    {
        VRCPlayerApi targetPlayer = VRCPlayerApi.GetPlayerById(targetPlayerId);

        if (Utilities.IsValid(targetPlayer))//target player is leviofliccat
        {
            if (targetPlayerId != Networking.LocalPlayer.playerId) return; //only proceed if the local player receiving the networking event is leviofliccat (aka only show logs for leviofliccat)
            
            VRCPlayerApi callingPlayer = NetworkCalling.CallingPlayer;  //the player who sent the network event i.e. who pressed the button

            for (int i = 0; i < playerIds.Length; i++)
            {
                if (playerIds[i] == callingPlayer.playerId)
                {
                    float totalTimeInWorld = Time.realtimeSinceStartup - playerJoinTimes[i];
                    Debug.Log($"[Time Debug] Spawn pet button pressed by {callingPlayer.displayName} id {callingPlayer.playerId} at {totalTimeInWorld} seconds since joining instance.");
                    break;
                }
            }
        }
        else 
        {
            Debug.Log("Log time called but no player");
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        // Clear their slot when they leave
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == player.playerId)
            {
                float totalTimeInWorld = Time.realtimeSinceStartup - playerJoinTimes[i];
                Debug.Log($"[Time Debug] {player.displayName} left instance. Time spent in world: {totalTimeInWorld} seconds.");
                
                playerIds[i] = 0;
                playerJoinTimes[i] = 0f;
                break;
            }
        }
    }
}
