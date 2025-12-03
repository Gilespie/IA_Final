using UnityEngine;

public class Character : MonoBehaviour, IDamageable
{
    [Header("Position")]
    [SerializeField] Transform _startPoint;

    [Header("Health")]
    [SerializeField] float _maxHealth;
    float _currentHealth;

    InputController _controller;
    CharacterMovement _movement;

    void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _movement.SetStartPos(_startPoint);
        _currentHealth = _maxHealth;
    }

    void Start()
    {
        _controller = new InputController();    
    }

    void Update()
    {
        _controller.UpdateArtificial();
    }

    void FixedUpdate()
    {
       if(_controller.GetInput().sqrMagnitude != 0f)
       {
            _movement.MoveCharacter(_controller.GetInput());
       }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}