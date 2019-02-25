using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace ThreadingApp
{
    public class MainPageViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> SubscribeOnInnerChain { get; }

        public ReactiveCommand<Unit, Unit> SubscribeOnCommandOutput { get; }

        public ReactiveCommand<Unit, Unit> Clear { get; }

        private string _result;
        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        public MainPageViewModel()
        {
            AppendResult("Ctor");

            Clear = ReactiveCommand
                .Create<Unit, Unit>(_ =>
                {
                    //Conclusion: Command execution runs fully on the main thread here as it has not been set
                    ClearResult();
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            SubscribeOnCommandOutput = ReactiveCommand
                .CreateFromObservable<Unit, Unit>(_ =>
                {
                    AppendResult($"ReactiveCommand body");

                    //Conclusion: Command execution runs fully on the main thread here as it has not been set
                    return GetSomeFakeData()
                        .Select(result =>
                        {
                            AppendResult("Observable chain");
                            return result;
                        });
                },
                outputScheduler: RxApp.MainThreadScheduler);

            SubscribeOnInnerChain = ReactiveCommand
                .CreateFromObservable<Unit, Unit>(_ =>
                {
                    //Conclusion: Everything that runs on the command body will assume the caller thread (eg InvokeCommand)
                    AppendResult($"ReactiveCommand body");

                    return GetSomeFakeData()
                        .Select(result =>
                        {
                            AppendResult("Observable chain");
                            return result;
                        })
                        //Conclusion: This is what makes the operation to run on the taskpool
                        .SubscribeOn(RxApp.TaskpoolScheduler);
                },
                //Conclusion: Output Scheduler must be the Main Thread (it is by default), otherwise the bound commands will "hang" the execution of the UI
                outputScheduler: RxApp.MainThreadScheduler);

            Clear.Subscribe();

            SubscribeOnCommandOutput
                .Select(result =>
                {
                    AppendResult("Command output before SubscribeOn");
                    return result;
                })
                //Conclusion: SubscribeOn won't be considered when set on the command chain!
                //Regular observable chains will consider this (like on line 66), this is not valid for the ReactiveCommand only. WHY?
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .Select(result =>
                {
                    AppendResult("Command output after SubscribeOn");
                    return result;
                })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(result =>
                {
                    AppendResult("Command output after ObserveOn");
                    return result;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                //Conclusion: Multiple uses of ObserveOn will be respected properly
                .Subscribe(_ =>
                {
                    AppendResult("Observer OnNext");
                });

            SubscribeOnInnerChain
                .Select(result =>
                {
                    AppendResult("Command output");
                    return result;
                })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(result =>
                {
                    AppendResult("Command output after ObserveOn");
                    return result;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    AppendResult("Observer OnNext");
                });
        }

        private IObservable<Unit> GetSomeFakeData()
        {
            AppendResult("Observable body");

            return Observable.Create<Unit>(observer =>
            { 
                AppendResult("Observable itself");

                observer.OnNext(Unit.Default);
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        private void AppendResult(string result)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            //For testing purposes
            RxApp.MainThreadScheduler.Schedule(
                () => Result = $"{Result}{Environment.NewLine} - {result} ThreadId: {threadId}");
        }

        private void ClearResult()
        {
            Result = string.Empty;
        }
    }
}
