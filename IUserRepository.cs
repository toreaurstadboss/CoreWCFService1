namespace CoreWCFService1
{
    public interface IUserRepository
    {

        public Task<bool> Authenticate(string username, string password);
    }

    public class UserRepository : IUserRepository
    {
        public Task<bool> Authenticate(string username, string password)
        {
            //TODO: some dummie auth mechanism used here, make something more realistic such as DB user repo lookup or similar
            if (username == "someuser" && password == "somepassw0rd")
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

}

