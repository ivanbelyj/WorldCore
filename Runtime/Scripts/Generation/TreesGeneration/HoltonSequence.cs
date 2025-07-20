using System;

public class HaltonSequence
{
    private int seqBase;
    private int index;

    public HaltonSequence(int seqBase)
    {
        index = 0;
        this.seqBase = seqBase;
    }

    public double Next()
    {
        index++;
        double result = 0;
        double f = 1.0 / seqBase;
        int i = index;

        while (i > 0)
        {
            result += f * (i % seqBase);
            i = (int)Math.Floor(i / (double)seqBase);
            f /= seqBase;
        }

        return result;
    }
}
