using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public interface IUndoRedoService
    {
        bool CanUndo { get; }
        bool CanRedo { get; }
        int UndoStackSize { get; }
        int RedoStackSize { get; }
        string UndoDescription { get; }
        string RedoDescription { get; }
        
        void RecordState(Map map, string description);
        Map? Undo(Map currentMap);
        Map? Redo(Map currentMap);
        void Clear();
        void SetLimit(int limit);
    }

    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<MapState> _undoStack = new Stack<MapState>();
        private readonly Stack<MapState> _redoStack = new Stack<MapState>();
        private int _limit = 50;
        private readonly JsonSerializerOptions _serializerOptions;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoStackSize => _undoStack.Count;
        public int RedoStackSize => _redoStack.Count;
        public string UndoDescription => CanUndo ? _undoStack.Peek().Description : string.Empty;
        public string RedoDescription => CanRedo ? _redoStack.Peek().Description : string.Empty;

        public UndoRedoService()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public void RecordState(Map map, string description)
        {
            if (map == null) return;

            try
            {
                // Serialize the current map state
                var json = JsonSerializer.Serialize(map, _serializerOptions);
                
                var state = new MapState
                {
                    Data = json,
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                _undoStack.Push(state);
                _redoStack.Clear(); // Clear redo stack when new action is performed

                // Maintain stack size limit
                while (_undoStack.Count > _limit)
                {
                    // Remove oldest items
                    var items = _undoStack.ToArray();
                    _undoStack.Clear();
                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        _undoStack.Push(items[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to record state: {ex.Message}");
            }
        }

        public Map? Undo(Map currentMap)
        {
            if (!CanUndo) return null;

            try
            {
                // Save current state to redo stack
                if (currentMap != null)
                {
                    var currentJson = JsonSerializer.Serialize(currentMap, _serializerOptions);
                    _redoStack.Push(new MapState
                    {
                        Data = currentJson,
                        Description = "Before undo",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Get previous state
                var previousState = _undoStack.Pop();
                var map = JsonSerializer.Deserialize<Map>(previousState.Data, _serializerOptions);
                
                return map;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to undo: {ex.Message}");
                return null;
            }
        }

        public Map? Redo(Map currentMap)
        {
            if (!CanRedo) return null;

            try
            {
                // Save current state to undo stack
                if (currentMap != null)
                {
                    var currentJson = JsonSerializer.Serialize(currentMap, _serializerOptions);
                    _undoStack.Push(new MapState
                    {
                        Data = currentJson,
                        Description = "Before redo",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Get next state
                var nextState = _redoStack.Pop();
                var map = JsonSerializer.Deserialize<Map>(nextState.Data, _serializerOptions);
                
                return map;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to redo: {ex.Message}");
                return null;
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public void SetLimit(int limit)
        {
            _limit = Math.Max(1, Math.Min(1000, limit)); // Limit between 1 and 1000
        }

        private class MapState
        {
            public string Data { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}