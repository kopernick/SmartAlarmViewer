namespace Alarm4Rest_Viewer.Services
{
    public class Item : PropertyChangeEventBase
    {
        public string Name { get; private set; }
        public string FieldName { get; private set; }
        public string Value { get; private set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(ref _isChecked, value); }
        }

        public Item(string value)
        {
            Value = value;
        }
        public Item(string value, string fieldName)
        {
            Value = value;
            FieldName = fieldName;
        }
        public Item(string name, string value, string fieldName)
        {
            Name = name;
            Value = value;
            FieldName = fieldName;
        }

        public Item(bool isChecked, string value, string fieldName)
        {
            IsChecked = isChecked;
            Value = value;
            FieldName = fieldName;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
