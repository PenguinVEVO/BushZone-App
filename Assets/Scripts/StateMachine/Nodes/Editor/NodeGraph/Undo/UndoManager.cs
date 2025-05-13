using System.Collections.Generic;
using System.Linq;


namespace FrameLabs.Utilities.NodeEditor
{
    public class UndoManager
    {
        private Stack<Command> undoStack = new Stack<Command>();
        private Stack<Command> redoStack = new Stack<Command>();

        private const int MaxStackSize = 10; // max undo / redo stack

        public void ExecuteCommand(Command command)
        {
            // Execute the command and push it onto the undo stack
            command.Execute();
            undoStack.Push(command);

            // Clear the redo stack as new action invalidates previous redo history
            redoStack.Clear();

            // Maintain stack size limit for undo stack
            TrimStack(undoStack);
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                Command command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);

                // Maintain stack size limit for redo stack
                TrimStack(redoStack);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                Command command = redoStack.Pop();
                command.Redo();
                undoStack.Push(command);

                // Maintain stack size limit for undo stack
                TrimStack(undoStack);
            }
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        // Method to trim the stack to maintain the maximum size limit
        private void TrimStack(Stack<Command> stack)
        {
            while (stack.Count > MaxStackSize)
            {
                stack = new Stack<Command>(stack.Reverse().Skip(1).Reverse());
            }
        }
    }

}