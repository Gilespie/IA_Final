using UnityEngine;

public class HunterEnergy : MonoBehaviour
{
    [SerializeField] float _maxEnergy = 100f;
    [SerializeField] float _restEnergyModifier = 5f;
    [SerializeField] float _consumeEnergyModifier = 2f;
    float _currentEnergy;

    private void Awake()
    {
        _currentEnergy = _maxEnergy;
    }

    public bool AddEnergy(float value)
    {
        _currentEnergy += Time.deltaTime * _restEnergyModifier;
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy);

        if (_currentEnergy >= _maxEnergy)
        {
            return true;
        }

        return false;
    }

    public bool ConsumeEnergy(float fatigue)
    {
        _currentEnergy -= fatigue * _consumeEnergyModifier;
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy);

        if (_currentEnergy <= 0)
        {
            return true;
        }
        
        return false;
    }
}
