using System.Reflection;
using DietAI.RabbitServer.Implementations.RabbitConnection;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using NSubstitute;
using RabbitMQ.Client;

namespace DietAI.Tests.RabbitServerTests;

public class TopicFactoryTests
{
    private ITopicFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new TopicFactory();
    }
 
    [Test]
    public void DeclareQueueAsync_Throws_WhenActiveChannelIsNull()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _factory.DeclareQueueAsync("test-queue"));
    }

    [Test]
    public void DeclareQueueAsync_Throws_WhenChannelIsClosed()
    {
        var channel = Substitute.For<IChannel>();
        channel.IsClosed.Returns(true);

        // set private setter ActiveChannel via reflection
        var prop = typeof(TopicFactory).GetProperty("ActiveChannel", BindingFlags.Public | BindingFlags.Instance)!;
        prop.SetValue(_factory, channel);

        Assert.ThrowsAsync<System.Threading.Channels.ChannelClosedException>(async () => await _factory.DeclareQueueAsync("test-queue"));
    }

    [Test]
    public async Task DeclareQueueAsync_Delegates_To_ChannelQueueDeclareAsync()
    {
        var channel = Substitute.For<IChannel>();
        channel.IsClosed.Returns(false);

        var expected = new QueueDeclareOk("test-queue", 0u, 0u);
        channel.QueueDeclareAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>())
               .Returns(Task.FromResult(expected));

        var prop = typeof(TopicFactory).GetProperty("ActiveChannel", BindingFlags.Public | BindingFlags.Instance)!;
        prop.SetValue(_factory, channel);

        var result = await _factory.DeclareQueueAsync("test-queue");

        await channel.Received(1).QueueDeclareAsync("test-queue", true, true, true, Arg.Any<IDictionary<string, object>>());
        Assert.That(result, Is.EqualTo(expected));
    }
}
