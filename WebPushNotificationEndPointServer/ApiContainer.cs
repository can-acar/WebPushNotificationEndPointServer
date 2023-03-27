using System.Reflection;
using Autofac;
using MediatR;
using Module = Autofac.Module;

namespace WebPushNotificationEndPointServer;

public class ApiContainer : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assemblies = Assembly.GetExecutingAssembly();


        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        

        // builder.RegisterAssemblyTypes(assemblies)
        //     .AsClosedTypesOf(typeof(IRequestHandler<>))
        //     .AsImplementedInterfaces()
        //     .InstancePerLifetimeScope();
        //
        //
        // builder.RegisterAssemblyTypes(assemblies);
        //
        // builder.RegisterAssemblyTypes(assemblies)
        //     .AsClosedTypesOf(typeof(INotificationHandler<>))
        //     .AsImplementedInterfaces()
        //     .InstancePerLifetimeScope();
    }
}