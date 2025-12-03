using UnityEngine;

public class Apple : MonoBehaviour
{
    [SerializeField] float _foodSatiety = 25f;
    public float FoodSatiety => _foodSatiety;

    private ObjectPool<Apple> _myPool;

    public void Initialize(ObjectPool<Apple> pool)
    {
        _myPool = pool;
    }

    public void Refresh()
    {
        transform.position = Vector3.zero;
    }

 /*   private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Needs needs))
        {
            needs.Eat(_foodSatiety);
            _myPool.ReturnToPool(this);
        }
    }*/

    public void ConsumeFood(Needs needs)
    {
        needs.Eat(_foodSatiety);
        _myPool.ReturnToPool(this);
    }
}