namespace Analysis.Services;

public class AnalysisService : IAnalysisService
{
    public bool IsValueNormal(IComparable value, IComparable bottomValue, IComparable topValue)
    {
        if (value.CompareTo(bottomValue) < 0)
        {
            return false;
        }

        if (value.CompareTo(topValue) > 0)
        {
            return false;
        }

        return true;
    }
}