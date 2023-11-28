using Common.Interface;
using Common.Model;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;
using System.Net;

namespace BookStore
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BookStore : StatefulService, IBookStore
    {
        public BookStore(StatefulServiceContext context) : base(context) { }

        public async Task<double> CheckBookQuantityAsync(ExampleModel model)
        {
            var books = await StateManager.GetOrAddAsync<IReliableDictionary<long, Book>>("Books");
        
            using var transaction = StateManager.CreateTransaction();
            var book = await books.TryGetValueAsync(transaction, 4);

            if (!book.HasValue) return -1;
            if (book.Value.Quantity < model.Quantity) return -1;

            return model.Quantity * book.Value.Price;
        }

        public async Task DrawBooks(ExampleModel model)
        {
            var bookDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, Book>>("Books");
            var oldBookDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, Book>>("OldBooks");

            using var transaction = StateManager.CreateTransaction();
            var book = await bookDict.TryGetValueAsync(transaction, 4);

            var enumerableSource = await bookDict.CreateEnumerableAsync(transaction);
            var enumerator = enumerableSource.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await oldBookDict.AddAsync(transaction, current.Key, current.Value);
            }

            book.Value.Quantity -= model.Quantity;

            await bookDict.AddOrUpdateAsync(transaction, book.Value.Id, book.Value, (k, v) => book.Value);
            await transaction.CommitAsync();
        }

        public async Task GetPerviousStateAsync()
        {
            var bookDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, Book>>("Books");
            var oldBookDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, Book>>("OldBooks");

            using var transaction = StateManager.CreateTransaction();

            if (await oldBookDict.GetCountAsync(transaction) == 0) return;

            var enumerablePrev = await oldBookDict.CreateEnumerableAsync(transaction);
            var enumerator = enumerablePrev.GetAsyncEnumerator();

            var enumerableNew = await bookDict.CreateEnumerableAsync(transaction);
            var newEnumerator = enumerableNew.GetAsyncEnumerator();

            while (await newEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = newEnumerator.Current;
                await bookDict.TryRemoveAsync(transaction, current.Key);
            }

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await bookDict.AddAsync(transaction, current.Key, current.Value);
            }

            await oldBookDict.ClearAsync();

            await transaction.CommitAsync();
        }

        public async Task InitializeAsync()
        {
            List<Book> books = new()
            {
                new Book { Id = 1, Title = "Prva", Author = "Prvi", Quantity = 1, Price = 100 },
                new Book { Id = 2, Title = "Druga", Author = "Drugi", Quantity = 2, Price = 200 },
                new Book { Id = 3, Title = "Treca", Author = "Treci", Quantity = 3, Price = 300 },
                new Book { Id = 4, Title = "Cetvrta", Author = "Cetvrti", Quantity = 4, Price = 400 },
                new Book { Id = 5, Title = "Peta", Author = "Peti", Quantity = 5, Price = 580 }
            };

            var bookDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("Books");

            using var transaction = StateManager.CreateTransaction();

            foreach (var book in books)
            {
                await bookDictionary.AddOrUpdateAsync(transaction, book.Title, book, (k, v) => book);
            }

            await transaction.CommitAsync();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            => this.CreateServiceRemotingReplicaListeners();

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync();

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
