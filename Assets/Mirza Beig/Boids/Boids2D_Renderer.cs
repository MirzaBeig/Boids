using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids2D_Renderer : MonoBehaviour
{
    Boids2D_Simulator simulator;

    public ParticleSystem particleSystem;
    ParticleSystem.Particle[] particles;

    [Space]

    public float positionLerpSpeed = 2.0f;
    public float velocityLerpSpeed = 10.0f;

    [Space]

    [Range(0.0f, 1.0f)]
    public float typeColourBlending = 0.2f;

    public Color[] typeColours = new Color[4]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    void Start()
    {
        simulator = GetComponent<Boids2D_Simulator>();
    }

    void Update()
    {
        // Boid -> particle.

        int boidCount = simulator.boids.Count;

        ParticleSystem.MainModule particleSystem_mainModule = particleSystem.main;
        float particleSystemStartSize = particleSystem_mainModule.startSize.constant;

        // Increase size if needed.

        if (particleSystem_mainModule.maxParticles < boidCount)
        {
            particleSystem_mainModule.maxParticles = boidCount;
        }

        int maxParticles = particleSystem_mainModule.maxParticles;

        if (particles == null || particles.Length != maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        float deltaTime = Time.deltaTime;

        for (int i = 0; i < boidCount; i++)
        {
            Boid boid = simulator.boids[i];

            // Apply to particle.

            ParticleSystem.Particle particle = particles[i];

            //Vector2 smoothPosition = Vector2.MoveTowards(particle.position, boid.position, Time.deltaTime * 10.0f);

            if (particle.position == Vector3.zero)
            {
                particle.position = boid.position;
            }

            // Certain properties, like colour, are based off velocity (via the 'Colour By Speed' module).
            // If the velocity is not smoothed/interpolated, the colour will jitter if FixedUpdate executes slower.

            particle.velocity = Vector2.Lerp(particle.velocity, boid.velocity, deltaTime * velocityLerpSpeed);

            // Integrate velocity manually for particles in Update for smooth motion.
            // Position is also interpolated towards the boid position to prevent wandering errors.

            particle.position += particle.velocity * deltaTime;
            particle.position = Vector2.Lerp(particle.position, boid.position, deltaTime * positionLerpSpeed);

            particle.startSize = particleSystemStartSize;

            particle.startLifetime = 1.0f;

            particle.startColor = Color.white; // Need to start with white for GetCurrentColor to work properly (else colour = clear).

            Color32 particleColour = particle.GetCurrentColor(particleSystem);
            particleColour = Color.Lerp(particleColour, typeColours[boid.type], typeColourBlending);

            particle.startColor = particleColour;

            particle.remainingLifetime = 1.0f;

            particles[i] = particle;
        }

        particleSystem.SetParticles(particles, boidCount);
    }
}
