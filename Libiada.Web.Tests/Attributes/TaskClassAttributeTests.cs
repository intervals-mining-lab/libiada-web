namespace Libiada.Web.Tests.Attributes;

using Libiada.Web.Attributes;
using Libiada.Web.Controllers.Calculators;

/// <summary>
/// The task class attribute tests.
/// </summary>
[TestFixture(TestOf = typeof(TaskClassAttribute))]
public class TaskClassAttributeTests
{
    /// <summary>
    /// Invalid task class value test.
    /// </summary>
    [Test]
    public void InvalidTaskClassValueTest()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new TaskClassAttribute(typeof(object)));
            Assert.Throws<ArgumentException>(() => new TaskClassAttribute(typeof(List<int>)));
        });

    }

    /// <summary>
    /// Image task class attribute value test.
    /// </summary>
    [Test]
    public void TaskClassAttributeValueTest()
    {
        TaskClassAttribute attribute = new(typeof(CalculationController));
        Assert.That(typeof(CalculationController), Is.EqualTo(attribute.Value));
    }
}
