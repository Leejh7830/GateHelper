using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GateHelper.LogValidator.Models
{
    public class UnitTemplateModel : INotifyPropertyChanged
    {
        private string _eventName;
        private string _maskingPattern;

        public string EventName
        {
            get => _eventName;
            set { if (_eventName != value) { _eventName = value; OnPropertyChanged(); } }
        }

        public string MaskingPattern
        {
            get => _maskingPattern;
            set { if (_maskingPattern != value) { _maskingPattern = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ScenarioStepModel : INotifyPropertyChanged
    {
        private int _stepNo;
        private string _eventName;
        private string _maskingPattern;
        private string _direction;
        private double _timeoutSeconds = 0;
        private bool _isOptional = false;
        private int _groupId = 0;
        // 💡 주의: 향후 이 부분도 Constants.GROUP_AND 로 교체하시면 완벽합니다.
        private string _groupType = "AND";

        public int StepNo { get => _stepNo; set { if (_stepNo != value) { _stepNo = value; OnPropertyChanged(); } } }
        public string EventName { get => _eventName; set { if (_eventName != value) { _eventName = value; OnPropertyChanged(); } } }
        public string MaskingPattern { get => _maskingPattern; set { if (_maskingPattern != value) { _maskingPattern = value; OnPropertyChanged(); } } }
        public string Direction { get => _direction; set { if (_direction != value) { _direction = value; OnPropertyChanged(); } } }
        public double TimeoutSeconds { get => _timeoutSeconds; set { if (_timeoutSeconds != value) { _timeoutSeconds = value; OnPropertyChanged(); } } }
        public bool IsOptional { get => _isOptional; set { if (_isOptional != value) { _isOptional = value; OnPropertyChanged(); } } }
        public int GroupId { get => _groupId; set { if (_groupId != value) { _groupId = value; OnPropertyChanged(); } } }
        public string GroupType { get => _groupType; set { if (_groupType != value) { _groupType = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}