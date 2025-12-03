using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    [SerializeField] float _radius = 100f;
    public float Radius => _radius;

    [SerializeField] private BorderType m_LimitMode;
    public BorderType LimitMode => m_LimitMode;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}