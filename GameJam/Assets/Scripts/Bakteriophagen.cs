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

    private float LifeCurrent;

    private Animator VirusAnimator;

    private NavMeshAgent NavigationAgent;
    
    private Vector3 Destination;

    private State CurrentState;

    private bool Jump;

    private int Hits;

    private float Timer;
    
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
        //VirusAnimator = GetComponent<Animator>();
        Spawn();
    }

    private void OnMouseDown()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Hit();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (CurrentState == State.MOVE)
            {
                Move();
            }/*
            else if (CurrentState == State.INFECT)
            {
                Infect();
            }*/
        }
    }
    
    public void Move()
    {
        /*
        if (Hits <= 0)
        {
            CallNewDestination();
            Hits = MaxHits;
            return;
        }*/

        if (Vector3.Distance(Destination, transform.position) < transform.position.y)
        {
            //Infect
            //NavigationAgent.isStopped = true;
            CallNewDestination();
            Debug.Log("At Destination");
            //CurrentState = State.INFECT;
        }

        if (Jump)
        {
            //StartCoroutine(ReachedGround());
        }
    }

    public void Infect()
    {
        Timer += Time.deltaTime;
        if (Timer < 5.0f)
        {
            if (Hits <= 0)
            {
                CurrentState = State.MOVE;
            }
            else
            {
                //Debug.Log("Bakteriophage is infecting");
            }
        }
        else
        {
            Timer = 0.0f;
            CallNewDestination();
            CurrentState = State.MOVE;
            NavigationAgent.isStopped = false;
        }
    }

    public void Die()
    {
        Debug.Log("Died");
        NavigationAgent.isStopped = true;
        Invoke("Spawn", SpawnSeconds);
    }
    
    public void Spawn()
    {
        LifeCurrent = Life;
        transform.position = SpawnPosition;
        transform.rotation.Set(0, 0, 0, 0);
        CallNewDestination();
        Hits = MaxHits;
        Timer = 0.0f;
        NavigationAgent.speed = Speed;

        if (CurrentState == State.SPAWN)
        {
            InvokeRepeating("CalculateJump", 0, TimeForCalculatingJump);
        }

        CurrentState = State.MOVE;
    }

    public void Hit()
    {
        if (CurrentState != State.DIED)
        {
            Debug.Log("Hit");
            Hits--;
            LifeCurrent -= Random.Range(1, 5);
            Debug.Log("Current Life: " + LifeCurrent);
            if (LifeCurrent <= 0)
            {
                CurrentState = State.DIED;
                Die();
            }
        }
    }

    [PunRPC]
    public void CalculateRandomDestination(Vector3 NewDestination)
    {
        NavigationAgent.SetDestination(NewDestination);
        Destination = NavigationAgent.destination;
        Debug.Log("Calculate Destination");
        CurrentState = State.MOVE;
    }

    private void CallNewDestination()
    {
        CurrentState = State.CALLED_DEST;
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 NewDestination = new Vector3(Random.Range(-AreaX, AreaX), transform.position.y, Random.Range(-AreaZ, AreaZ));
            this.photonView.RPC("CalculateRandomDestination", RpcTarget.AllViaServer, NewDestination);
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
}
