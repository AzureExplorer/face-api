using System.Web.Http;
using Unity;
using Unity.WebApi;

using Unity.Injection;

namespace AzureExplorer.FaceAPI
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers



            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
      
}