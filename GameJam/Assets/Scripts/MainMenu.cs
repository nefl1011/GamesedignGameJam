using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [Header("Info")]
    public static int virusKilled_First;

    [Header("References")]
    public Animator openInfo;
    public GameObject virusPrefab;
    public GameObject canvas;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadState();
        SpawnViruses();
    }


    void Update()
    {
        
    }

    public static void SaveState()
    {
        PlayerPrefs.SetInt("virusKilled_First", virusKilled_First);
    }

    public static void LoadState()
    {
        if (PlayerPrefs.HasKey("virusKilled_First")){
            virusKilled_First = PlayerPrefs.GetInt("virusKilled_First");
        }
        else
        {
            virusKilled_First = 1;
        }
    }

    public static void KilledVirus_First()
    {
        virusKilled_First++;
        SaveState();
    }

    public void SpawnViruses()
    {
        for(int i=0; i < virusKilled_First; i++)
        {
            GameObject tempObj = Instantiate(virusPrefab, Vector3.zero, Quaternion.identity);
            tempObj.transform.parent = canvas.transform;
            tempObj.transform.SetSiblingIndex(1);
            tempObj.transform.localScale = Vector3.one;
            Vector2 rngPos = new Vector2(Random.Range(0f,300), Random.Range(-200, 0));
            tempObj.transform.localPosition = rngPos;
        }
    }

    public void OpenInfo_First()
    {
        openInfo.SetTrigger("Trigger");
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
