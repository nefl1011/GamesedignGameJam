using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ARPlayer : MonoBehaviourPunCallbacks
{
    public static ARPlayer instance;

    [Header("Info/Settings")]
    public int currentItem = 0;
    public int[] currentAmmo = { 10, 20 };
    public int[] ammunition = { 80, 80 };
    public int[] maxAmmo = { 120, 120 };
    public int rayLength = 60;

    private Camera mCam;
    
    private RaycastHit hitInfo;


    void Awake()
    {
        if (photonView.IsMine)
        {
            instance = this;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("ControlSupplyDrops",5f,GameController.instance.spawnTimer);
        }
        mCam = Camera.main;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SupplyRay(Input.mousePosition);
        }
    }
    

    public void UseItem()
    {
        TargetRay();
        switch (currentItem)
        {
            case 0:
                Interface_Inventory.instance.Audio_SoundAtCam(3);
                Fight();
                break;
            case 1:
                Interface_Inventory.instance.Audio_SoundAtCam(2);
                Desinfect();
                break;
            default:
                break;
        }
    }

    public void TargetRay()
    {
        Ray ray = mCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Physics.Raycast(ray, out hitInfo, rayLength);
    }

    public void ReceiveAmmo(int itemNo, int amount)
    {
        if (ammunition[itemNo] == 0)
        {
            currentAmmo[itemNo] = 5;
            Interface_Inventory.instance.itemLoaded = true;
        }
        ammunition[itemNo] = ammunition[itemNo] += amount;
        ammunition[itemNo] = Mathf.Clamp(ammunition[itemNo], 0, maxAmmo[itemNo]);

        Interface_Inventory.instance.UpdateAmmo();
    }

    public void SupplyRay(Vector2 pos) {
        Ray ray = mCam.ScreenPointToRay(new Vector3(pos.x, pos.y, 0));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, rayLength))
        {
            if (hitInfo.collider.CompareTag("SupplyDrop"))
            {
                hitInfo.collider.GetComponent<SupplyDrop>().AddAmmo();
            }
        }
    }

    public void Desinfect()
    {
        if(hitInfo.collider != null)
        GameController.instance.Caller_Desinfect(hitInfo.point);
    }

    public void Fight()
    {
        if (hitInfo.collider != null)
        {
            GameController.instance.Caller_SpawnParts(hitInfo.point);
            Debug.Log("spawn parts");
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                Debug.Log("Enemy");
                if (GameController.instance.couldFight)
                {
                    Debug.Log("couldFight");
                    GameController.instance.Caller_Fight();
                }
            }
        }
    }

    public void ControlSupplyDrops()
    {
        int rng = (int)Random.Range(0, 100);
        if(rng < GameController.instance.dropChance)
        {
            //Debug.Log("GenerateDrop");
            int typeRng = Random.Range(0, 2);
            float xRng = Random.Range(-18f, 18f); 
            float zRng = Random.Range(-18f, 18f);
            GameController.instance.Caller_SpawnSupply(typeRng, xRng, zRng);
        }
    }


}
