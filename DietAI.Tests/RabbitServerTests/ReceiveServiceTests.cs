using DietAI.RabbitServer.Abstractions.RabbitConnection;
using DietAI.RabbitServer.Implementations.ReceiverService;
using NSubstitute;
using RabbitMQ.Client;

namespace DietAI.Tests.RabbitServerTests;

public class ReceiveServiceTests
{
    private ITopicFactory _topicFactory = null!;
    private IChannel _channel = null!;
    private ReceiverService _receiver = null!;

    [SetUp]
    public void Setup()
    {
        _topicFactory = Substitute.For<ITopicFactory>();
        _channel = Substitute.For<IChannel>();
        _topicFactory.ActiveChannel.Returns(_channel);

        _receiver = new ReceiverService(_topicFactory);
    }

    [Test]
    public async Task StartConsumingAsync_DeclaresQueue_And_StartsConsumer()
    {
        var queue = "q-start";

        // simulate BasicConsumeAsync returning a consumer tag
        _channel.BasicConsumeAsync(queue,
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<IDictionary<string, object>>(),
            Arg.Any<IAsyncBasicConsumer>(),
            Arg.Any<System.Threading.CancellationToken>()).Returns(Task.FromResult("tag-1"));

        var tag = await _receiver.StartConsumingAsync(queue, async ea => await Task.CompletedTask, autoAck: true);

        await _topicFactory.Received(1).DeclareQueueAsync(queue);
        Assert.That(tag, Is.EqualTo("tag-1"));
    }

    [Test]
    public async Task StopConsumingAsync_Cancels_Consumer_And_Removes_From_Dictionary()
    {
        // arrange: add a consumer tag into the private dictionary by starting consume
        var queue = "q";
        _channel.BasicConsumeAsync(queue,
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<IDictionary<string, object>>(),
            Arg.Any<IAsyncBasicConsumer>(),
            Arg.Any<System.Threading.CancellationToken>()).Returns(Task.FromResult("ctag"));
        var tag = await _receiver.StartConsumingAsync(queue, async ea => await Task.CompletedTask, autoAck: true);

        await _receiver.StopConsumingAsync(tag);

        await _channel.Received(1).BasicCancelAsync(tag);
    }

    [Test]
    public async Task GetMessageAsync_Delegates_To_Channel_BasicGetAsync()
    {
        var queue = "q-get";
        _channel.BasicGetAsync(queue, Arg.Any<bool>()).Returns(Task.FromResult<BasicGetResult?>(null));

        var res = await _receiver.GetMessageAsync(queue, autoAck: false);

        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task RejectAsync_And_RejectMultipleAsync_Call_Channel()
    {
        await _receiver.RejectAsync(5, requeue: true);
        await _receiver.RejectMultipleAsync(10, requeue: false);

        await _channel.Received(1).BasicRejectAsync(5, true);
        await _channel.Received(1).BasicNackAsync(10, true, false);
    }
}
