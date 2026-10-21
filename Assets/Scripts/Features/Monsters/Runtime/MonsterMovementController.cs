using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class MonsterMovementController : MonoBehaviour
{
    private MonsterController owner;
    private NavMeshAgent agent;
    private Coroutine knockbackRoutine;

    public NavMeshAgent Agent => agent;
    public bool IsControlLocked { get; private set; }

    public void Initialize(MonsterController owner)
    {
        this.owner = owner;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void ApplyMoveSpeed(float speed)
    {
        if (agent != null)
            agent.speed = Mathf.Max(0f, speed);
    }

    public void MoveTo(Vector3 destination)
    {
        if (!TryResume())
            return;

        agent.SetDestination(destination);
    }

    public bool TryResume()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        agent.isStopped = false;
        return true;
    }

    public void Stop()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    public void ApplyKnockback(Vector2 direction, float distance, float duration)
    {
        if (owner == null || owner.IsDead || direction.sqrMagnitude <= 0.0001f || distance <= 0f)
            return;

        StopKnockback();
        IsControlLocked = true;
        owner.PrepareForControlEffect();
        knockbackRoutine = StartCoroutine(KnockbackRoutine(
            direction,
            distance,
            Mathf.Max(0.01f, duration)));
    }

    public void ResetRuntime()
    {
        StopKnockback();
        IsControlLocked = false;
        Stop();
    }

    public void Suspend()
    {
        StopKnockback();
        IsControlLocked = true;
        Stop();
    }

    private void StopKnockback()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        IsControlLocked = false;
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float distance, float duration)
    {
        Vector3 start = transform.position;
        Vector3 desiredEnd = start + (Vector3)(direction.normalized * distance);
        Vector3 end = ResolveKnockbackDestination(start, desiredEnd, direction.normalized);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        transform.position = end;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
            agent.ResetPath();
            agent.isStopped = false;
        }

        knockbackRoutine = null;
        IsControlLocked = false;
        owner?.ResumeBehaviorAfterControlEffect();
    }

    private Vector3 ResolveKnockbackDestination(Vector3 start, Vector3 desiredEnd, Vector2 direction)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return start;

        Vector3 end = desiredEnd;
        if (NavMesh.Raycast(start, desiredEnd, out NavMeshHit boundaryHit, agent.areaMask))
        {
            float inset = Mathf.Max(0.05f, agent.radius * 0.25f);
            end = boundaryHit.position - (Vector3)(direction * inset);
        }

        float sampleRadius = Mathf.Max(0.25f, agent.radius * 2f);
        return NavMesh.SamplePosition(end, out NavMeshHit sampled, sampleRadius, agent.areaMask)
            ? sampled.position
            : start;
    }
}
