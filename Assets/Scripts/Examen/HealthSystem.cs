using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _lowHealthLimit = 40f;

    float _currentHealth;
    public float CurrentHealth => _currentHealth;

    void Awake()
    {
        _currentHealth = _maxHealth;
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

    public bool IsLowHealth() => _currentHealth <= _lowHealthLimit;
}
