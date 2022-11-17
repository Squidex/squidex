// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing.Consume;

public class EventConsumerWorkerTests
{
    private readonly IEventConsumer consumer1 = A.Fake<IEventConsumer>();
    private readonly IEventConsumer consumer2 = A.Fake<IEventConsumer>();
    private readonly EventConsumerProcessor processor1 = A.Fake<EventConsumerProcessor>();
    private readonly EventConsumerProcessor processor2 = A.Fake<EventConsumerProcessor>();
    private readonly EventConsumerWorker sut;

    public EventConsumerWorkerTests()
    {
        var factory = new Func<IEventConsumer, EventConsumerProcessor>(consumer =>
        {
            return consumer == consumer1 ? processor1 : processor2;
        });

        A.CallTo(() => consumer1.Name).Returns("1");
        A.CallTo(() => consumer2.Name).Returns("2");

        sut = new EventConsumerWorker(new[] { consumer1, consumer2 }, factory);
    }

    [Fact]
    public async Task Should_stop_without_start()
    {
        await sut.StopAsync(default);
    }

    [Fact]
    public async Task Should_initialize_all_processors_on_initialize()
    {
        await sut.StartAsync(default);

        A.CallTo(() => processor1.InitializeAsync(default))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => processor2.InitializeAsync(default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_activate_all_processors_on_initialize()
    {
        await sut.StartAsync(default);

        A.CallTo(() => processor1.ActivateAsync())
            .MustHaveHappened();

        A.CallTo(() => processor2.ActivateAsync())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Shoud_start_correct_processor()
    {
        await sut.HandleAsync(new EventConsumerStart(consumer1.Name), default);

        A.CallTo(() => processor1.StartAsync())
            .MustHaveHappened();

        A.CallTo(() => processor2.StartAsync())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Shoud_stop_correct_processor()
    {
        await sut.HandleAsync(new EventConsumerStart(consumer1.Name), default);

        A.CallTo(() => processor1.StartAsync())
            .MustHaveHappened();

        A.CallTo(() => processor2.StartAsync())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Shoud_reset_correct_processor()
    {
        await sut.HandleAsync(new EventConsumerReset(consumer1.Name), default);

        A.CallTo(() => processor1.ResetAsync())
            .MustHaveHappened();

        A.CallTo(() => processor2.ResetAsync())
            .MustNotHaveHappened();
    }
}
