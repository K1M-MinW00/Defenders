using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : MonoBehaviour
{
    [Header("Path Refresh")]
    [SerializeField, Min(0.02f)] private float pathRefreshInterval = 0.15f;
    [SerializeField, Min(0f)] private float destinationChangeThreshold = 0.1f;

    private UnitController owner;
    private NavMeshAgent agent;
    private Vector3 lastDestination;
    private float nextPathRefreshTime;
    private bool hasDestination;

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
        ResetPathRefresh();
    }

    public void MoveTo(Vector3 destination)
    {
        if (!TryActivateOnNavMesh())
            return;

        agent.isStopped = false;

        if (hasDestination && Time.time < nextPathRefreshTime)
            return;

        nextPathRefreshTime = Time.time + pathRefreshInterval;

        float thresholdSqr = destinationChangeThreshold * destinationChangeThreshold;
        if (hasDestination && (destination - lastDestination).sqrMagnitude <= thresholdSqr)
            return;

        if (!agent.SetDestination(destination))
            return;

        lastDestination = destination;
        hasDestination = true;
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

        ResetPathRefresh();
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

    private void ResetPathRefresh()
    {
        hasDestination = false;
        lastDestination = Vector3.zero;
        nextPathRefreshTime = float.NegativeInfinity;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        pathRefreshInterval = Mathf.Max(0.02f, pathRefreshInterval);
        destinationChangeThreshold = Mathf.Max(0f, destinationChangeThreshold);
    }
#endif
}
