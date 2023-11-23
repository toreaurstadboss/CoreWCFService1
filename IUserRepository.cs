namespace CoreWCFService1
{

    public interface IUserRepository
    {
        public Task<bool> Authenticate(string username, string password);
    }

}

