
namespace FreelyProgrammableControl.Logic
{
    public interface ISubject
    {
        int Count { get; }

        void Attach(EventHandler observer);
        void Detach(EventHandler observer);
    }
}