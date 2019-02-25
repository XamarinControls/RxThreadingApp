using ReactiveUI;
using ReactiveUI.XamForms;

namespace ThreadingApp
{
    public partial class MainPage : ReactiveContentPage<MainPageViewModel>
    {
        public MainPage()
        {
            InitializeComponent();

            var ctx = new MainPageViewModel();
            ViewModel = ctx;

            this.OneWayBind(ViewModel, x => x.Result, x => x.ResultEditor.Text);
            this.BindCommand(ViewModel, x => x.Start, x => x.StartButton);
        }
    }
}
