using UnityEngine;

public class BoundaryLimiter : MonoBehaviour
{
    [Header("Boundaries")]
    [SerializeField] float _minX = -20f;
    [SerializeField] float _maxX = 20f;
    [SerializeField] float _minZ = -20f;
    [SerializeField] float _maxZ = 20f;

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
        pos.z = Mathf.Clamp(pos.z, _minZ, _maxZ);
        transform.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((_minX + _maxX) / 2f, 0f, (_minZ + _maxZ) / 2f);
        Vector3 size = new Vector3(_maxX - _minX, 0.1f, _maxZ - _minZ);
        Gizmos.DrawWireCube(center, size);
    }
}
