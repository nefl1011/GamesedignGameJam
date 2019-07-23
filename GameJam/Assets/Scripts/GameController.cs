using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviourPunCallbacks
{
    public static GameController instance;

    [Header("Settings")]
    public int spawnTimer = 5;
    public int dropChance = 20;

    [Header("References")]
    public List<GameObject> supplyDrops;
    public GameObject[] supplyPrefabs;
    public GameObject infectPrefab;

    public int supplyDropsCreated = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    [PunRPC]
    public void RPC_SpawnSupply(int no, int type, float posX, float posZ)
    {
        Vector3 pos = new Vector3(posX, 150, posZ);
        GameObject supplyDrop = Instantiate(supplyPrefabs[type], pos, Quaternion.identity);
        supplyDrop.name = "SupplyDrop_" + no;
        supplyDrop.GetComponent<SupplyDrop>().dropNo = no;
        supplyDrops.Add(supplyDrop);
    }

    [PunRPC]
    public void RPC_DestroySupply(int no)
    {
        for(int i = 0; i < supplyDrops.Count; i++)
        {
            SupplyDrop tempDrop = supplyDrops[i].GetComponent<SupplyDrop>();
            if(tempDrop.dropNo == no)
            {
                supplyDrops.Remove(tempDrop.gameObject);
                Destroy(tempDrop.gameObject);
            }
        }
    }

    [PunRPC]
    public void RPC_Desinfect(float x, float y, float z)
    {
        Vector3 pos = new Vector3(x, y, z);
        GameObject desinfectZone = Instantiate(infectPrefab, pos, Quaternion.identity);
        Destroy(desinfectZone, 5);
    }

    [PunRPC]
    public void RPC_Fight()
    {

    }

    public void Caller_SpawnSupply(int type, float posX, float posZ)
    {
        photonView.RPC("RPC_SpawnSupply", RpcTarget.AllViaServer, supplyDropsCreated, type, posX, posZ);
        supplyDropsCreated++;
    }

    public void Caller_DestroySupply(int no)
    {
        photonView.RPC("RPC_DestroySupply", RpcTarget.AllViaServer, no);
    }

    public void Caller_Desinfect(Vector3 pos)
    {
        photonView.RPC("RPC_Desinfect", RpcTarget.AllViaServer, pos.x, pos.y, pos.z);
    }

    public void Caller_Fight()
    {
        photonView.RPC("RPC_Fight", RpcTarget.AllViaServer);
    }
}
