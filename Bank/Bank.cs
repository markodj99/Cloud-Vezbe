using Common.Interface;
using Common.Model;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Bank
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Bank : StatefulService, IBank
    {
        public Bank(StatefulServiceContext context) : base(context) { }

        public async Task<bool> CheckUserCreditAsync(ExampleModel model, double price)
        {
            var bankAccDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("Accounts");

            using var transaction = StateManager.CreateTransaction();
            var account = await bankAccDictionary.TryGetValueAsync(transaction, 4);

            return account.HasValue && !(account.Value.AmountOfMoney < price);
        }

        public async Task DrawMoneyAsync(double amount)
        {
            var bankAccDic = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("Accounts");
            var oldBankAccDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("OldAccounts");

            using var transaction = StateManager.CreateTransaction();
            var account = await bankAccDic.TryGetValueAsync(transaction, 4);

            var enumerableSource = await bankAccDic.CreateEnumerableAsync(transaction);
            var enumerator = enumerableSource.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await oldBankAccDict.AddAsync(transaction, current.Key, current.Value);
            }

            account.Value.AmountOfMoney -= amount;
            await transaction.CommitAsync();
        }

        public async Task GetPerviousStateAsync()
        {
            var bankAccDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("Accounts");
            var odlBankAccDict = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("OldAccounts");

            using var transaction = StateManager.CreateTransaction();

            if (await odlBankAccDict.GetCountAsync(transaction) == 0) return;

            var enumerablePrev = await odlBankAccDict.CreateEnumerableAsync(transaction);
            var enumerator = enumerablePrev.GetAsyncEnumerator();

            var enumerableNew = await bankAccDict.CreateEnumerableAsync(transaction);
            var newEnumerator = enumerableNew.GetAsyncEnumerator();

            while (await newEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = newEnumerator.Current;
                await bankAccDict.TryRemoveAsync(transaction, current.Key);
            }

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await bankAccDict.AddAsync(transaction, current.Key, current.Value);
            }

            await odlBankAccDict.ClearAsync();
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

        public async Task InitializeAsync()
        {
            List<BankAccount> accounts = new()
            {
                new BankAccount { AccountNumber = 1, AmountOfMoney = 1000 },
                new BankAccount { AccountNumber = 2, AmountOfMoney = 2000 },
                new BankAccount { AccountNumber = 3, AmountOfMoney = 3000 },
                new BankAccount { AccountNumber = 4, AmountOfMoney = 4000 },
                new BankAccount { AccountNumber = 5, AmountOfMoney = 5000 }
            };

            var bankAccDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<long, BankAccount>>("Accounts");

            using var transaction = StateManager.CreateTransaction();
            foreach (var account in accounts)
            {
                await bankAccDictionary.AddOrUpdateAsync(transaction, account.AccountNumber, account, (k, v) => account);
            }

            await transaction.CommitAsync();
        }

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
