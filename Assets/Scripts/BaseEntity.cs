using UnityEngine;

public abstract class BaseEntity : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected float moveSpeed = 5f;

    protected Rigidbody2D rb;
    protected Vector2 lastFacingDirection = Vector2.right;

    protected virtual void Start()
    {
        InitializeRigidbody();
    }

    protected void InitializeRigidbody()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    protected void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.magnitude > 0)
        {
            lastFacingDirection = direction.normalized;
        }
    }

    // 디버그 유틸리티
    protected void DebugDrawCircle(Vector2 center, float radius, Color color, float duration)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;

            Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(point1, point2, color, duration);
        }
    }
}
