using UnityEngine;

public class SupplyDrop : MonoBehaviour
{
    [Header("Settings")]
    public int itemNo = 0;
    public int amount = 30;
    public int dropNo = 0;

    public void AddAmmo()
    {
        ARPlayer.instance.ReceiveAmmo(itemNo, amount);
        GameController.instance.Caller_DestroySupply(dropNo);
        enabled = false;
    }
}
