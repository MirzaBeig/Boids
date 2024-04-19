using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids2D_GroupForceFields : MonoBehaviour
{
    [System.Serializable]
    public struct GroupForceField
    {
        public Vector3 targetPosition;
        public Vector3 boidsAverageVelocity;

        public ParticleSystemForceField psForceField;

        public readonly Transform Transform
        {
            get { return psForceField.transform; }
        }
        public readonly GameObject GameObject
        {
            get { return psForceField.gameObject; }
        }

        public readonly void LerpToTargetPosition(float t, float teleportThreshold)
        {
            float distanceToTarget = Vector3.Distance(Transform.position, targetPosition);

            if (distanceToTarget > teleportThreshold)
            {
                Transform.position = targetPosition;
            }
            else
            {
                Transform.position = Vector3.Lerp(Transform.position, targetPosition, t);
            }
        }
    }

    Boids2D_Simulator simulator;
    List<GroupForceField> forceFields;

    public ParticleSystem[] typeParticleSystems;

    [Space]

    public int maxGroupCount = 32;
    public int startingForceFieldCapacity = 32;

    [Space]

    public float forceFieldLerpSpeed = 10.0f;
    public float forceFieldTeleportThreshold = 0.0f;

    [Space]

    public ParticleSystemForceField forceFieldPrefab;

    void Start()
    {
        simulator = GetComponent<Boids2D_Simulator>();
        forceFields = new List<GroupForceField>();

        for (int i = 0; i < startingForceFieldCapacity; i++)
        {
            ParticleSystemForceField forceField = Instantiate(forceFieldPrefab);

            GroupForceField groupForceField = new()
            {
                psForceField = forceField
            };

            groupForceField.GameObject.SetActive(false);

            forceFields.Add(groupForceField);
        }
    }

    void Update()
    {
        for (int i = 0; i < forceFields.Count; i++)
        {
            GroupForceField forceField = forceFields[i];

            if (!forceField.GameObject.activeSelf)
            {
                continue;
            }

            forceField.LerpToTargetPosition(Time.deltaTime * forceFieldLerpSpeed, forceFieldTeleportThreshold);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < forceFields.Count; i++)
        {
            GroupForceField forceField = forceFields[i];
            forceField.GameObject.SetActive(false);
        }

        for (int i = 0; i < typeParticleSystems.Length; i++)
        {
            typeParticleSystems[i].externalForces.RemoveAllInfluences();
        }

        for (int i = 0; i < Mathf.Min(forceFields.Count, simulator.boidGroups.Count); i++)
        {
            List<int> group = simulator.boidGroups[i];
            int groupCount = group.Count;

            Vector2 averagePosition = Vector2.zero;
            Vector2 averageVelocity = Vector2.zero;

            Boid firstBoidInGroup = simulator.boids[group[0]];
            int groupType = firstBoidInGroup.type;

            for (int j = 0; j < groupCount; j++)
            {
                Boid boid = simulator.boids[group[j]];

                averagePosition += boid.position;
                averageVelocity += boid.velocity;
            }

            averagePosition /= groupCount;
            averageVelocity /= groupCount;

            // Force field.

            GroupForceField forceField = forceFields[i];

            forceField.GameObject.SetActive(true);
            forceField.targetPosition = averagePosition;

            float normalizedGroupCount = groupCount / (float)maxGroupCount;

            ParticleSystem.MinMaxCurve gravity = forceField.psForceField.gravity;

            gravity.constant = forceFieldPrefab.gravity.constant * normalizedGroupCount;
            forceField.psForceField.endRange = forceFieldPrefab.endRange * normalizedGroupCount;

            if (gravity.constant > forceFieldPrefab.gravity.constant)
            {
                gravity.constant = forceFieldPrefab.gravity.constant;
            }

            if (forceField.psForceField.endRange > forceFieldPrefab.endRange)
            {
                forceField.psForceField.endRange = forceFieldPrefab.endRange;
            }

            //gravity.constant = Mathf.Min(gravity.constant, forceFieldPrefab.gravity.constant);
            //forceField.psForceField.endRange = Mathf.Min(forceField.psForceField.endRange, forceFieldPrefab.endRange);

            forceField.psForceField.gravity = gravity;
            forceField.boidsAverageVelocity = averageVelocity;

            forceFields[i] = forceField;

            // Particle system.

            ParticleSystem particleSystem = typeParticleSystems[groupType];
            particleSystem.externalForces.AddInfluence(forceField.psForceField);
        }

        for (int i = 0; i < forceFields.Count; i++)
        {
            GroupForceField forceField = forceFields[i];

            if (forceField.GameObject.activeSelf)
            {
                Debug.DrawRay(forceField.Transform.position, forceField.boidsAverageVelocity.normalized * 2.0f, Color.yellow);
            }
        }
    }
}
