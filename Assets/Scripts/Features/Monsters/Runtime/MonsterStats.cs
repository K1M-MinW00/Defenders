[System.Serializable]
public class MonsterStats
{
    public float maxHp = 30f;
    public float moveSpeed = 3f;
    public float atkDamage = 10f;
    public float atkRange = 2f;
    public float atkPerSec = 2f;

    public MonsterStats()
    {
    }

    public MonsterStats(MonsterStats source)
    {
        if (source == null)
            return;

        maxHp = source.maxHp;
        moveSpeed = source.moveSpeed;
        atkDamage = source.atkDamage;
        atkRange = source.atkRange;
        atkPerSec = source.atkPerSec;
    }

    public MonsterStats CreateRuntimeCopy()
    {
        return new MonsterStats(this);
    }
}
