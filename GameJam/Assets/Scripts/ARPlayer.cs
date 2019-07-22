using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPlayer : MonoBehaviour
{
    public static ARPlayer instance;

    [Header("Info/Settings")]
    public int currentItem = 0;
    public int[] currentAmmo = { 10, 20 };
    public int[] ammunition = { 80, 80 };
    public int[] maxAmmo = { 120, 120 };

    void Start()
    {
        instance = this;
    }


    void Update()
    {
        
    }

    public void UseItem()
    {

    }


}
