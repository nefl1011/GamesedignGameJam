using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bakteriophagen : MonoBehaviourPunCallbacks, Virus
{
    
    [SerializeField]
    private Vector3 SpawnPosition;
    [SerializeField]
    private int SpawnSeconds;
    [SerializeField]
    private float Life;
    [SerializeField]
    private float AreaX;
    [SerializeField]
    private float AreaZ;
    [SerializeField]
    private float AngularSpeed;
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float TimeForJump;
    [SerializeField]
    private float TimeForCalculatingJump;
    [SerializeField]
    private float SpeedMultiplier;
    [SerializeField]
    private int MaxHits;
    [SerializeField]
    private int JumpPossibility;
    [SerializeField]
    private int TimeForInfection;
    [SerializeField]
    private int MaxMushroomCount;
    [SerializeField]
    private float MushroomSpawnRadius;

    private float LifeCurrent;

    private Animator VirusAnimator;

    private NavMeshAgent NavigationAgent;
    
    private Vector3 Destination;

    private State CurrentState;

    private bool Jump;

    private int Hits;

    private float Timer;

    private GameController Controller;

    private int MushroomCounter;
    
    private enum State
    {
        SPAWN = 0,
        MOVE = 1,
        CALLED_DEST = 2,
        START_INFECTION = 3,
        INFECT = 4,
        DIED = 5
    }
    
    // Start is called before the first frame update
    void Start()
    {
        CurrentState = State.SPAWN;
        NavigationAgent = GetComponent<NavMeshAgent>();
        VirusAnimator = GetComponentInChildren<Animator>();
        //Spawn();
        Controller = GameController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (CurrentState == State.MOVE)
            {
                Move();
            }
            else if (CurrentState == State.START_INFECTION)
            {
                InvokeRepeating("SpawnMushroom", 0, TimeForInfection / MaxMushroomCount);
                CurrentState = State.INFECT;
                VirusAnimator.SetTrigger("Infect");
            }
            else if (CurrentState == State.INFECT)
            {
                Infect();
            }
        }
    }
    
    public void Move()
    {
        if (Hits <= 0)
        {
            CallNewDestination();
            Hits = MaxHits;
            return;
        }
        Debug.Log(Vector3.Distance(Destination, transform.position));
        if (Vector3.Distance(Destination, transform.position) < 2)
        {
            //Infect
            NavigationAgent.isStopped = true;
            Debug.Log("At Destination");
            CurrentState = State.START_INFECTION;
        }

        if (Jump)
        {
            //StartCoroutine(ReachedGround());
        }
    }

    public void Infect()
    {
        if (Timer < TimeForInfection)
        {
            Timer += Time.deltaTime;
            if (Hits <= 0)
            {
                MushroomCounter = MaxMushroomCount;
                CurrentState = State.MOVE;
            }
        }
        else
        {
            Timer = 0.0f;
            CallNewDestination();
        }
    }

    private void SpawnMushroom()
    {
        if (MushroomCounter < MaxMushroomCount)
        {
            float angle = Mathf.PI * 2.0f / MaxMushroomCount;
            float xPos = MushroomSpawnRadius * Mathf.Cos(angle * MushroomCounter);
            float yPos = MushroomSpawnRadius * Mathf.Sin(angle * MushroomCounter);

            Vector3 pos = new Vector3(xPos + transform.position.x, transform.position.y, yPos + transform.position.z);

            Controller.Caller_Infect(pos);

            MushroomCounter++;
        }
        else
        {
            MushroomCounter = 0;
            CancelInvoke("SpawnMushroom");
        }
    }

    public void Die()
    {
        Debug.Log("Died");
        NavigationAgent.isStopped = true;
        MainMenu.KilledVirus_First();
        VirusAnimator.SetTrigger("Death");
        Invoke("GoBackToMainMenu", 5);
    }

    private void GoBackToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        Interface_Inventory.instance.Leave();
    }
    
    public void Spawn()
    {
        Invoke("SpawnAfterSeconds", 2);
    }

    public void SpawnAfterSeconds()
    {
        LifeCurrent = Life;
        transform.position = SpawnPosition;
        transform.rotation.Set(0, 0, 0, 0);
        CallNewDestination();
        Hits = MaxHits;
        Timer = 0.0f;
        NavigationAgent.speed = Speed;
        MushroomCounter = 0;

        if (CurrentState == State.SPAWN)
        {
            InvokeRepeating("CalculateJump", 0, TimeForCalculatingJump);
        }

        CurrentState = State.MOVE;
    }

    public void Hit(int amount)
    {
        if (CurrentState != State.DIED)
        {
            Debug.Log("Hit");
            Hits--;
            LifeCurrent -= amount;
            Debug.Log("Current Life: " + LifeCurrent);
            if (LifeCurrent <= 0)
            {
                CurrentState = State.DIED;
                Die();
            }
        }
    }

    [PunRPC]
    public void CalculateRandomDestination(float x, float y)
    {
        Destination = new Vector3(x, transform.position.y, y);

        NavMeshPath path = new NavMeshPath();
        Debug.Log("Path: " + NavigationAgent.CalculatePath(Destination, path));
        Debug.Log(path.status);

        NavigationAgent.SetDestination(Destination);
        Debug.Log("Calculate Destination");
        NavigationAgent.isStopped = false;
        CurrentState = State.MOVE;
    }

    private void CallNewDestination()
    {
        CurrentState = State.CALLED_DEST;
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 newDestination = RandomNavmeshLocation(AreaX);
            this.photonView.RPC("CalculateRandomDestination", RpcTarget.AllViaServer, newDestination.x, newDestination.z);//Random.Range(-AreaX, AreaX), Random.Range(-AreaZ, AreaZ));
        }
    }

    private void CalculateJump()
    {
        Jump = Random.Range(0, 100) < JumpPossibility;
    }

    private IEnumerator ReachedGround()
    {
        NavigationAgent.speed = Speed * SpeedMultiplier;
        NavigationAgent.angularSpeed = AngularSpeed * SpeedMultiplier;
        yield return new WaitForSeconds(TimeForJump);
        NavigationAgent.speed = Speed;
        NavigationAgent.angularSpeed = AngularSpeed;
    }

    private Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;

        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
}
