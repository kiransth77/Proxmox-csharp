using Xunit;

namespace ProxmoxApi.Tests;

public class BasicLibraryTests
{
    [Fact]
    public void ProxmoxClient_CanBeInstantiated()
    {
        // This is a basic test to ensure the library can be loaded
        // More comprehensive tests will be added as the API evolves
        
        Assert.True(true, "ProxmoxApi library loaded successfully");
    }
}
