using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FsmManager : MonoBehaviour
{
    [SerializeField] private CitizenDataSO citizenDataSO;
    [SerializeField] private NpcType npcType;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameEventSO enemyShootEvent;
    [SerializeField] private GameEventSO enemyDiedEvent;
    [SerializeField] private GameEventSO civilianDiedEvent;

    public Transform[] waypoints;
    private List<StateBase> states = new List<StateBase>();
    private StateBase currentState;
    private float shootCdAux;
    private HealthSystemV2 healthSystemV2;

    private void Awake()
    {
        healthSystemV2 = GetComponent<HealthSystemV2>();
        healthSystemV2.onDie += HealthSystem_onDie;

        states.Add(new StateIdle());
        states.Add(new StateClapping());
        states.Add(new StateShoot());
        states.Add(new StateWalking());
        states.Add(new StateDie());

        foreach (StateBase state in states)
            state.Initialize(animator, this, agent);

        currentState = FindState(StateType.Idle);
    }

    private void Start()
    {
        if (npcType == NpcType.Civilian)
        {
            SwitchState(StateType.Clapping);
        }
        else if (npcType == NpcType.Enemy)
        {
            shootCdAux = citizenDataSO.EnemyThrowCD;
        }
    }

    private void Update()
    {
        Enemy();
        if (currentState != null)
            currentState.OnUpdate();
    }

    private void OnDestroy()
    {
        healthSystemV2.onDie -= HealthSystem_onDie;
    }

    private void HealthSystem_onDie()
    {
        switch (npcType)
        {
            case NpcType.Enemy:
                enemyDiedEvent.Raise();
                break;
            case NpcType.Civilian:
                civilianDiedEvent.Raise();
                break;
        }
        Destroy(gameObject);
    }

    public void SwitchState(StateType targetState)
    {
        SwitchState(FindState(targetState));
    }

    public void SwitchState(StateBase targetState)
    {
        if (currentState == targetState)
        {
            return;
        }

        currentState.OnExit();
        currentState = targetState;
        currentState.OnEnter();
    }

    public StateBase FindState(StateType stateToFind)
    {
        foreach (StateBase state in states)
            if (state.stateType == stateToFind)
                return state;
        return null;
    }

    public void Enemy()
    {
        if (npcType == NpcType.Enemy && currentState.stateType != StateType.Shoot)
        {
            shootCdAux -= Time.deltaTime;

            if (shootCdAux <= 0)
            {
                SwitchState(StateType.Shoot);
                shootCdAux = citizenDataSO.EnemyThrowCD;
            }
        }
    }

    public void ThrowWrench()
    {
        enemyShootEvent.Raise();
        Vector3 start = shootPoint.position;
        Vector3 forward = transform.forward;

        float distance = 10f;
        float arcHeight = 5f;
        Vector3 target = start + forward * distance;

        Wrench wrench = PoolManager.Instance.Get<Wrench>();

        if (wrench != null)
        {
            wrench.transform.position = start;
            wrench.transform.rotation = Quaternion.identity;
            wrench.Activate();

            Rigidbody rb = wrench.GetComponent<Rigidbody>();
            Vector3 dir = target - start;
            Vector3 dirXZ = new Vector3(dir.x, 0, dir.z);

            Vector3 velocityXZ = dirXZ / citizenDataSO.EnemyProjectileDuration;
            float velocityY = (arcHeight * 2) / citizenDataSO.EnemyProjectileDuration;
            Vector3 finalVelocity = velocityXZ + Vector3.up * velocityY;

            rb.linearVelocity = finalVelocity;
        }
    }
}