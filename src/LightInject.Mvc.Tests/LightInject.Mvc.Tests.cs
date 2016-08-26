namespace LightInject.Mvc.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;   
    using Mvc;    
    using Xunit;
    using Moq;
    using SampleServices;

    public class MvcTests : TestBase
    {
        [Fact]
        public void GetFilters_CustomFilter_InjectsPropertyDependencies()
        {
            var container = CreateContainer();
            container.Register<IFoo, Foo>();
            var filterProvider = new LightInjectFilterProvider(container);
            var actionDescriptor = CreateActionDescriptor();
            var controllerContext = CreateControllerContext();

            var filter = filterProvider.GetFilters(controllerContext, actionDescriptor).First();

            Assert.IsType(typeof(Foo), ((SampleFilterAttribute)filter.Instance).Foo);
        }

        [Fact]
        public void GetService_KnownService_ReturnsInstance()
        {
            var container = CreateContainer();
            container.Register<IFoo, Foo>();
            IDependencyResolver resolver = new LightInjectMvcDependencyResolver(container);

            var instance = resolver.GetService<IFoo>();

            Assert.IsType(typeof(Foo), instance);
        }

        [Fact]
        public void GetService_UnknownService_ReturnsNull()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>();
            IDependencyResolver resolver = new LightInjectMvcDependencyResolver(container);

            var instance = resolver.GetService<IFoo>();

            Assert.Null(instance);
        }

        [Fact]
        public void GetServices_MultipleServices_ReturnsAllInstances()
        {
            var container = CreateContainer();
            container.Register<IFoo, Foo>();
            container.Register<IFoo, AnotherFoo>("AnotherFoo");
            IDependencyResolver resolver = new LightInjectMvcDependencyResolver(container);

            var instances = resolver.GetServices<IFoo>();

            Assert.Equal(2, instances.Count());
        }

        [Fact]
        public void GetServices_UnknownService_ReturnsEmptyEnumerable()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>();
            IDependencyResolver resolver = new LightInjectMvcDependencyResolver(container);

            var instances = resolver.GetServices<IFoo>();

            Assert.Equal(0, instances.Count());
        }

        [Fact]
        public void RegisterControllers_AssemblyWithController_RegistersController()
        {
            var container = CreateContainer();
            container.RegisterControllers(typeof(MvcTests).Assembly);

            Assert.True(container.AvailableServices.Count() == 1);
            Assert.True(container.AvailableServices.Any(sr => sr.ServiceType == typeof(SampleController)));
        }

        [Fact]
        public void RegisterControllers_NoSpecifiedAssembly_RegistersController()
        {
            var container = CreateContainer();
            container.RegisterControllers();

            Assert.True(container.AvailableServices.Count() == 1);
            Assert.True(container.AvailableServices.Any(sr => sr.ServiceType == typeof(SampleController)));
        }

        [Fact]
        public void GetInstance_FilterProvider_ReturnsCustomFilterProvider()
        {
            var container = CreateContainer();
            container.RegisterControllers();
            container.EnableMvc();

            var instance = container.GetInstance<IFilterProvider>();

            Assert.IsType(typeof(LightInjectFilterProvider), instance);
        }

        [Fact]
        public void GetAllInstances_FilterProvider_ReturnsEnumerable()
        {
            var container = CreateContainer();
            container.RegisterControllers();
            container.EnableMvc();

            var instance = container.GetAllInstances<IFilterProvider>();
            Assert.IsAssignableFrom(typeof(IEnumerable<IFilterProvider>), instance);
            Assert.IsAssignableFrom(typeof(LightInjectFilterProvider), instance.First());
        }


        private static ActionDescriptor CreateActionDescriptor()
        {
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(typeof(SampleController));
            var method = typeof(SampleController).GetMethod("Execute");
            ActionDescriptor actionDescriptor = new ReflectedActionDescriptor(method, "Execute", controllerDescriptor);
            return actionDescriptor;
        }

        private static ControllerContext CreateControllerContext()
        {
            var httpContextMock = new Mock<HttpContextBase>();
            var controllerContext = new ControllerContext(httpContextMock.Object, new RouteData(), new SampleController());
            return controllerContext;
        }
    }
}