using System;
using System.Windows.Forms;
using System.Windows.Input;

using EscherGroup.EssentialCP.Client.Core;
using EscherGroup.EssentialCP.Client.UI.Commands;
using EscherGroup.EssentialCP.Client.UI.ViewModels;
using THP.Extention.Models;

namespace THP.Extention.UI
{
    public class ViewImageCommand: CommandViewModelBase
    {
        private readonly BugHistoryItem _item;
        private readonly DataContext _dc;

        private readonly ICommand _command;

        public override ICommand Command
        {
            get
            {
                return _command;
            }
        }

        public ViewImageCommand(BugHistoryItem item, DataContext dc): base("ViewImage")
        {
            _item = item;
            _dc = dc;

            Caption = "View";

            _command =
                new DelegateCommand(
                    ExecuteView);
        }

        private void ExecuteView()
        {
            try
            {
                // Save original typed text and captured screenshot path if not already saved
                if (!_dc.ContainsKey("OriginalBugDescription"))
                {
                    _dc.Set("OriginalBugDescription", _dc.Get<string>("BugDescription") ?? string.Empty);
                }
                if (!_dc.ContainsKey("OriginalScreenshotPath"))
                {
                    _dc.Set("OriginalScreenshotPath", _dc.Get<string>("ScreenshotPath") ?? string.Empty);
                }

                // Load clicked history item description and image
                _dc.Set("BugDescription", _item.Description);
                _dc.Set("SelectedScreenshotPath", _item.ScreenshotPath);

                // Set active editing item ID
                _dc.Set("EditingBugId", _item.Id);

                // Toggle checked status if in delete mode to trigger row highlight
                bool isDeleteMode = false;
                if (_dc.ContainsKey("IsDeleteMode"))
                {
                    isDeleteMode = _dc.Get<bool>("IsDeleteMode");
                }
                        
                if (isDeleteMode)
                {
                    _item.IsChecked = !_item.IsChecked;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public override void OnLanguageChanged()
        {
        }
    }   
}