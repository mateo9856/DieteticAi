using DietAI.Api.Options;
using DietAI.Api.Services.AiPlanSender.Implementations;
using DietAI.Api.Services.AiPlanSender.Enums;
using DietAI.Api.Services.AiPlanSender.Requests;
using DietAI.Api.Tools;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;

namespace DietAI.Tests.ApiServices;

public class AiPlanSenderServiceTests
{
    [Test]
    public async Task SendPlanRequestAsync_WhenNoResponseWithinTimeout_ThrowsTimeoutException()
    {
        var senderService = Substitute.For<ISenderService>();
        var receiveService = Substitute.For<IReceiveService>();
        receiveService.GetMessageAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult<BasicGetResult?>(null));

        var topicFactory = Substitute.For<ITopicFactory>();
        var channel = Substitute.For<IChannel>();
        channel.IsClosed.Returns(false);
        topicFactory.ActiveChannel.Returns(channel);

        var connectionFactory = Substitute.For<IRabbitConnectionFactory>();
        var rabbitOptions = Options.Create(new RabbitMqOptions
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        });

        var topicManager = new TopicManager(connectionFactory, topicFactory, rabbitOptions);
        var service = new AiPlanSenderService(senderService, receiveService, topicManager);

        var request = new SendPlanRequest
        {
            Age = 30,
            ActualWeight = 70m,
            ActualHeight = 175m,
            Sex = SexEnum.Male,
            DietType = DietType.Standard,
            CaloricDemand = 2000m
        };

        Func<Task> act = async () => await service.SendPlanRequestAsync("user-1", request, CancellationToken.None);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("*No response received for request*");
    }
}
