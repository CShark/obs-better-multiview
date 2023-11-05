using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BetterMultiview.Controls;

namespace BetterMultiview.Data.Presets
{
    public class PresetSlot : INotifyPropertyChanged
    {
        private PresetBase? preset;
        private bool _isOnAir;
        private PresetButtonState _buttonState;

        public bool HasPreset => preset != null;

        public int SlotId { get; set; }

        public bool IsOnAir
        {
            get => _isOnAir;
            set
            {
                if (value == _isOnAir) return;
                _isOnAir = value;
                OnPropertyChanged();
            }
        }

        public PresetButtonState ButtonState
        {
            get => _buttonState;
            set
            {
                if (value == _buttonState) return;
                _buttonState = value;
                OnPropertyChanged();
            }
        }

        public PresetBase? Preset
        {
            get => preset;
            set
            {
                preset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPreset));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PresetSlot(int slotId)
        {
            SlotId = slotId;
        }
    }
}