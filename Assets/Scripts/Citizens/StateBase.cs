using UnityEngine;
using UnityEngine.AI;

public abstract class StateBase
{
    protected static readonly int State = Animator.StringToHash("State");

    public StateType stateType = StateType.None;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected FsmManager fsmManager;

    public virtual void Initialize(Animator animator, FsmManager fsmManager, NavMeshAgent agent)
    {
        this.animator = animator;
        this.agent = agent;
        this.fsmManager = fsmManager;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}

public class StateIdle : StateBase
{
    public override void Initialize(Animator animator1, FsmManager fsmManager1, NavMeshAgent agent1)
    {
        base.Initialize(animator1, fsmManager1, agent1);
        stateType = StateType.Idle;
    }

    public override void OnEnter()
    {
        animator.SetInteger(State, (int)StateType.Idle);
    }
}

public class StateWalking : StateBase
{
    int saveIndex = -1;

    public override void Initialize(Animator animator1, FsmManager fsmManager1, NavMeshAgent agent)
    {
        base.Initialize(animator1, fsmManager1, agent);
        stateType = StateType.Walking;
    }

    public override void OnEnter()
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        animator.SetInteger(State, (int)StateType.Walking);
        GoToNextPoint();
    }

    public override void OnUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    public override void OnExit()
    {
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }
    }

    void GoToNextPoint()
    {
        if (fsmManager.waypoints.Length == 0) return;

        int randomIndex = Random.Range(0, fsmManager.waypoints.Length);

        if (fsmManager.waypoints.Length > 1)
        {
            while (saveIndex == randomIndex)
            {
                randomIndex = Random.Range(0, fsmManager.waypoints.Length);
            }
        }

        saveIndex = randomIndex;
        agent.SetDestination(fsmManager.waypoints[randomIndex].position);
    }
}

public class StateShoot : StateBase
{
    private float timer;
    private float shootDuration = 1.5f;

    public override void Initialize(Animator animator1, FsmManager fsmManager1, NavMeshAgent agent1)
    {
        base.Initialize(animator1, fsmManager1, agent1);
        stateType = StateType.Shoot;
    }

    public override void OnEnter()
    {
        timer = shootDuration;
        fsmManager.ThrowWrench();
        animator.SetInteger(State, (int)StateType.Shoot);
    }

    public override void OnUpdate()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            fsmManager.SwitchState(StateType.Idle);
        }
    }
}

public class StateClapping : StateBase
{
    public override void Initialize(Animator animator1, FsmManager fsmManager1, NavMeshAgent agent1)
    {
        base.Initialize(animator1, fsmManager1, agent1);
        stateType = StateType.Clapping;
    }

    public override void OnEnter()
    {
        animator.SetInteger(State, (int)StateType.Clapping);
    }
}

public class StateDie : StateBase
{
    public override void Initialize(Animator animator1, FsmManager fsmManager1, NavMeshAgent agent1)
    {
        base.Initialize(animator1, fsmManager1, agent1);
        stateType = StateType.Die;
    }

    public override void OnEnter()
    {
        animator.SetInteger(State, (int)StateType.Die);

        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }
    }
}