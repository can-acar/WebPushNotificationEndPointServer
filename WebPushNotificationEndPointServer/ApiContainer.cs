using System.Reflection;
using Autofac;
using MediatR;
using WebPushNotificationEndPointServer.Services;
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

        builder.RegisterType<PushNotificationService>().As<IPushNotificationService>().SingleInstance();
        builder.Register(c => new HttpClient()).As<HttpClient>();


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