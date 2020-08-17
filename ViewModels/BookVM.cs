using BlazorCientApp.Services;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using ReactiveUI;
using SharedModels;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BlazorClientApp.ViewModels
{
    public class BookVM : ReactiveObject, IActivatableViewModel
    {
        public class BookViewModelValidator : AbstractValidator<BookVM>
        {
            public BookViewModelValidator()
            {
                RuleFor(Q => Q.Title).NotEmpty().MaximumLength(5);
                RuleFor(Q => Q.Price).GreaterThanOrEqualTo(2).LessThan(10);
            }
        }
        public BookViewModelValidator Validator { get; }
        private readonly IRefitBookService _refitService;
        public JsonPatchDocument<Book> Patch;

        readonly ObservableAsPropertyHelper<bool> _isExecuting;
        public bool IsExecuting => _isExecuting.Value;

        readonly ObservableAsPropertyHelper<bool> _formInvalid;
        public bool FormInvalid => _formInvalid.Value;
        
        public string _id;
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        private int _price;
        public int Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }
        public ReactiveCommand<Unit, Unit> Initialize { get; private set; }
        public ReactiveCommand<Unit, Book> Submit { get; private set; }

        public ViewModelActivator Activator { get; }

        public BookVM(IRefitBookService refitService)
        {
            _refitService = refitService;
            Validator = new BookViewModelValidator(); 
            Activator = new ViewModelActivator();
            Patch = new JsonPatchDocument<Book>();

            this.WhenActivated(disposables => 
            {
                this.Initialize.Execute().Subscribe().DisposeWith(disposables);
            });

            this.Initialize = ReactiveCommand.CreateFromTask(async () => 
            {
                //TODO: Replace with actual call via Refit
                await Task.Delay(2000);
                var book = new Book() { Id = "fc62d1f9-3003-4d13-b90c-797108b8c2f3", Title = "Anon", Price = 7 };
                (Id, Title, Price) = (book.Id, book.Title, book.Price);
            });
            // Enable submit button when VM is valid, watches for changes only properties that require validation
            var _submit = this.WhenAnyValue(vm => vm.Title, vm => vm.Price)
                .Select(_ => !Validator.Validate(this).IsValid);

            _submit.ToProperty(this, x => x.FormInvalid, out _formInvalid, scheduler: RxApp.MainThreadScheduler);

            this.Submit = ReactiveCommand.CreateFromTask(async () => await _refitService.PatchBookAsync(Patch, Id), _submit);
            this.Submit.IsExecuting.ToProperty(this, x => x.IsExecuting, out _isExecuting, scheduler: RxApp.MainThreadScheduler);

            var c = this.WhenAnyValue(x => x.Id)
                .Skip(1)
                .Take(1)
                .Select(x => new Operation<Book>("test", $"/{nameof(Book.Id)}", null, x));

            // Modify patch document
            var a = this.WhenAnyValue(x => x.Title)
                .Skip(1) // supressing initial value always emitted by observables
                .Where(x => Validator.Validate(this, options => options.IncludeProperties(nameof(this.Title))).IsValid)
                .Select(x => new Operation<Book>()
                {
                    op = OperationType.Replace.ToString().ToLower(),
                    path = $"/{nameof(Book.Title)}",
                    value = x
                });
            var b = this.WhenAnyValue(x => x.Price)
                .Skip(1)
                .Where(x => Validator.Validate(this, options => options.IncludeProperties(nameof(this.Price))).IsValid)
                .Select(x => new Operation<Book>(OperationType.Replace.ToString().ToLower(), $"/{nameof(Book.Price)}", null, x));
                
            Observable.Merge(a, b, c).SubscribeOn(RxApp.MainThreadScheduler).Subscribe(o =>
            {
                var existing = Patch.Operations.SingleOrDefault(x => x.op == o.op && x.path == o.path);
                if (existing != null)
                {
                    Patch.Operations.Remove(existing);
                }
                Patch.Operations.Add(o);
            });
        }
    }
}
