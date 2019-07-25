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

    void Start()
    {
        PhotonNetwork.GameVersion = GameVersion.ToString();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }

 }
