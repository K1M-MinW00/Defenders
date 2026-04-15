using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : MonoBehaviour
{
    private UnitController owner;
    private NavMeshAgent agent;

    public NavMeshAgent Agent => agent;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent == null) return;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    public void Resume()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
    }

    public void EnableMovement(bool active)
    {
        if(agent.enabled != active)
            agent.enabled = active;
    }
}