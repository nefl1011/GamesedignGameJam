using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public int id = 0;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Desinfection")
        {
            GameController.instance.Caller_Destroy_Mushroom(id);
        }
    }
}
