namespace FreelyProgrammableControl.Logic.Common
{
    internal interface IMemory<T>
    {
        int Length { get; }

        T GetValue(int position);
    }
}