using Microsoft.AspNetCore.Authorization;

namespace CoreWCFService1
{
    public class Service : IService
    {

        [Authorize]
        public string GetData(int value)
        {
            return $"You entered: {value}. <br />The client logged in with transport security with BasicAuth with https (BasicHttpsBinding).<br /><br />The username is set inside ServiceSecurityContext.Current.PrimaryIdentity.Name: {ServiceSecurityContext.Current.PrimaryIdentity.Name}. <br /> This username is also stored inside Thread.CurrentPrincipal.Identity.Name: {Thread.CurrentPrincipal?.Identity?.Name}";
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
