using System.ServiceModel;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface IBookStore : IService
    {
        [OperationContract]
        Task<ReturnCode> CheckBookQuantityAsync(ExampleModel model);
    }
}
