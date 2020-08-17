using BlazorCientApp.Services;
using DynamicData;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using SharedModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace BlazorCientApp.ViewModels
{
    public class BookMainViewModel : ReactiveObject, IDisposable
    {
        private HubConnection _hubConnection;
        private readonly IRefitBookService _webApiClient;

        private readonly SourceCache<Book, string> _items;
        private readonly ReadOnlyObservableCollection<Book> _derived;
        public ReadOnlyObservableCollection<Book> Derived => _derived;

        private string _selectedId;

        public string SelectedId
        {
            get => _selectedId;
            set => this.RaiseAndSetIfChanged(ref _selectedId, value);
        }

        private int _price;
        public int Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }
        public ReactiveCommand<Unit, Unit> OnActivateAsync { get; private set; }

        public BookMainViewModel(IRefitBookService webApiClient)
        {
            _items = new SourceCache<Book, string>(b => b.Id);
            _webApiClient = webApiClient;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:7071/api")
                .Build();

            var pricePredicate = this.WhenAnyValue(b => b.Price)
                .Select(PricePredicate);
            var idPredicate = this.WhenAnyValue(b => b.SelectedId)
                .Select(SelectionPredicate);

            var shared = _items
                .Connect().RefCount();

            shared.Filter(pricePredicate)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _derived)
                .Subscribe();

            //TODO 
            //shared.Filter(idPredicate)
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .ToProperty(... );

            this.OnActivateAsync = ReactiveCommand.CreateFromTask(async () =>
            {
                var apiBooks = await _webApiClient.GetBooksAsync();
                //TODO Handle errors
                var books = apiBooks.Content;
                _items.Edit(inner =>
                {
                    inner.Clear();
                    inner.AddOrUpdate(books);
                });
                _hubConnection.On<Book>("BookUpdate", book => _items.AddOrUpdate(book));
                await _hubConnection.StartAsync();
            });
        }

        private Func<Book, bool> SelectionPredicate(string id) => book => book.Id == id;

        private Func<Book, bool> PricePredicate(int price) => book => book.Price > price;
        
        public void Dispose()
        {
            _ = _hubConnection.DisposeAsync();
        }
    }
}
