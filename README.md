## RxThreadingApp
Dealing with multi-threaded apps is a common and required thing but it is not always simple to be achieved, specially when it comes to raw thread managment 
or even using tasks (aka `async/await` apis). 

Fortunately Rx.NET and ReactiveUI makes the things a lot easier for us by giving us a chance to schedule work using schedulers. 
Have you heard about them before? If not I strongly recommend you to take a look now! They will allow you to take full control over 
threading and also to implement unit tests easily with a `TestScheduler` that allows you to control time on the tests.

I've been working with these technologies for a few years but I've been always in doubt on some topics, so now I decided to make sure my assumptions were correct on how 
things behave and how they should be done properly. To be able to inspect what goes on I've created this Xamarin.Forms app that writes the current thread on screen
on some points of interest.

This app is intended to help other developers to understand what they are doing precisely and how they could improve their code to develop fluid apps.
No one wants to use an app that hangs while you fetch some data.

### Here were my questions/assumptions:
- Where the code on the constructor body would run by default? - On the main thread
- Where the command execution would happen if no scheduler is set on it (no `SubscribeOn`, `ObserveOn`)? - On the main thread
- Would commands behave the same as for regular observables? - Yes (respecting observeon, subscribeon) - This is WRONG!
- How the output scheduler of the commands would affect the command execution/output chain? - It would affect just the output (seems to be obvious)
- What would happen when a command is invoked using `InvokeCommand` observing on a background thread? - This was not fully clear for me
- What if I invoke a command using `InvokeCommand` as above but now I add the `SubscribeOn` to the command chain, and to the inner chain? - I was not pretty sure what would be the precedence
- Can I use `ObserveOn` more than once on the same chain? Would this be respected? - My assumption was yes for both

### What I've concluded on this:
- First and most important one: I've been doing things wrongly. Things that looked to be obvious are not exactly what I expected.
- Constructor body runs on the main thread
- Commands run by default on the main thread if nothing is set explicitly (as they assume the current thread to run in)
- Regular observables DO respect the `SubscribeOn` operator for setting the source sequence thread
- `ReactiveCommand` DOES NOT respect the `SubscribeOn` operator when set on the command output (WHY?) - it should be as it is an observable
- Every code that runs on the "body" of the method directly (outside of the observable chain itself) will assume the caller thread as context
- ReactiveCommand's `outputScheduler` must be set to the main thread (it is by default) - setting a different scheduler will sometimes make the command execution to "hang", specially if there is a command bound to it on the UI
- While unit testing you must set either the `ReactiveCommand outputScheduler` or set both the `RxApp.MainThreadScheduler` and `RxApp.TaskpoolScheduler` to be an instance of the `TestScheduler`, otherwise controlling time won't be effective in some situations. The same applies to `Observable.Delay`, `Observable.Throttle` and so on
- `ObserveOn` can be set more than once on the same observable chain, it is respected properly
- When a command is invoked using `InvokeCommand` observing a specific thread the command will assume the caller thread as context for the command execution, `SubscribeOn` will be considered if set on the inner command chain only
