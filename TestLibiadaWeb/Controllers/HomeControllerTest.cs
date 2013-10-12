using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibiadaWeb.Controllers;

namespace TestLibiadaWeb.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Упорядочение
            HomeController controller = new HomeController();

            // Действие
            ViewResult result = controller.Index() as ViewResult;

            // Утверждение
            Assert.AreEqual("Измените этот шаблон, чтобы быстро приступить к работе над приложением ASP.NET MVC.", result.ViewBag.Message);
        }

        [TestMethod]
        public void About()
        {
            // Упорядочение
            HomeController controller = new HomeController();

            // Действие
            ActionResult result = controller.About();

            // Утверждение
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Contact()
        {
            // Упорядочение
            HomeController controller = new HomeController();

            // Действие
            ActionResult result = controller.Contact();

            // Утверждение
            Assert.IsNotNull(result);
        }
    }
}
