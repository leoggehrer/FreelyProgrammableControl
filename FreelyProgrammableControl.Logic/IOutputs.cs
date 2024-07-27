namespace FreelyProgrammableControl.Logic
{
    public interface IOutputs
    {
        IOutputDevice this[int index] { get; set; }
        int Length { get; }
        bool GetValue(int position);
    }
}