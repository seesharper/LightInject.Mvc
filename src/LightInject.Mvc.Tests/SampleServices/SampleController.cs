namespace LightInject.Mvc.Tests.SampleServices
{
    using System.Web.Mvc;

    public class SampleController : Controller
    {
        [SampleFilter]
        public void Execute()
        {

        }
    }

    public class SampleFilterAttribute : ActionFilterAttribute
    {
        public IFoo Foo { get; set; }
    }
}