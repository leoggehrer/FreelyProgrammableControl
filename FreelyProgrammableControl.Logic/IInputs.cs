namespace FreelyProgrammableControl.Logic
{
    public interface IInputs : ISubject
    {
        IInputDevice this[int index] { get; set; }
        int Length { get; }
        bool GetValue(int position);
    }
}