using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using EscherGroup.EssentialCP.Client.UI.ViewModels;
namespace THP.Extention.Models
{
    [DataContract]
    public class BugHistoryItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        private string _description;

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int No { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ShortDescription");
                }
            }
        }

        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return string.Empty;
                }
                string cleanText = Description.Replace("\r\n", " ").Replace("\n", " ");
                return cleanText.Length > 50 ? cleanText.Substring(0, 50) + "..." : cleanText;
            }
        }

        [DataMember]
        public string ScreenshotPath { get; set; }

        public CommandViewModelBase ViewImageCommand { get; set; }

        // Runtime UI state, not serialized
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }   
}