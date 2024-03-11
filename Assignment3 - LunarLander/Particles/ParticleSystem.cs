using Microsoft.Xna.Framework;
using Random;
using System.Collections.Generic;

namespace CS5410.Particles
{
    public class ParticleSystem
    {
        private Dictionary<long, Particle> m_particles = new Dictionary<long, Particle>();
        public Dictionary<long, Particle>.ValueCollection particles { get { return m_particles.Values; } }
        private RandomMisc m_random = new RandomMisc();

        public Vector2 m_center;
        private int m_sizeMean; // pixels
        private int m_sizeStdDev;   // pixels
        private float m_speedMean;  // pixels per millisecond
        private float m_speedStDev; // pixels per millisecond
        private float m_lifetimeMean; // milliseconds
        private float m_lifetimeStdDev; // milliseconds
        private int m_maxParticles; // New variable for maximum particles
        private bool isBurstComplete = false; // To track if the burst has completed

        public ParticleSystem(Vector2 center, int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev, int maxParticles)
        {
            m_center = center;
            m_sizeMean = sizeMean;
            m_sizeStdDev = sizeStdDev;
            m_speedMean = speedMean;
            m_speedStDev = speedStdDev;
            m_lifetimeMean = lifetimeMean;
            m_lifetimeStdDev = lifetimeStdDev;
            m_maxParticles = maxParticles;
        }

        public void shipThrust(int numberOfParticles)
        {
            for (int i = 0; i < numberOfParticles; i++)
            {
                Particle newParticle = create();
                m_particles.Add(newParticle.name, newParticle);
            }
        }

        public void shipCrash()
        {

        }

        private Particle create()
        {
            float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
            var p = new Particle(
                    m_center,
                    m_random.nextCircleVector(),
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev),
                    new Vector2(size, size),
                    new System.TimeSpan(0, 0, 0, 0, (int)(m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev))));
            return p;
        }

        public void update(GameTime gameTime)
        {
            // Update existing particles
            List<long> removeMe = new List<long>();
            foreach (Particle p in m_particles.Values)
            {
                if (!p.update(gameTime))
                {
                    removeMe.Add(p.name);
                }
            }

            // Remove dead particles
            foreach (long key in removeMe)
            {
                m_particles.Remove(key);
            }

            // Check if we should still generate particles
            if (!isBurstComplete && m_particles.Count < m_maxParticles)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (m_particles.Count >= m_maxParticles)
                    {
                        isBurstComplete = true; // Stop generating particles once limit is reached
                        break;
                    }
                    var particle = create();
                    m_particles.Add(particle.name, particle);
                }
            }
        }
    }
}
