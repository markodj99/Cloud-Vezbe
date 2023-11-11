using System.ServiceModel;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface IValidator : IService
    {
        [OperationContract]
        Task<ReturnCode> BuyBookAsync(ExampleModel model);
    }
}
