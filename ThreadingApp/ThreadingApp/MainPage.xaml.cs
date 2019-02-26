using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace ThreadingApp
{
    public partial class MainPage : ReactiveContentPage<MainPageViewModel>
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainPageViewModel();

            this.OneWayBind(ViewModel, x => x.Output, x => x.OutputLabel.Text);
            this.BindCommand(ViewModel, x => x.SubscribeOnInnerChain, x => x.SubscribeOnInnerChainButton);
            this.BindCommand(ViewModel, x => x.SubscribeOnCommandOutput, x => x.SubscribeOnCommandOutputButton);
            this.BindCommand(ViewModel, x => x.Clear, x => x.ClearButton);

            InvokeCommandButton
                .Events()
                .Clicked
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(_ => Unit.Default)
                .InvokeCommand(ViewModel, x => x.InvokedCommand);
        }
    }
}
