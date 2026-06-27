using System.Drawing;

namespace ImageProcessingTool;

public class UndoRedoManager : IDisposable
{
    private readonly Stack<Bitmap> _undoStack = new();
    private readonly Stack<Bitmap> _redoStack = new();
    private readonly int _maxSteps;

    public event EventHandler? StateChanged;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public UndoRedoManager(int maxSteps = 20)
    {
        _maxSteps = maxSteps;
    }

    public void PushState(Bitmap current)
    {
        _undoStack.Push(new Bitmap(current));
        while (_undoStack.Count > _maxSteps)
        {
            _undoStack.TryPop(out var old);
            old?.Dispose();
        }
        DisposeStack(_redoStack);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public Bitmap? Undo(Bitmap current)
    {
        if (!CanUndo) return null;
        _redoStack.Push(new Bitmap(current));
        var prev = _undoStack.Pop();
        StateChanged?.Invoke(this, EventArgs.Empty);
        return prev;
    }

    public Bitmap? Redo(Bitmap current)
    {
        if (!CanRedo) return null;
        _undoStack.Push(new Bitmap(current));
        var next = _redoStack.Pop();
        StateChanged?.Invoke(this, EventArgs.Empty);
        return next;
    }

    public void Clear()
    {
        DisposeStack(_undoStack);
        DisposeStack(_redoStack);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Clear();
    }

    private static void DisposeStack(Stack<Bitmap> stack)
    {
        while (stack.Count > 0)
        {
            stack.Pop().Dispose();
        }
    }
}
