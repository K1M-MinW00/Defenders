public static class StageEnterHolder
{
    public static StageEnterData Data { get; private set; }

    public static void Set(StageEnterData data)
    {
        Data = data;
    }

    public static StageEnterData Consume()
    {
        StageEnterData data = Data;
        Data = null;
        return data;
    }
}