using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using System.Collections.Concurrent;

// An agent can be represented by it's position and velocity.

// Fun fact 1: Boid is a portmanteau* of bird and '-oid' (as in, android).
// The name 'boid' corresponds to a shortened version of 'bird-oid object', which refers to a bird-like object.

// Portmanteau: A word blending the sounds and combining the meanings of two others, for example motel (from 'motor' and 'hotel') or brunch (from 'breakfast' and 'lunch').

// Fun fact 2: Boids are an example of a particle system, where agents are represented as position and velocity.
// Fun fact 3: Boids are an example of an n-body simulation, where the behaviour of each agent/particle is influenced by the behaviour of every other agent.

// Fun fact 4: Boids demonstrate emergent behaviour, where complex global behaviour emerges from simple localized rules.
// Fun fact 5: Boids are an artificial life program, developed by Craig Reynolds in 1986 to simulate the flocking behaviour of birds (and fish, etc...).

public struct Boid
{
    // Position: where it is.
    // Velocity: movement over time.

    public Vector2 position;
    public Vector2 velocity;

    public int type;

    public float wanderAngle;

    //public void AddForce(Vector2 force, float deltaTime)
    //{
    //    position += force * deltaTime;
    //}
    //public void ApplyFriction(float friction, float deltaTime)
    //{
    //    velocity *= 1.0f - friction * deltaTime;
    //}
    //public void Update(float deltaTime)
    //{
    //    position += velocity * deltaTime;
    //}
}

public class Boids2D_Simulator : MonoBehaviour
{
    // Current (spawned) list of boids.
    // Read-only (public get, private set).

    public List<Boid> boids { get; private set; }

    [Header("Bounds")]

    public Vector2 bounds = Vector2.one;

    [Space]

    public float boundsScale = 10.0f;
    public float boundingForce = 1.0f;

    [Space]

    public int typeCount = 1;

    [Header("Spatial Partioning")]

    public float cellSize = 2.0f;

    [Header("Speed")]

    public float drag = 0.1f;

    [Space]

    public float minSpeed = 1.0f;
    public float maxSpeed = 2.0f;

    [Header("Flocking")]

    public float cohesionRadius = 1.0f;
    public float separationRadius = 0.5f;
    public float alignmentRadius = 1.0f;

    [Space]

    public float cohesionForce = 1.0f;
    public float separationForce = 1.0f;
    public float alignmentForce = 1.0f;

    [Header("Wander")]

    public float wanderRadius = 1.0f;
    public float wanderDistance = 2.0f;

    [Range(0.0f, 360.0f)]
    public float wanderAngleJitter = 45.0f;

    public float wanderForce = 1.0f;

    [Header("Debug Spawn")]

    public int addBoidCount = 10; // Even though this should be uint, it can require some annoying casting to int later, so we'll stick with int from the get-go.
    public float addBoidRandomSpeedScale = 1.0f;

    [Header("Debug Visuals")]

    public bool debugBase;

    [Space]

    public bool debugText;
    public bool debugWander;

    [Space]

    public float debugRadius = 0.1f;

    [Space]

    public Color debugColour = Color.white;

    void Awake()
    {
        boids = new();
    }

    void Start()
    {

    }

    public void AddBoids(int count)
    {
        // Using half bounds because we want to spawn boids within the bounding 'extents', centered at the origin.

        float halfBoundsScale = boundsScale / 2.0f;

        for (int i = 0; i < count; i++)
        {
            // Random position within bounds.

            Vector2 randomPosition = new Vector2(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y)) * halfBoundsScale;

            // Random between [0.0, 1.0].

            Vector2 randomVelocity = new(Random.value, Random.value);

            // Remap to [-1.0, 1.0].

            randomVelocity = (randomVelocity * 2.0f) - Vector2.one;

            // Scale by speed.

            randomVelocity *= addBoidRandomSpeedScale;

            // Create boid and add to list.

            Boid boid = new()
            {
                position = randomPosition,
                velocity = randomVelocity,

                type = Random.Range(0, typeCount),
            };

            boids.Add(boid);
        }
    }

    public void RemoveBoids(int count)
    {
        if (count > boids.Count)
        {
            count = boids.Count;
        }

        boids.RemoveRange(0, count);
    }

    // Angles are in RADIANS.

    Vector2 Wander(Vector2 position, Vector2 forward, ref float currentWanderAngle, float wanderDistance, float wanderRadius, float wanderAngleJitter)
    {
        // Increment wander angle by random value.

        currentWanderAngle += Random.Range(-wanderAngleJitter, wanderAngleJitter);

        // Calculate offset on wander circle.

        Vector2 circleOffset = new Vector2(Mathf.Cos(currentWanderAngle), Mathf.Sin(currentWanderAngle)) * wanderRadius;

        // Position wander target in front of boid at a specified distance.

        Vector2 wanderTarget = position + (forward.normalized * wanderDistance);

        // Add offset so wander target that's currently some distance directly in front of the boid is moved to some point on a circle or sphere (2D/3D).

        wanderTarget += circleOffset;

        Vector2 offsetToTarget = wanderTarget - position;

        return offsetToTarget;
    }

    //bool IsWithinBounds(Vector2 position, Vector2 origin, Vector2 extents)
    //{
    //    Vector2 lowerBound = origin - extents;
    //    Vector2 upperBound = origin + extents;

    //    for (int i = 0; i < 2; ++i)
    //    {
    //        if (position[i] < lowerBound[i] || position[i] > upperBound[i])
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}

    Vector2 GetBoundingForce(Vector2 position, Vector2 origin, Vector2 extents, float forceScale)
    {
        Vector2 lowerBound = origin - extents;
        Vector2 upperBound = origin + extents;

        Vector2 force = Vector2.zero;

        for (int i = 0; i < 2; ++i)
        {
            if (position[i] < lowerBound[i])
            {
                force[i] = forceScale;
            }
            else if (position[i] > upperBound[i])
            {
                force[i] = -forceScale;
            }
        }

        return force;
    }

    //Vector2Int GetCellPosition(Vector2 position)
    //{
    //    return new Vector2Int((int)(position.x / cellSize), (int)(position.y / cellSize));
    //}

    void Update()
    {
        // Input.

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddBoids(addBoidCount);
        }
    }

    // Boids don't need to update every rendered frame, so we'll use FixedUpdate instead of Update.

    void FixedUpdate()
    {
        // CACHE IS KING.
        // Cache and pre-calculate values.

        float deltaTime = Time.deltaTime;
        Vector3 transformPosition = transform.position;

        Vector2 scaledBoundingExtents = bounds * (boundsScale / 2.0f);

        int boidCount = boids.Count;

        float wanderAngleJitterRad = Mathf.Deg2Rad * wanderAngleJitter;

        Color wanderDebugColour = Color.cyan;
        wanderDebugColour.a = debugColour.a;

        for (int i = 0; i < boidCount; i++)
        {
            Boid boid = boids[i];

            // Accumulate forces.

            Vector2 force = Vector2.zero;

            // Bounding force.

            force += GetBoundingForce(boid.position, transformPosition, scaledBoundingExtents, boundingForce);

            // --FLOCKING START--
            // Flocking is three behaviours: cohesion, separation, and alignment.

            Vector2 averagePositionForCohesion = Vector2.zero;
            Vector2 averageVelocityForAlignment = Vector2.zero;

            uint otherBoidsInCohesionRadius = 0;
            uint otherBoidsInAlignmentRadius = 0;

            Vector2 separation = Vector2.zero;

            for (int j = 0; j < boidCount; j++)
            {
                // Skip self.

                if (i == j)
                {
                    continue;
                }

                Boid otherBoid = boids[j];

                Vector2 offsetToOtherBoid = otherBoid.position - boid.position;
                float distanceToOtherBoid = Vector3.Magnitude(offsetToOtherBoid);

                Vector2 directionToOtherBoid = offsetToOtherBoid / distanceToOtherBoid;

                // Separation.

                if (distanceToOtherBoid < separationRadius)
                {
                    // Inverse normalized distance (closer = stronger force).

                    float inverseNormalizedDistance = 1.0f - (distanceToOtherBoid / separationRadius);
                    separation -= directionToOtherBoid * inverseNormalizedDistance;
                }

                // Only apply cohesion and alignment if same type.

                if (boid.type == otherBoid.type)
                {
                    // Cohesion.

                    if (distanceToOtherBoid < cohesionRadius)
                    {
                        otherBoidsInCohesionRadius++;
                        averagePositionForCohesion += otherBoid.position;
                    }

                    // Alignment.

                    if (distanceToOtherBoid < alignmentRadius)
                    {
                        otherBoidsInAlignmentRadius++;
                        averageVelocityForAlignment += otherBoid.velocity;
                    }
                }
            }

            // Separation.

            force += separation * separationForce;

            // Cohesion.

            if (otherBoidsInCohesionRadius > 0)
            {
                averagePositionForCohesion /= otherBoidsInCohesionRadius;
                Vector2 offsetToAveragePosition = averagePositionForCohesion - boid.position;

                float distanceToAveragePosition = offsetToAveragePosition.magnitude;
                Vector2 directionToAveragePosition = offsetToAveragePosition / distanceToAveragePosition;

                Vector2 cohesion = directionToAveragePosition * cohesionForce;

                force += cohesion;
            }

            // Alignment.

            if (otherBoidsInAlignmentRadius > 0)
            {
                averageVelocityForAlignment /= otherBoidsInAlignmentRadius;
                Vector2 offsetToAverageVelocity = averageVelocityForAlignment - boid.velocity;

                Vector2 alignment = offsetToAverageVelocity * alignmentForce;

                force += alignment;
            }

            // --FLOCKING END--

            // Wander.

            Vector2 wander = Wander(boid.position, boid.velocity.normalized, ref boid.wanderAngle, wanderDistance, wanderRadius, wanderAngleJitterRad) * wanderForce;

            if (debugWander)
            {
                // Draw unscaled ray.

                Debug.DrawRay(boid.position, wander, wanderDebugColour);
            }

            force += wander;

            // Apply friction.

            boid.velocity *= 1.0f - drag * deltaTime;

            // Update velocity with simple Euler integration (v += f * t).

            boid.velocity += force * deltaTime;

            // Clamp velocity to min/max speed.

            float currentSpeed = boid.velocity.magnitude;
            Vector2 currentDirection = boid.velocity.normalized;

            if (currentSpeed < minSpeed)
            {
                boid.velocity = currentDirection * minSpeed;
            }
            else if (currentSpeed > maxSpeed)
            {
                boid.velocity = currentDirection * maxSpeed;
            }

            // Update position the same way (p += v * t).

            boid.position += boid.velocity * deltaTime;

            boids[i] = boid;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Boid Count: " + boids.Count);

        if (!debugText)
        {
            return;
        }

        for (int i = 0; i < boids.Count; i++)
        {
            Boid boid = boids[i];

            Vector2 screenPosition = Camera.main.WorldToScreenPoint(boid.position);

            GUI.Label(new Rect(screenPosition.x, Screen.height - screenPosition.y, 200, 20), $"#{i}, type {boid.type}");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, bounds * boundsScale);

        if (!Application.isPlaying)
        {
            return;
        }

        Color colour = debugColour;
        Gizmos.color = colour;

        foreach (Boid boid in boids)
        {
            if (debugBase)
            {
                Gizmos.DrawSphere(boid.position, debugRadius);
            }

            if (boid.velocity != Vector2.zero)
            {
                if (debugBase)
                {
                    Vector2 boidForward = boid.velocity.normalized;
                    Gizmos.DrawRay(boid.position, boidForward);

                    if (debugWander)
                    {
                        // Draw wander circle.

                        colour = Color.green;

                        colour.a = debugColour.a;
                        Gizmos.color = colour;

                        Gizmos.DrawWireSphere(boid.position + (boidForward * wanderDistance), wanderRadius);
                    }
                }
            }
        }
    }
}
