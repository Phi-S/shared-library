using Microsoft.Extensions.Hosting;
using Serilog;

namespace shared_library_test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var builder = Host.CreateDefaultBuilder();
    }
}