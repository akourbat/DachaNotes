using BlazorCientApp.Services;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SharedModels;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BlazorCientApp.Pages
{
    public partial class BookView
    {
        [Inject]
        public IRefitBookService service { get; set; }

        protected string _submitted = String.Empty;
        protected bool loadFailed;

        private string _vTitle;
        public string VTitle { get => _vTitle; set { _vTitle = value; StateHasChanged(); }}

        protected Book _response;

        public BookView()
        {
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, vm => vm.Title, v => v.VTitle)
                    .DisposeWith(disposables);
            });
        }
        protected async override Task OnInitializedAsync()
        {
            //TODO: Should move this logic to OnParametersSetAsync
            //await ViewModel.Initialize.Execute();
            await base.OnInitializedAsync();
        }
        public async Task SubmitFormAsync()
        {
            try
            {
                loadFailed = false;
                _submitted = "Form Submitted";
                _response = await ViewModel.Submit.Execute();
            }
            catch (Exception)
            {
                loadFailed = true;
            }
        }
    }
}

