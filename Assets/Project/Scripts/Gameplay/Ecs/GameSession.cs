using System;
using System.Collections.Generic;
using System.Threading;
using Leopotam.EcsLite;

namespace Project.Scripts.Gameplay.Ecs
{
    public sealed class GameSession : IDisposable
    {
        private readonly CancellationTokenSource m_cancellationTokenSource;
        private readonly EcsWorld m_world;
        private readonly IEcsSystems m_updateSystems;
        private readonly IEcsSystems m_fixedSystems;
        private readonly List<IGameSessionOperation> m_sessionOperations;

        private bool m_isStarted;
        private bool m_isDisposed;

        public GameSession(IEnumerable<IEcsSystem> systems, IEnumerable<IGameSessionOperation> sessionOperations = null)
        {
            if (systems == null)
                throw new ArgumentNullException(nameof(systems));

            m_cancellationTokenSource = new CancellationTokenSource();
            m_world = new EcsWorld();
            m_updateSystems = new EcsSystems(m_world);
            m_fixedSystems = new EcsSystems(m_world);
            m_sessionOperations = sessionOperations == null ? new List<IGameSessionOperation>() : new List<IGameSessionOperation>(sessionOperations);

            foreach (IEcsSystem system in systems)
            {
                if (system == null)
                    throw new ArgumentException("ECS session cannot contain a null system.", nameof(systems));

                if (GameSystemsComposer.IsPhysicsSystem(system.GetType()))
                    m_fixedSystems.Add(system);
                else
                    m_updateSystems.Add(system);
            }
        }

        public CancellationToken CancellationToken => m_cancellationTokenSource.Token;

        public bool IsRunning => m_isStarted && !m_isDisposed;

        public void Start()
        {
            ThrowIfDisposed();

            if (m_isStarted)
                throw new InvalidOperationException("Game session is already started.");

            m_isStarted = true;

            try
            {
                m_updateSystems.Init();
                m_fixedSystems.Init();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Tick()
        {
            if (!IsRunning)
                return;

            m_updateSystems.Run();
        }

        public void FixedTick()
        {
            if (!IsRunning)
                return;

            m_fixedSystems.Run();
        }

        public void Dispose()
        {
            if (m_isDisposed)
                return;

            m_isDisposed = true;
            m_cancellationTokenSource.Cancel();

            try
            {
                CancelSessionOperations();
            }
            finally
            {
                try
                {
                    DestroySystems();
                }
                finally
                {
                    m_world.Destroy();
                    m_cancellationTokenSource.Dispose();
                }
            }
        }

        private void DestroySystems()
        {
            if (!m_isStarted)
                return;

            try
            {
                m_fixedSystems.Destroy();
            }
            finally
            {
                m_updateSystems.Destroy();
            }
        }

        private void CancelSessionOperations()
        {
            foreach (IGameSessionOperation sessionOperation in m_sessionOperations)
                sessionOperation?.Cancel();
        }

        private void ThrowIfDisposed()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException(nameof(GameSession));
        }
    }
}
