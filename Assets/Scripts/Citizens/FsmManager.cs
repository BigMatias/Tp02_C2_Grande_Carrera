using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Error: Alta - El TP exige que Enemies y Civilians "deben heredar de la clase base abstracta Vehicle". FsmManager es un MonoBehaviour suelto que mezcla NPC enemy y civilian via un enum (npcType). No hay clase base compartida con Player. Se pierde el criterio POO
// Suggestion: Alta - Lo correcto sería un abstract class NPC : Vehicle y dos subclases EnemyNPC / CivilianNPC con override del comportamiento, en lugar de switchear por enum dentro de un único MonoBehaviour.
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

    // Bug: Alta - GetComponent<HealthSystemV2>() sin null-check; si el prefab del NPC no tiene HealthSystemV2, la siguiente línea reventará con NullReferenceException.
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

    // Warning: Alta - Destroy(gameObject) en lugar de devolverlo a un Pool. El TP exige Object Pool propio "para cualquier objeto que se genere en runtime y en cantidad"
    // Suggestion: Media - Antes de destruir habría que pasar el FSM a StateDie (animación + audio)
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

    // Suggestion: Media - El método se llama "Enemy()" (igual que el tipo enum) y maneja un cooldown de disparo. 
    // Warning: Baja - Esta lógica corre en Update para TODOS los NPC (incluyendo civiles), aunque el if descarta civiles.
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

    // Warning: Alta - distance=10f y arcHeight=5f hardcodeados. 
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

            // Warning: Media - GetComponent<Rigidbody>() ejecutado cada disparo. Wrench podría saber su RB
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