using System.Collections.Generic;
using WwXMapEditor.Models;

namespace WwXMapEditor
{
    public class UndoRedoManager
    {
        private readonly Stack<Map> _undoStack = new();
        private readonly Stack<Map> _redoStack = new();
        private Map _current;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Reset(Map map)
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _current = DeepCopy(map);
        }

        public Map Undo()
        {
            if (!CanUndo) return _current;
            _redoStack.Push(DeepCopy(_current));
            _current = _undoStack.Pop();
            return DeepCopy(_current);
        }

        public Map Redo()
        {
            if (!CanRedo) return _current;
            _undoStack.Push(DeepCopy(_current));
            _current = _redoStack.Pop();
            return DeepCopy(_current);
        }

        public void Push(Map map)
        {
            _undoStack.Push(DeepCopy(_current));
            _current = DeepCopy(map);
            _redoStack.Clear();
        }

        private Map DeepCopy(Map map)
        {
            // Use serialization for deep copy
            var json = System.Text.Json.JsonSerializer.Serialize(map);
            return System.Text.Json.JsonSerializer.Deserialize<Map>(json);
        }
    }
}