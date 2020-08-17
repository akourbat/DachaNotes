using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BlazorCientApp.ViewModels;
using DynamicData.Binding;
using ReactiveUI;
using SharedModels;

namespace BlazorCientApp.Pages
{
    public partial class BookMainView
    {
        public BookMainView()
        {
            this.WhenActivated(disposables => 
            {
                this.ViewModel.Derived
                    .ObserveCollectionChanges()
                    //.Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Add || e.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                    .Select(e => Unit.Default) //not sure it is needed
                ///.ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => InvokeAsync(() => { _shouldRender = true; StateHasChanged(); }))
                    .DisposeWith(disposables);
            });
        }
        
        protected override async Task OnInitializedAsync()
        {
            await ViewModel.OnActivateAsync.Execute();
            await base.OnInitializedAsync();
        }
    }
}
