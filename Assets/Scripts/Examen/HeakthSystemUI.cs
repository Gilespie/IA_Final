using UnityEngine;
using UnityEngine.UI;

public class HeakthSystemUI : MonoBehaviour
{
    [SerializeField] Image _healthImage;
    HealthSystem _healthSystem;

    void Awake()
    {
        _healthSystem = GetComponentInParent<HealthSystem>();
    }

    void OnEnable()
    {
        _healthSystem.OnHealthChanged += OnHealthChanged;
    }

    void OnDisable()
    {
        _healthSystem.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float health)
    {
        _healthImage.fillAmount = health;
    }
}