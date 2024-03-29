# Boids

An implementation of [Craig Reynold's 1986 artificial life program](https://www.red3d.com/cwr/boids/) for Unity.

For a live/WebGL demo, please see [here](https://github.com/MirzaBeig/Boids-WebGL/tree/main).

Current behaviours:

- **Flocking**:
  - **Cohesion, Separation, Alignment.**
    - *Cohesion*: Steer towards local average position (group center).
    - *Separation*: Steer away from individual boids nearby.
    - *Alignment*: Steer towards local average velocity (group heading).   
- **Type (visualized as colour)**:
  - Flocking *cohesion* and *alignment* filtered to same-type boids.
  - Separation still applies to all (to prevent inter-particle/boid collisions).
- **Wander**:
  - Each boid has a tendency to stray randomly (with order over time).
- **Bounds**:
  - There are no *strict* bounds, boids can simulate anywhere.
  - A 'soft' bounding force contains them on the screen (which you can set to 0.0).

https://github.com/MirzaBeig/Boids/assets/37354140/057994d4-3c83-4017-bc20-bf1c74aa31d5

The simulator is independent from the renderer, where the latter uses Unity's actual particle system to render all agents efficiently in one go.

## License

Pending... (will update later)
