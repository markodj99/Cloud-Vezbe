using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interface
{
    [ServiceContract]
    public interface ICoordinator : IService
    {
        [OperationContract]
        Task<ReturnCode> SendBookAsync(ExampleModel model);
    }
}
