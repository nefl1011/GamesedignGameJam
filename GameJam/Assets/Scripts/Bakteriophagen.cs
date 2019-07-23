using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bakteriophagen : MonoBehaviour, Virus
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
        START_INFECTION = 2,
        INFECT = 3,
        DIED = 4
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
        Hit();
    }
    // Update is called once per frame
    void Update()
    {
        if (CurrentState == State.MOVE)
        {
            Move();
        }
        else if (CurrentState == State.INFECT)
        {
            Infect();
        }
    }
    
    public void Move()
    {
        if (Hits <= 0)
        {
            CalculateRandomDestination();
            Hits = MaxHits;
            return;
        }

        if (Vector3.Distance(Destination, transform.position) < transform.position.y)
        {
            //Infect
            Debug.Log("At Destination");
            CurrentState = State.INFECT;
        }

        if (Jump)
        {
            StartCoroutine(ReachedGround());
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
            CalculateRandomDestination();
            CurrentState = State.MOVE;
        }
    }

    public void Die()
    {
        Debug.Log("Died");
        NavigationAgent.SetDestination(transform.position);
        Invoke("Spawn", SpawnSeconds);
    }
    
    public void Spawn()
    {
        LifeCurrent = Life;
        transform.position = SpawnPosition;
        transform.rotation.Set(0, 0, 0, 0);
        CalculateRandomDestination();
        Debug.Log("Destination: " + Destination);
        Debug.Log("agent dest: " + NavigationAgent.destination);
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

    private void CalculateRandomDestination()
    {
        NavigationAgent.SetDestination(new Vector3(Random.Range(-AreaX, AreaX), transform.position.y, Random.Range(-AreaZ, AreaZ)));
        Destination = NavigationAgent.destination;
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
