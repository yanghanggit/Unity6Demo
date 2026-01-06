using NUnit.Framework;

/// <summary>
/// GameUtils 工具类的单元测试
/// </summary>
public class GameUtilsTests
{
    [Test]
    public void GetDisplayName_WithFullName_ReturnsLastPart()
    {
        // Arrange
        string fullName = "角色.战士.卡恩";

        // Act - 执行被测试的方法
        string result = GameUtils.GetDisplayName(fullName);

        // Assert - 验证结果
        Assert.AreEqual("卡恩", result);
    }

    [Test]
    public void GetDisplayName_WithSingleName_ReturnsSameName()
    {
        // Arrange
        string fullName = "卡恩";

        // Act
        string result = GameUtils.GetDisplayName(fullName);

        // Assert
        Assert.AreEqual("卡恩", result);
    }

    [Test]
    public void GetDisplayName_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        string fullName = "";

        // Act
        string result = GameUtils.GetDisplayName(fullName);

        // Assert
        Assert.AreEqual("", result);
    }

    [Test]
    public void GetDisplayName_WithNull_ReturnsEmpty()
    {
        // Arrange
        string fullName = null;

        // Act
        string result = GameUtils.GetDisplayName(fullName);

        // Assert
        Assert.AreEqual("", result);
    }
}
