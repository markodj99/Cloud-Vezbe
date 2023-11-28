using Common.Model;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface ITransactionCoordinator : IService
    {
        [OperationContract]
        Task<bool> PrepareAsync(ExampleModel model);

        [OperationContract]
        Task<ReturnCode> CommitAsync(ExampleModel model);

        [OperationContract]
        Task<ReturnCode> RollbackAsync(ExampleModel model);
    }
}
