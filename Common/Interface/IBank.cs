using System.ServiceModel;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface IBank : IService
    {
        [OperationContract]
        Task<ReturnCode> CheckUserCreditAsync(ExampleModel model);
    }
}
