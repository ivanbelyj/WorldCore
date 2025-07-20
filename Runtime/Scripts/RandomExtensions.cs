using System.Collections;
using System.Collections.Generic;
using System;

public static class RandomExtensions
{
    public static float Range(this Random random, float minInclusive, float maxExclusive)
    {
        double range = (double)maxExclusive - (double)minInclusive;
        double sample = random.NextDouble() * range + (double)minInclusive;
        return (float)sample;
    }
}

