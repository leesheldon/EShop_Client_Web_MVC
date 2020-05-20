using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc5;
using Client_Web_MVC.Extensions;
using Client_Web_MVC.Services;

namespace Client_Web_MVC
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IUserSession, UserSession>();
            container.RegisterType<IProductService, ProductService>();
            container.RegisterType<IAccountService, AccountService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}