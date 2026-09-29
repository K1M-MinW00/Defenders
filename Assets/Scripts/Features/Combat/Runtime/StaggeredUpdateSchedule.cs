public static class StaggeredUpdateSchedule
{
    public static float GetInitialDelay(int stableId, float interval)
    {
        if (interval <= 0f)
            return 0f;

        uint hash = unchecked((uint)stableId * 2654435761u);
        float normalizedOffset = (hash & 0xFFFFu) / 65536f;
        return normalizedOffset * interval;
    }
}
