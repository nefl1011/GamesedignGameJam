using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class Interface_Inventory : MonoBehaviourPunCallbacks
{
    public static Interface_Inventory instance;

    [Header("Settings")]
    public float swipeThreshold = 0.3f;
    public float reloadSpeed = 10f;
    public float reloadLeftThreshold = 160;
    public float reloadRightThreshold = 310;

    [Header("References")]
    public Camera mCam;
    public GameObject reload;
    public Image reloadPush;
    public Image ammoImage;
    public Text ammoText;
    public Animator itemMenu;
    public Image curItemImage;
    public Sprite[] itemImages;
    public GameObject splatter;
    public AudioClip[] audioClips;
    public GameObject win;

    public bool itemLoaded = true;
    public bool pumpAllowed;

    private Vector2 inpStart;
    private Vector2 inpStartreload;
    private Vector2 inpEndreload;
    private Vector2 inpEnd;
    private Vector2 inpDirection;
    private float inpMagnitude;

    public void Start()
    {
        instance = this;
        mCam = Camera.main;
        UpdateAmmo();
    }

    public void Update()
    {
        inpStartreload = mCam.ScreenToViewportPoint(Input.mousePosition);

        if (!itemLoaded)
        {
            ReloadItem();
        }
        inpEndreload = mCam.ScreenToViewportPoint(Input.mousePosition);
    }

    public void ChooseItem(int itemNo)
    {
        ARPlayer.instance.currentItem = itemNo;
        curItemImage.sprite = itemImages[itemNo];
        UpdateAmmo();
        OpenCloseItemMenu();
    }

    public void ReloadItem()
    {
        float currentDelta = inpStartreload.x - inpEndreload.x;
        Vector3 pos = reloadPush.transform.localPosition;
        pos.x += currentDelta*reloadSpeed;
        if(pos.x <= reloadLeftThreshold + 20 && pumpAllowed)
        {
            pumpAllowed = false;
            splatter.SetActive(true);
            if (ARPlayer.instance.ammunition[ARPlayer.instance.currentItem] >= 5)
            {
                Audio_SoundAtCam(1);
                ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] += 5;
                ARPlayer.instance.ammunition[ARPlayer.instance.currentItem] -= 5;
                if (ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] == 20)
                {
                    itemLoaded = true;
                    reload.SetActive(false);
                }
                UpdateAmmo();
            }
        }
        if(pos.x >= reloadRightThreshold - 20 && !pumpAllowed)
        {
            splatter.SetActive(false);
            pumpAllowed = true;
        }
        pos.x = Mathf.Clamp(pos.x, reloadLeftThreshold, reloadRightThreshold);
        reloadPush.transform.localPosition = pos;
    }

    public void UpdateAmmo()
    {
        ammoText.text = ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] + " / "+ ARPlayer.instance.ammunition[ARPlayer.instance.currentItem];
        ammoImage.fillAmount = (float)ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] / 20f;
    }

    public void ActionButton()
    {
        if (ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] == 1)
        {
            splatter.SetActive(false);
            itemLoaded = false;
            reload.SetActive(true);
        }
        if(ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] >= 1)
        {
            ARPlayer.instance.UseItem();
            ARPlayer.instance.currentAmmo[ARPlayer.instance.currentItem] -= 1;
        }
        UpdateAmmo();
    }

    public void OpenCloseItemMenu()
    {
        Audio_SoundAtCam(0);
        itemMenu.SetTrigger("Trigger"); //Automatic switch between open/close happens in the animator
    }

    //Unused
    private bool CheckSwipe()
    {
        if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(1) || EventSystem.current.currentSelectedGameObject != null)
            return false;

        if (Input.GetMouseButtonDown(0))
        {
            inpStart = mCam.ScreenToViewportPoint(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            inpEnd = mCam.ScreenToViewportPoint(Input.mousePosition);
            inpDirection = inpEnd - inpStart;
            inpMagnitude = inpDirection.magnitude;
            if (inpMagnitude > swipeThreshold)
            {
                return true;
            }
        }
        return false;
    }

    public void Audio_SoundAtCam(int soundNo)
    {
        AudioSource.PlayClipAtPoint(audioClips[soundNo], mCam.transform.position);
    }

    public void Leave()
    {
        GameController.instance.Caller_PlayerLeft();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void WinScreen()
    {
        win.SetActive(true);
    }
}
