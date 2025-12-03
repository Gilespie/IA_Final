using UnityEngine;

public class Needs : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] float _maxHealth = 100f;
    public float CurrentHealth => _currentHealth;

    [Header("Hungry")]
    [SerializeField] float _maxHungry = 100f;
    [SerializeField] float _hungryThreshold = 50f;
    [SerializeField] float _speedHungry = 1.0f;

    float _currentHungry;
    float _currentHealth;

    void Awake()
    {
        _currentHungry = _maxHungry;
        _currentHealth = _maxHealth;
    }

    void Update()
    {
        _currentHungry -= Time.deltaTime * _speedHungry;

        _currentHungry = Mathf.Clamp(_currentHungry, 0, _maxHungry);
    }

    public bool IsHungry() => _currentHungry < _hungryThreshold;

    public void Eat(float value)
    {
        if (value < 0.0f) return;

        _currentHungry += value;
    }

    public void TakeDamage(float damage)
    {
        if(damage < 0.0f) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if(_currentHealth <= 0f)
        {
            FlockingManager.Instance.RemoveBoid(GetComponentInParent<Boid>());
            Destroy(this.gameObject);
        }
    }
}