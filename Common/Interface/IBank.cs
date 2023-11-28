using System.ServiceModel;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    [ServiceContract]
    public interface IBank : IService
    {
        [OperationContract]
        Task<bool> CheckUserCreditAsync(ExampleModel model, double price);

        [OperationContract]
        Task DrawMoneyAsync(double amount);

        [OperationContract]
        Task GetPerviousStateAsync();
    }
}
