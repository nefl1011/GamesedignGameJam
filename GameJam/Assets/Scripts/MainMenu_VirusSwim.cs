using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_VirusSwim : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 target;
    public float speed = 20;
    public int newDestTime = 10;

    private float move;

    void Start()
    {
        int rngDestTime = Random.Range(4, newDestTime);
        newDestTime = rngDestTime;
        FindDestination();
    }

    void LateUpdate()
    {
     
        move = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, move);
    }

    public void FindDestination()
    {
        Vector3 relative = transform.InverseTransformPoint(target);
        float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);

        float rngX = Random.Range(0f, Screen.width);
        float rngY = Random.Range(0f, Screen.height);
        target = new Vector2(rngX, rngY);

        Invoke("FindDestination", newDestTime);
    }

    public void OpenInfo(int type = 0)
    {
        switch (type)
        {
            case 0:
                MainMenu.instance.OpenInfo_First();
                break;
            case 1:
                MainMenu.instance.OpenInfo_Second();
                break;
            case 2:
                MainMenu.instance.OpenInfo_Third();
                break;
            default:
                MainMenu.instance.OpenInfo_First();
                break;
        }
    }
}
