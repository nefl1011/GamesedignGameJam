using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int GameVersion;
    [SerializeField]
    private int RoomSize;

    private int RoomNumber = 1;
    private bool Joined = false;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    void Start()
    {
        PhotonNetwork.GameVersion = GameVersion.ToString();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        Joined = true;
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room");
        CreateRoom();
    }

    void CreateRoom()
    {
        Debug.Log("Creating room now");
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)RoomSize };
        PhotonNetwork.CreateRoom("Room" + RoomNumber, roomOps);
        Debug.Log(RoomNumber);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom();
    }

    public void StartGame(int id)
    {
        if (!Joined)
        {
            RoomNumber = id;
            PhotonNetwork.JoinRoom("Room" + RoomNumber);
            Debug.Log("Join room");
        }
    }
}
