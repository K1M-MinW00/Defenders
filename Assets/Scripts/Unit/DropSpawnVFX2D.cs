using System.Collections;
using UnityEngine;

public class DropSpawnVFX2D : MonoBehaviour
{
    [SerializeField] private Transform body;        // Body 오브젝트
    [SerializeField] private float height = 1.5f;   // 떨어지는 높이(월드 유닛이 아니라 local Y)
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 bodyBaseLocalPos;

    private void Awake()
    {
        if (body == null) body = transform;
        bodyBaseLocalPos = body.localPosition;
    }

    private void OnEnable()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        Vector3 startPos = bodyBaseLocalPos + Vector3.up * height;
        Vector3 endPos = bodyBaseLocalPos;

        Vector3 startScale = Vector3.one * 1.05f;
        Vector3 endScale = Vector3.one;

        body.localPosition = startPos;
        body.localScale = startScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float e = ease.Evaluate(u);

            body.localPosition = Vector3.LerpUnclamped(startPos, endPos, e);
            body.localScale = Vector3.LerpUnclamped(startScale, endScale, e);

            yield return null;
        }

        body.localPosition = endPos;
        body.localScale = endScale;

        // TODO: 착지 파티클/사운드 트리거
    }
}