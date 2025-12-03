using System;
using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
    public static event Action<bool> OnTimeChanged;

    [SerializeField] float _timeInSeconds = 1440f;
    [SerializeField, Range(0.1f, 10)] float _timeScale = 3f;
    bool _isDay = false;
    bool _isEvening = false;
    bool _isMorning = false;
    bool _isNight = false;
    float _timer = 0;
    float _percentTime;

    void Update()
    {
        _timer += Time.deltaTime * _timeScale;

        if (_timer >= _timeInSeconds)
        {
            _timer = 0;
        }
        
        _percentTime = _timer / _timeInSeconds;

        ChangeTime();
    }

    void ChangeTime()
    {
        if(_percentTime > 0f && _percentTime < 0.25f)
        {
            _isNight = false;
            _isMorning = true;
            OnTimeChanged?.Invoke(_isMorning);
            //Debug.Log($"Morning{_isMorning}");
        }
        else if(_percentTime > 0.25f && _percentTime < 0.5f)
        {
            _isMorning = false;
            _isDay = true;
            OnTimeChanged?.Invoke(_isDay);
            //Debug.Log($"Day{_isDay}");
        }
        else if(_percentTime > 0.5f && _percentTime < 0.75f)
        {
            _isDay= false;
            _isEvening = true;
            OnTimeChanged?.Invoke(_isEvening);
            //Debug.Log($"Evening{_isEvening}");
        }
        else if(_percentTime > 0.75f)
        {
            _isEvening= false;
            _isNight= true;
            OnTimeChanged?.Invoke(_isNight);
            //Debug.Log($"Night{_isNight}");
        }    
    }
}