using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace ThreadingApp
{
    public class MainPageViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> Start { get; }

        private string _result;
        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        StringBuilder _builder = new StringBuilder();

        public MainPageViewModel()
        {
            AppendResult("CTOR");

            Start = ReactiveCommand
                .CreateFromObservable<Unit, Unit>(_ =>
                {
                    AppendResult($"COMMAND BODY");

                    return GetSomeFakeData()
                        .Select(result =>
                        {
                            AppendResult("COMMAND CHAIN");
                            return result;
                        })
                        .SubscribeOn(RxApp.TaskpoolScheduler);
                },
                outputScheduler: RxApp.MainThreadScheduler);

            Start
                .Select(result =>
                {
                    AppendResult("COMMAND CHAIN BEFORE SUBSCRIBEON");
                    return result;
                })
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .Select(result =>
                {
                    AppendResult("COMMAND CHAIN AFTER SUBSCRIBEON");
                    return result;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(result =>
                {
                    AppendResult("COMMAND CHAIN AFTER OBSERVEON");
                    return result;
                })
                .Subscribe(
                    onNext: response =>
                    {
                        AppendResult("COMMAND ONNEXT");
                    },
                    onError: ex =>
                    {
                        AppendResult("COMMAND ONERROR");
                    });
        }

        private IObservable<Unit> GetSomeFakeData()
        {
            AppendResult("COMMAND BODY");

            return Observable.Create<Unit>(observer =>
            {
                AppendResult("COMMAND CHAIN");

                observer.OnNext(Unit.Default);
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        private void AppendResult(string result)
        {
            _builder.AppendLine($"- {result}: {Thread.CurrentThread.ManagedThreadId}");
            Result = _builder.ToString();
        }

        private void ClearResult()
        {
            _builder.Clear();
            Result = string.Empty;
        }
    }
}
