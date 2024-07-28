namespace FreelyProgrammableControl.Logic
{
    public interface IOutputs : ISubject
    {
        IOutputDevice this[int index] { get; set; }
        int Length { get; }
        bool GetValue(int position);
    }
}