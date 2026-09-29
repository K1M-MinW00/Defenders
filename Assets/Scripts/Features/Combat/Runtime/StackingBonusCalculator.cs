using UnityEngine;

public static class StackingBonusCalculator
{
    public static float Calculate(int currentStacks, int stacksToMaximum, float maximumBonus)
    {
        if (stacksToMaximum <= 0 || maximumBonus <= 0f)
            return 0f;

        int clampedStacks = Mathf.Clamp(currentStacks, 0, stacksToMaximum);
        return maximumBonus * clampedStacks / stacksToMaximum;
    }
}
