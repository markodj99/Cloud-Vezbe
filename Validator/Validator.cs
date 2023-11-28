using Common.Interface;
using Common.Model;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Validator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Validator : StatelessService, IValidator
    {
        public Validator(StatelessServiceContext context) : base(context) { }

        public async Task<ReturnCode> BuyBookAsync(ExampleModel model)
        {
            if (model == null) return ReturnCode.ValidatorError;

            if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName) || string.IsNullOrEmpty(model.CardNumber)
                                                      || string.IsNullOrEmpty(model.BookName) || model.Quantity <= 0) return ReturnCode.ValidatorError;

            var proxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri("fabric:/Cloud/TransactionCoordinator"), new ServicePartitionKey(1));

            try
            {
                if (await proxy.PrepareAsync(model)) return await proxy.CommitAsync(model); 
                return ReturnCode.ValidatorError;
            }
            catch (Exception)
            {
                await proxy.RollbackAsync(model);
                return ReturnCode.ValidatorError;
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() 
            => this.CreateServiceRemotingInstanceListeners();

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
