namespace Libiada.Web.Tests.Extensions;

using Libiada.Database.Attributes;
using Libiada.Database.Models;
using Libiada.Web.Extensions;
using Libiada.Web.Models;
using Libiada.Web.Models.CalculatorsData;

using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// Tests for enum extensions.
/// </summary>
[TestFixture(TestOf = typeof(EnumExtensions))]
public class EnumExtensionsTests
{
    /// <summary>
    /// Enum used for testing extenssion methods.
    /// </summary>
    private enum TestEnum : byte
    {
        [Nature(Database.Models.Nature.Literature)]
        First = 1,

        [Nature(Database.Models.Nature.Music)]
        Second = 2,

        [Nature(Database.Models.Nature.Image)]
        Third = 3
    }

    [Test]
    public void GetSelectListTest()
    {
        List<SelectListItem> expected = [
            new() { Text = "First", Value = "1" },
            new() { Text = "Second", Value = "2" },
            new() { Text = "Third", Value = "3" }
        ];
        var result = EnumExtensions.GetSelectList<TestEnum>();

        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));
    }

    [Test]
    public void GetSelectListWithSelectedValuesTest()
    {
        // test with one selected value
        List<SelectListItem> expected = [
            new() { Text = "First", Value = "1", Selected = true },
            new() { Text = "Second", Value = "2" },
            new() { Text = "Third", Value = "3" }
        ];
        var result = EnumExtensions.GetSelectList([TestEnum.First]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // test with several selected values
        expected = [
            new() { Text = "First", Value = "1" },
            new() { Text = "Second", Value = "2", Selected = true },
            new() { Text = "Third", Value = "3", Selected = true }
        ];
        result = EnumExtensions.GetSelectList([TestEnum.Second, TestEnum.Third]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // test with all values selected
        expected = [
            new() { Text = "First", Value = "1", Selected = true },
            new() { Text = "Second", Value = "2", Selected = true },
            new() { Text = "Third", Value = "3", Selected = true }
        ];
        result = EnumExtensions.GetSelectList([TestEnum.First, TestEnum.Second, TestEnum.Third]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));
    }

    [Test]
    public void ToSelectListTest()
    {
        // Test with all enum values
        List<TestEnum> input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        List<SelectListItem> expected = [
            new() { Text = "First", Value = "1" },
            new() { Text = "Second", Value = "2" },
            new() { Text = "Third", Value = "3" }
        ];
        var result = input.ToSelectList();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // Test with a single enum value
        input = [TestEnum.First];
        expected = [
           new() { Text = "First", Value = "1" }
       ];
        result = input.ToSelectList();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // Test with empty array
        input = [];
        expected = [];
        result = input.ToSelectList();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));
    }

    [Test]
    public void ToSelectListWithSelectedValuesTest()
    {
        // Test with all enum values and one selected
        List<TestEnum> input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        List<SelectListItem> expected = [
            new() { Text = "First", Value = "1" },
            new() { Text = "Second", Value = "2", Selected = true },
            new() { Text = "Third", Value = "3" }
        ];
        var result = input.ToSelectList([TestEnum.Second]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // Test with all enum values and several selected
        input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        expected = [
            new() { Text = "First", Value = "1", Selected = true },
            new() { Text = "Second", Value = "2", Selected = true },
            new() { Text = "Third", Value = "3" }
        ];
        result = input.ToSelectList([TestEnum.First, TestEnum.Second]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));

        // Test with a single enum value that is selected
        input = [TestEnum.Third];
        expected = [
           new() { Text = "Third", Value = "3", Selected = true }
       ];
        result = input.ToSelectList([TestEnum.Third]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemComparer()));
    }

    [Test]
    public void ToSelectListWithNatureTest()
    {
        // Test with all enum values
        List<TestEnum> input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        List<SelectListItemWithNature> expected = [
            new() { Text = "First", Value = "1", Nature = (byte)Nature.Literature  },
            new() { Text = "Second", Value = "2", Nature = (byte)Nature.Music },
            new() { Text = "Third", Value = "3", Nature = (byte)Nature.Image }
        ];
        var result = input.ToSelectListWithNature();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));

        // Test with a single enum value
        input = [TestEnum.First];
        expected = [
           new() { Text = "First", Value = "1", Nature = (byte)Nature.Literature }
       ];
        result = input.ToSelectListWithNature();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));

        // Test with empty array
        input = [];
        expected = [];
        result = input.ToSelectListWithNature();
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));
    }

    [Test]
    public void ToSelectListWithNatureWithSelectedValuesTest()
    {
        // Test with all enum values and one selected
        List<TestEnum> input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        List<SelectListItemWithNature> expected = [
            new() { Text = "First", Value = "1", Nature = (byte)Nature.Literature },
            new() { Text = "Second", Value = "2", Selected = true, Nature = (byte)Nature.Music },
            new() { Text = "Third", Value = "3", Nature = (byte)Nature.Image }
        ];
        var result = input.ToSelectListWithNature([TestEnum.Second]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));

        // Test with all enum values and several selected
        input = [TestEnum.First, TestEnum.Second, TestEnum.Third];
        expected = [
            new() { Text = "First", Value = "1", Selected = true, Nature = (byte)Nature.Literature },
            new() { Text = "Second", Value = "2", Nature = (byte)Nature.Music },
            new() { Text = "Third", Value = "3", Selected = true, Nature = (byte)Nature.Image }
        ];
        result = input.ToSelectListWithNature([TestEnum.First, TestEnum.Third]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));

        // Test with a single enum value that is selected
        input = [TestEnum.Third];
        expected = [
           new() { Text = "Third", Value = "3", Selected = true, Nature = (byte)Nature.Image }
       ];
        result = input.ToSelectListWithNature([TestEnum.Third]);
        Assert.That(result, Is.EquivalentTo(expected).Using(new SelectListItemWithNatureComparer()));
    }
}
