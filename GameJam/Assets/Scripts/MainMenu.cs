using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LWRP;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [Header("Info")]
    public static int virusKilled_First;

    [Header("References")]
    public Animator openInfo;
    public Animator openSecondInfo;
    public Animator openThirdInfo;
    public Animator openSettings;
    public LightweightRenderPipelineAsset pipelineAsset;
    public AudioClip[] audioClips;
    public Transform mCam;

    public GameObject[] virusPrefab;
    public GameObject canvas;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mCam = Camera.main.transform;
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
        GameObject tempObj;
        Vector2 rngPos;
        float rngSize;

        for (int i=0; i < virusKilled_First; i++)
        {
            tempObj = Instantiate(virusPrefab[0], Vector3.zero, Quaternion.identity);
            tempObj.transform.parent = canvas.transform;
            tempObj.transform.SetSiblingIndex(1);
            rngSize = Random.Range(0.6f, 1f);
            tempObj.transform.localScale = new Vector3(rngSize,rngSize,rngSize);
            rngPos = new Vector2(Random.Range(0f,300), Random.Range(-200, 0));
            tempObj.transform.localPosition = rngPos;
        }

        //Spawn other in the prototype not killable viruses
        tempObj = Instantiate(virusPrefab[1], Vector3.zero, Quaternion.identity);
        tempObj.transform.parent = canvas.transform;
        tempObj.transform.SetSiblingIndex(1);
        rngSize = Random.Range(0.6f, 1f);
        tempObj.transform.localScale = new Vector3(rngSize, rngSize, rngSize);
        rngPos = new Vector2(Random.Range(0f, 300), Random.Range(-200, 0));
        tempObj.transform.localPosition = rngPos;

        tempObj = Instantiate(virusPrefab[2], Vector3.zero, Quaternion.identity);
        tempObj.transform.parent = canvas.transform;
        tempObj.transform.SetSiblingIndex(1);
        rngSize = Random.Range(0.6f, 1f);
        tempObj.transform.localScale = new Vector3(rngSize, rngSize, rngSize);
        rngPos = new Vector2(Random.Range(0f, 300), Random.Range(-200, 0));
        tempObj.transform.localPosition = rngPos;
    }

    public void OpenInfo_First()
    {
        Audio_SoundAtCam(2);
        Audio_SoundAtCam(0);
        openInfo.SetTrigger("Trigger");
    }

    public void OpenInfo_Second()
    {
        Audio_SoundAtCam(1);
        Audio_SoundAtCam(0);
        openSecondInfo.SetTrigger("Trigger");
    }

    public void OpenInfo_Third()
    {
        Audio_SoundAtCam(3);
        Audio_SoundAtCam(0);
        openThirdInfo.SetTrigger("Trigger");
    }

    public void OpenSettings()
    {
        openSettings.SetTrigger("Trigger");
        Audio_SoundAtCam(0);
    }

    public void LowSpecMode()
    {
        Audio_SoundAtCam(0);
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        pipelineAsset.shadowCascadeOption = 0;
        pipelineAsset.shadowDistance = 0;
        pipelineAsset.renderScale = 0.5f;
        OpenSettings();
    }

    public void ExitApp()
    {
        Audio_SoundAtCam(0);
        Application.Quit();
    }

    public void Audio_SoundAtCam(int soundNo)
    {
        AudioSource.PlayClipAtPoint(audioClips[soundNo], mCam.position);
    }
}
