namespace GRPCServer
{
    public class GreeterService : IGreeterService
    {
        public string Ping()
        {
            return "Pong"; 
        }
    }
}
