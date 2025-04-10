using NUnit.Framework;
using V2.Components;

public class ResourceComponentTests
{
    private ResourceComponent resourceComponent;

    [SetUp]
    public void Setup()
    {
        resourceComponent = new ResourceComponent();
    }

    [Test]
    public void GenerateResources_AddsCorrectValues()
    {
        // Act
        resourceComponent.GenerateResources();

        // Assert
        var resources = resourceComponent.GetAllResources();
        Assert.AreEqual(10f, resources["Food"]);
        Assert.AreEqual(5f, resources["Wood"]);
    }

    [Test]
    public void GetResourceOverview_FormatsCorrectly()
    {
        resourceComponent.GenerateResources();
        string overview = resourceComponent.GetResourceOverview();

        Assert.IsTrue(overview.Contains("Food: 10.0"));
        Assert.IsTrue(overview.Contains("Wood: 5.0"));
    }
}