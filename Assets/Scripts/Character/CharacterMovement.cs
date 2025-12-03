using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] float _maxSpeed = 5f;
    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void MoveCharacter(Vector3 dir)
    {
        _rb.MovePosition(_rb.position + dir.normalized * _maxSpeed * Time.fixedDeltaTime);
    }

    public void SetStartPos(Transform startPos)
    {
        _rb.position = startPos.position;
    }
}