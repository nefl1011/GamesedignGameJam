using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bakteriophagen : MonoBehaviourPunCallbacks, Virus
{
    public AudioClip[] audioClips;
    public AudioSource aSource;

    [SerializeField]
    private Vector3 SpawnPosition;
    [SerializeField]
    private int SpawnSeconds;
    [SerializeField]
    private int Life;
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

    private int LifeCurrent;

    [SerializeField]
    private Animator VirusAnimator;
    [SerializeField]
    private Animator HitAnimator;

    private NavMeshAgent NavigationAgent;
    
    private Vector3 Destination;

    private State CurrentState;

    private bool Jump;

    private int Hits;

    private float Timer;

    private GameController Controller;

    private int MushroomCounter;

    private Vector3 ScaleAtSpawn;
        
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

        Controller = GameController.instance;
        LifeCurrent = Life;
        PlaySound(0);
        ScaleAtSpawn = transform.localScale;
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

        if (Vector3.Distance(Destination, transform.position) < 2)
        {
            //Infect
            NavigationAgent.isStopped = true;
            Debug.Log("At Destination");
            photonView.RPC("TriggerInfectForAll", RpcTarget.AllViaServer);
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

    [PunRPC]
    public void TriggerInfectForAll()
    {
        Debug.Log("Triggerforall");
        CurrentState = State.START_INFECTION;
        VirusAnimator.SetTrigger("Infect");
        PlaySound(2);
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
            LifeCurrent++;
            photonView.RPC("RPC_Heal", RpcTarget.AllViaServer, LifeCurrent);
            MushroomCounter++;
        }
        else
        {
            MushroomCounter = 0;
            CancelInvoke("SpawnMushroom");
        }
    }

    [PunRPC]
    public void RPC_Heal(int newLife)
    {
        LifeCurrent = newLife;
        ScaleUp();
        Debug.Log("LifeCurrent");
    }

    public void ScaleUp()
    {
        if (transform.localScale.x < ScaleAtSpawn.x * 1.5f)
        {
            float scale = ScaleAtSpawn.x * 0.01f;
            transform.localScale += new Vector3(scale, scale, scale);
        }
    }

    private void ScaleDown()
    {
        if (transform.localScale.x > ScaleAtSpawn.x * 0.5f)
        {
            float scale = ScaleAtSpawn.x * 0.01f;
            transform.localScale -= new Vector3(scale, scale, scale);
        }
        Debug.Log("scaleDown");
    }

    public void Die()
    {
        Debug.Log("Died");
        PlaySound(1);
        NavigationAgent.isStopped = true;
        MainMenu.KilledVirus_First();
        VirusAnimator.SetTrigger("Death");
        Invoke("GoBackToMainMenu", 3);
    }

    private void GoBackToMainMenu()
    {
        Interface_Inventory.instance.WinScreen();
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
            LifeCurrent -= amount;
            photonView.RPC("UpdateLife", RpcTarget.AllViaServer, LifeCurrent);
        }
    }

    [PunRPC]
    public void UpdateLife(int newLife)
    {
        if (CurrentState == State.INFECT)
        {
            Hits--;
        }

        LifeCurrent = newLife;
        if (LifeCurrent <= 0)
        {
            CurrentState = State.DIED;
            Die();
        }
        else
        {
            HitAnimator.SetTrigger("Trigger");
            ScaleDown();
        }
        PlaySound(3);
    }

    [PunRPC]
    public void CalculateRandomDestination(float x, float y)
    {
        Destination = new Vector3(x, transform.position.y, y);
        
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
            Vector3 newDestination;
            NavMeshPath path = new NavMeshPath();
            do
            {
                newDestination = RandomNavmeshLocation(AreaX);
                NavigationAgent.CalculatePath(newDestination, path);
                Debug.Log(path.status);
            } while (path.status != NavMeshPathStatus.PathComplete);

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
    
    public void PlaySound(int soundNo)
    {
        aSource.clip = audioClips[soundNo];
        aSource.Play();
    }
}
