namespace FreelyProgrammableControl.Logic
{
    public interface ICounters : ISubject
    {
        int Length { get; }

        int GetValue(int position);
    }
}