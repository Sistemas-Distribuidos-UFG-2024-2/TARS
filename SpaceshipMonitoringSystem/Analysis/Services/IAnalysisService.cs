namespace Analysis.Services;

public interface IAnalysisService
{
    bool IsValueNormal(IComparable value, IComparable bottomValue, IComparable topValue);
}