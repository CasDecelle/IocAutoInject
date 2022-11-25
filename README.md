# IocAutoInject
Register your services with the InjectAttribute&lt;T> on your implementation and a single extension method on IServiceCollection. IocAutoInject will automatically scan all assemblies to find and register all services with this attribute automatically. Lifetime default is scoped, but can be passed as a parameter.
