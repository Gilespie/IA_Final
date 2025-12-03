public class AppleFoodFactory : Factory<Apple>
{
    public override Apple Create()
    {
        var x = _pool.GetFromPool();
        x.Initialize(_pool);
        return x;
    }

    protected override Apple CreatePrefab()
    {
        var x = Instantiate(_prefab);
        return x;
    }

    protected override void TurnOff(Apple obj)
    {
        obj.Refresh();
        obj.gameObject.SetActive(false);
    }

    protected override void TurnOn(Apple obj)
    {
        obj.gameObject.SetActive(true);
    }
}