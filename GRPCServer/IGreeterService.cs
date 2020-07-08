using System.ServiceModel;

namespace GRPCServer
{
    [ServiceContract]
    public interface IGreeterService
    {
        [OperationContract]
        string Ping(); 
    }
}
