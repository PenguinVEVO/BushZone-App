namespace FrameLabs.Utilities.NodeEditor
{
    public abstract class Command
    {
        public abstract void Execute();
        public abstract void Undo();
        public virtual void Redo() { }
    }
}