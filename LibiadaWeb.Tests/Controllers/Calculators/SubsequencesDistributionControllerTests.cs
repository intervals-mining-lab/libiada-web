namespace LibiadaWeb.Tests.Controllers.Calculators
{
    using LibiadaWeb.Controllers.Calculators;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class SubsequencesDistributionControllerTests
    {
        [Test]
        [Ignore("No mock db yet")]
        public void CreateAlignmentTask()
        {
            var controller = new SubsequencesDistributionController();
            var jsonResult = controller.CreateAlignmentTask(new long[] {3532336, 4882434 });
            dynamic result = JsonConvert.DeserializeObject(jsonResult);
            Assert.AreEqual(result.Status, "Success");

        }
    }
}
