using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : MonoBehaviour
{
    private UnitController owner;
    private NavMeshAgent agent;

    public NavMeshAgent Agent => agent;

    public Vector3 MoveDirection
    {
        get
        {
            Vector3 velocity = agent.velocity;

            if(velocity.sqrMagnitude > 0.001f)
                return velocity.normalized;

            return Vector3.zero;
        }
    }

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!TryActivateOnNavMesh())
            return;

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent == null || !agent.enabled)
            return;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        agent.enabled = false;
    }

    public void Resume()
    {
        if (!TryActivateOnNavMesh())
            return;

        agent.isStopped = false;
    }

    public void EnableMovement(bool active)
    {
        if (agent != null && agent.enabled != active)
            agent.enabled = active;
    }

    private bool TryActivateOnNavMesh()
    {
        if (agent == null)
            return false;

        if (!agent.enabled)
            agent.enabled = true;

        return agent.isOnNavMesh;
    }
}
