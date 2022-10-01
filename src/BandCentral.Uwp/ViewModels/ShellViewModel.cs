using CommonHelpers.Common;
using Intense.Presentation;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace BandCentral.Uwp.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly NavigationItemCollection topItems = new NavigationItemCollection();
        private readonly NavigationItemCollection bottomItems = new NavigationItemCollection();
        private NavigationItem selectedTopItem;
        private NavigationItem selectedBottomItem;
        private bool isSplitViewPaneOpen;

        public ShellViewModel()
        {
            this.ToggleSplitViewPaneCommand = new RelayCommand(() => this.IsSplitViewPaneOpen = !this.IsSplitViewPaneOpen);

            // open splitview pane in wide state on first launch
            //this.IsSplitViewPaneOpen = IsWideState();
        }

        public ICommand ToggleSplitViewPaneCommand { get; private set; }

        public bool IsSplitViewPaneOpen
        {
            get => this.isSplitViewPaneOpen;
            set => SetProperty(ref this.isSplitViewPaneOpen, value);
        }

        public NavigationItem SelectedTopItem
        {
            get => this.selectedTopItem;
            set
            {
                if (SetProperty(ref this.selectedTopItem, value) && value != null)
                {
                    OnSelectedItemChanged(true);
                }
            }
        }

        public NavigationItem SelectedBottomItem
        {
            get => this.selectedBottomItem;
            set
            {
                if (SetProperty(ref this.selectedBottomItem, value) && value != null)
                {
                    OnSelectedItemChanged(false);
                }
            }
        }

        public NavigationItem SelectedItem
        {
            get => this.selectedTopItem ?? this.selectedBottomItem;
            set
            {
                this.SelectedTopItem = this.topItems.FirstOrDefault(m => m == value);
                this.SelectedBottomItem = this.bottomItems.FirstOrDefault(m => m == value);
            }
        }

        public Type SelectedPageType
        {
            get => this.SelectedItem?.PageType;
            set
            {
                // select associated menu item
                this.SelectedTopItem = this.topItems.FirstOrDefault(m => m.PageType == value);
                this.SelectedBottomItem = this.bottomItems.FirstOrDefault(m => m.PageType == value);
            }
        }

        public NavigationItemCollection TopItems => this.topItems;

        public NavigationItemCollection BottomItems => this.bottomItems;

        private void OnSelectedItemChanged(bool top)
        {
            if (top)
            {
                this.SelectedBottomItem = null;
            }
            else {
                this.SelectedTopItem = null;
            }
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(SelectedPageType));

            // auto-close split view pane (only when not in widestate)
            if (!IsWideState())
            {
                this.IsSplitViewPaneOpen = false;
            }
        }

        // a helper determining whether we are in a wide window state
        // mvvm purists probably don't appreciate this approach
        private bool IsWideState()
        {
            return Window.Current.Bounds.Width >= 1024;
        }
    }
}
