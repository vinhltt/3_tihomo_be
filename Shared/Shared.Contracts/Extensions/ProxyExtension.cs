using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Contracts.Extensions;

/// <summary>
///     Extension methods for adding proxied services to the IServiceCollection. (EN)<br />
///     Các phương thức mở rộng để thêm dịch vụ được ủy nhiệm (proxied) vào IServiceCollection. (VI)
/// </summary>
public static class ProxyExtension
{
    /// <summary>
    ///     Adds a proxied scoped service of the type specified in TInterface with an implementation type specified in
    ///     TImplementation. (EN)<br />
    ///     Thêm một dịch vụ phạm vi (scoped) được ủy nhiệm với kiểu được chỉ định trong TInterface và kiểu triển khai được chỉ
    ///     định trong TImplementation. (VI)
    /// </summary>
    /// <typeparam name="TInterface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </typeparam>
    /// <typeparam name="TImplementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </typeparam>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    public static void AddProxiedScoped<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TImplementation>();
        services.AddScoped(typeof(TInterface), serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService<TImplementation>();
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(typeof(TInterface), actual, interceptors);
        });
    }

    /// <summary>
    ///     Adds a proxied scoped service of the type specified in @interface with an implementation type specified in
    ///     implementation. (EN)<br />
    ///     Thêm một dịch vụ phạm vi (scoped) được ủy nhiệm với kiểu giao diện được chỉ định trong @interface và kiểu triển
    ///     khai được chỉ định trong implementation. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    /// <param name="interface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </param>
    /// <param name="implementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </param>
    public static void AddProxiedScoped(this IServiceCollection services, Type @interface, Type implementation)
    {
        services.AddScoped(implementation);
        services.AddScoped(@interface, serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService(implementation);
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(@interface, actual, interceptors);
        });
    }

    /// <summary>
    ///     Adds a proxied transient service of the type specified in TInterface with an implementation type specified in
    ///     TImplementation. (EN)<br />
    ///     Thêm một dịch vụ tạm thời (transient) được ủy nhiệm với kiểu được chỉ định trong TInterface và kiểu triển khai được
    ///     chỉ định trong TImplementation. (VI)
    /// </summary>
    /// <typeparam name="TInterface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </typeparam>
    /// <typeparam name="TImplementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </typeparam>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    public static void AddProxiedTransient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddTransient<TImplementation>();
        services.AddTransient(typeof(TInterface), serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService<TImplementation>();
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(typeof(TInterface), actual, interceptors);
        });
    }

    /// <summary>
    ///     Adds a proxied transient service of the type specified in @interface with an implementation type specified in
    ///     implementation. (EN)<br />
    ///     Thêm một dịch vụ tạm thời (transient) được ủy nhiệm với kiểu giao diện được chỉ định trong @interface và kiểu triển
    ///     khai được chỉ định trong implementation. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    /// <param name="interface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </param>
    /// <param name="implementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </param>
    public static void AddProxiedTransient(this IServiceCollection services, Type @interface, Type implementation)
    {
        services.AddTransient(implementation);
        services.AddTransient(@interface, serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService(implementation);
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(@interface, actual, interceptors);
        });
    }

    /// <summary>
    ///     Adds a proxied singleton service of the type specified in TInterface with an implementation type specified in
    ///     TImplementation. (EN)<br />
    ///     Thêm một dịch vụ đơn thể (singleton) được ủy nhiệm với kiểu được chỉ định trong TInterface và kiểu triển khai được
    ///     chỉ định trong TImplementation. (VI)
    /// </summary>
    /// <typeparam name="TInterface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </typeparam>
    /// <typeparam name="TImplementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </typeparam>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    public static void AddProxiedSingleton<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton(typeof(TInterface), serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService<TImplementation>();
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(typeof(TInterface), actual, interceptors);
        });
    }

    /// <summary>
    ///     Adds a proxied singleton service of the type specified in @interface with an implementation type specified in
    ///     implementation. (EN)<br />
    ///     Thêm một dịch vụ đơn thể (singleton) được ủy nhiệm với kiểu giao diện được chỉ định trong @interface và kiểu triển
    ///     khai được chỉ định trong implementation. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection to add the service to. (EN)<br />
    ///     IServiceCollection để thêm dịch vụ vào. (VI)
    /// </param>
    /// <param name="interface">
    ///     The type of the interface to add. (EN)<br />
    ///     Kiểu của giao diện cần thêm. (VI)
    /// </param>
    /// <param name="implementation">
    ///     The type of the implementation to use. (EN)<br />
    ///     Kiểu của lớp triển khai cần sử dụng. (VI)
    /// </param>
    public static void AddProxiedSingleton(this IServiceCollection services, Type @interface, Type implementation)
    {
        services.AddSingleton(implementation);
        services.AddSingleton(@interface, serviceProvider =>
        {
            var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
            var actual = serviceProvider.GetRequiredService(implementation);
            var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
            return proxyGenerator.CreateInterfaceProxyWithTarget(@interface, actual, interceptors);
        });
    }
}