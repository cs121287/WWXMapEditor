using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WWXMapEditor.ViewModels
{
    #region Helper Classes

    public class MapLayer : ViewModelBase
    {
        private string _number = "";
        private string _name = "";
        private bool _isActive;

        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }

    public class TileData : ViewModelBase
    {
        private string _name = "";
        private string _imageSource = "";
        private System.Windows.Media.Brush _borderBrush = System.Windows.Media.Brushes.Transparent;
        private System.Windows.Media.Brush _background = System.Windows.Media.Brushes.White;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public System.Windows.Media.Brush BorderBrush
        {
            get => _borderBrush;
            set => SetProperty(ref _borderBrush, value);
        }

        public System.Windows.Media.Brush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }
    }

    public class MapTile : ViewModelBase
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _terrainImage = "";
        private double _terrainOpacity = 1.0;
        private double _collisionOpacity = 0.5;
        private Visibility _collisionVisibility = Visibility.Collapsed;
        private string? _propertyImage;
        private double _propertyOpacity = 1.0;
        private Visibility _propertyVisibility = Visibility.Collapsed;
        private string? _unitImage;
        private double _unitOpacity = 1.0;
        private Visibility _unitVisibility = Visibility.Collapsed;
        private Visibility _selectionVisibility = Visibility.Collapsed;

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string TerrainImage
        {
            get => _terrainImage;
            set => SetProperty(ref _terrainImage, value);
        }

        public double TerrainOpacity
        {
            get => _terrainOpacity;
            set => SetProperty(ref _terrainOpacity, value);
        }

        public double CollisionOpacity
        {
            get => _collisionOpacity;
            set => SetProperty(ref _collisionOpacity, value);
        }

        public Visibility CollisionVisibility
        {
            get => _collisionVisibility;
            set => SetProperty(ref _collisionVisibility, value);
        }

        public string? PropertyImage
        {
            get => _propertyImage;
            set => SetProperty(ref _propertyImage, value);
        }

        public double PropertyOpacity
        {
            get => _propertyOpacity;
            set => SetProperty(ref _propertyOpacity, value);
        }

        public Visibility PropertyVisibility
        {
            get => _propertyVisibility;
            set => SetProperty(ref _propertyVisibility, value);
        }

        public string? UnitImage
        {
            get => _unitImage;
            set => SetProperty(ref _unitImage, value);
        }

        public double UnitOpacity
        {
            get => _unitOpacity;
            set => SetProperty(ref _unitOpacity, value);
        }

        public Visibility UnitVisibility
        {
            get => _unitVisibility;
            set => SetProperty(ref _unitVisibility, value);
        }

        public Visibility SelectionVisibility
        {
            get => _selectionVisibility;
            set => SetProperty(ref _selectionVisibility, value);
        }
    }

    public class GridLine
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
    }

    public class MapStatistic
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ValidationResult
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
    }

    // Simple UndoRedoManager implementation
    public class UndoRedoManager
    {
        private readonly Stack<MapState> _undoStack = new Stack<MapState>();
        private readonly Stack<MapState> _redoStack = new Stack<MapState>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void RecordState()
        {
            // TODO: Implement state recording
            // For now, create a placeholder state
            var state = new MapState
            {
                Data = "Current state",
                Timestamp = DateTime.Now
            };
            _undoStack.Push(state);
            _redoStack.Clear(); // Clear redo stack when new action is performed
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var state = _undoStack.Pop();
                // TODO: Implement undo logic to restore the state
                _redoStack.Push(state);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var state = _redoStack.Pop();
                // TODO: Implement redo logic to reapply the state
                _undoStack.Push(state);
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        private class MapState
        {
            public string Data { get; set; } = "";
            public DateTime Timestamp { get; set; }
        }
    }

    #endregion
}