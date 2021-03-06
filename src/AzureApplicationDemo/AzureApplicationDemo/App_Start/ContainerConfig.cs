﻿using Autofac;
using MediatR;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Autofac.Features.Variance;
using System.Collections.Generic;
using System.Web;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using AzureApplicationDemo.Features.Upload;
using AzureApplicationDemo.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Owin;

namespace AzureApplicationDemo
{
    public class ContainerConfig
    {
        public static void RegisterServices(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            builder.RegisterSource(new ContravariantRegistrationSource());
            RegisterMediatr(builder);
           
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterAssemblyTypes(typeof(UploadFileCommand).Assembly).AsImplementedInterfaces();
            builder.RegisterModule<AutofacWebTypesModule>();
            builder.RegisterFilterProvider();

            // build up list of services
            builder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationUserStore>().As<IUserStore<ApplicationUser>>().InstancePerRequest();
            builder.RegisterType<ApplicationDbContext>().AsSelf().InstancePerRequest();
            builder.RegisterType<UserManager<ApplicationUser>>();
            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();


            // register AutoFac as the container
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();

        }
        private static void RegisterMediatr(ContainerBuilder builder)
        {
            builder.Register(x => new ServiceLocatorProvider(() => new AutofacServiceLocator(AutofacDependencyResolver.Current.RequestLifetimeScope)))
                                        .InstancePerRequest();
            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => (IEnumerable<object>)c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
            });
            builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        }
    }
}