using System.ServiceModel;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface IBookStore : IService
    {
        [OperationContract]
        Task<double> CheckBookQuantityAsync(ExampleModel model);
        [OperationContract]
        Task DrawBooks(ExampleModel model);
        [OperationContract]
        Task GetPerviousStateAsync();
    }
}
