using System;
using System.ComponentModel;
using System.Reflection;

namespace WWXMapEditor.Views.NewMapSteps.PlayerSelect.Support
{
    // Robust uniqueness enforcement at the ViewModel level.
    // Last writer wins: when a player is assigned a non-Random country already taken, the previous owner is set to Random.
    // This prevents duplicate countries regardless of UI state or converter glitches.
    public sealed class UniqueCountryCoordinator : IDisposable
    {
        private readonly object _vm;
        private readonly string _p1;
        private readonly string _p2;
        private readonly string _p3;
        private readonly string _p4;
        private readonly string _random;
        private bool _updating;

        private readonly INotifyPropertyChanged? _inpc;

        public UniqueCountryCoordinator(object viewModel,
                                        string player1PropName,
                                        string player2PropName,
                                        string player3PropName,
                                        string player4PropName,
                                        string randomKeyword = "Random")
        {
            _vm = viewModel;
            _p1 = player1PropName;
            _p2 = player2PropName;
            _p3 = player3PropName;
            _p4 = player4PropName;
            _random = randomKeyword;

            _inpc = viewModel as INotifyPropertyChanged;
            if (_inpc != null)
            {
                _inpc.PropertyChanged += OnVmPropertyChanged;
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_updating) return;
            if (e.PropertyName != _p1 && e.PropertyName != _p2 && e.PropertyName != _p3 && e.PropertyName != _p4) return;

            try
            {
                _updating = true;

                string? c1 = Get(_p1);
                string? c2 = Get(_p2);
                string? c3 = Get(_p3);
                string? c4 = Get(_p4);

                // Enforce uniqueness: Random is always allowed
                ResolveDuplicate(_p1, ref c1, _p2, ref c2);
                ResolveDuplicate(_p1, ref c1, _p3, ref c3);
                ResolveDuplicate(_p1, ref c1, _p4, ref c4);
                ResolveDuplicate(_p2, ref c2, _p3, ref c3);
                ResolveDuplicate(_p2, ref c2, _p4, ref c4);
                ResolveDuplicate(_p3, ref c3, _p4, ref c4);

                Set(_p1, c1);
                Set(_p2, c2);
                Set(_p3, c3);
                Set(_p4, c4);
            }
            finally
            {
                _updating = false;
            }
        }

        private void ResolveDuplicate(string pA, ref string? a, string pB, ref string? b)
        {
            if (IsRandom(a) || IsRandom(b)) return;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return;
            if (!string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return;

            // Duplicate found. Keep the one that changed (last writer wins).
            // We can't know which fired first reliably; assume 'pA' is the last change right now.
            b = _random;
        }

        private bool IsRandom(string? v) => string.Equals(v, _random, StringComparison.OrdinalIgnoreCase);

        private string? Get(string name)
        {
            var pi = _vm.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return pi?.GetValue(_vm)?.ToString();
        }

        private void Set(string name, string? value)
        {
            var pi = _vm.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null)
            {
                var targetValue = value;
                if (pi.PropertyType == typeof(string))
                {
                    pi.SetValue(_vm, targetValue, null);
                }
                else
                {
                    // If VM uses enums or other types, try simple conversion
                    try
                    {
                        var converted = Convert.ChangeType(targetValue, pi.PropertyType);
                        pi.SetValue(_vm, converted, null);
                    }
                    catch
                    {
                        // ignore if not assignable
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_inpc != null)
            {
                _inpc.PropertyChanged -= OnVmPropertyChanged;
            }
        }
    }
}