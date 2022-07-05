// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed class TestState<T> where T : class, new()
    {
        private HandleSnapshot<T>? handleSnapshot;
        private T state = new T();

        public IPersistenceFactory<T> PersistenceFactory { get; private set; }

        public IPersistence<T> Persistence { get; } = A.Fake<IPersistence<T>>();

        public T Value
        {
            get => state;
            set => state = value;
        }

        public TestState(DomainId id)
            : this(id, A.Fake<IPersistenceFactory<T>>())
        {
        }

        public TestState(DomainId id, IPersistenceFactory<T> persistenceFactory)
        {
            PersistenceFactory = persistenceFactory;

            A.CallTo(() => PersistenceFactory.WithSnapshots(A<Type>._, id, A<HandleSnapshot<T>>._))
                .Invokes(x =>
                {
                    handleSnapshot = x.GetArgument<HandleSnapshot<T>>(2);
                })
                .Returns(Persistence);

            A.CallTo(() => PersistenceFactory.WithSnapshotsAndEventSourcing(A<Type>._, id, A<HandleSnapshot<T>>._, A<HandleEvent>._))
                .Invokes(x =>
                {
                    handleSnapshot = x.GetArgument<HandleSnapshot<T>>(2);
                })
                .Returns(Persistence);

            A.CallTo(() => Persistence.WriteSnapshotAsync(A<T>._, A<CancellationToken>._))
                .Invokes(x =>
                {
                    state = x.GetArgument<T>(0)!;
                });

            A.CallTo(() => Persistence.ReadAsync(A<long>._, A<CancellationToken>._))
                .Invokes(x =>
                {
                    handleSnapshot?.Invoke(state, 0);
                });

            A.CallTo(() => Persistence.DeleteAsync(A<CancellationToken>._))
                .Invokes(x =>
                {
                    state = new T();
                });
        }
    }
}
