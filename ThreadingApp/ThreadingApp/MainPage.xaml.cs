using ReactiveUI;
using ReactiveUI.XamForms;

namespace ThreadingApp
{
    public partial class MainPage : ReactiveContentPage<MainPageViewModel>
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainPageViewModel();

            this.OneWayBind(ViewModel, x => x.Result, x => x.ResultLabel.Text);
            this.BindCommand(ViewModel, x => x.SubscribeOnInnerChain, x => x.SubscribeOnInnerChainButton);
            this.BindCommand(ViewModel, x => x.SubscribeOnCommandOutput, x => x.SubscribeOnCommandOutputButton);
            this.BindCommand(ViewModel, x => x.Clear, x => x.ClearButton);
        }
    }
}
