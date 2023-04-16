using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Silk.NET.Maths;

namespace MtgWeb.Core.Physics;

public class PhysicsWorld : IDisposable
{
    public Simulation Simulation;
    private BufferPool _bufferPool;

    public PhysicsWorld()
    {
        _bufferPool = new BufferPool();
        Simulation = Simulation.Create(
            _bufferPool,
            new DemoNarrowPhaseCallbacks(),
            new DemoPoseIntegratorCallbacks(-9.81f * Vector3.UnitY),
            new SolveDescription(8, 1)
        );
    }

    public void Add(Scene scene)
    {
        foreach (var entity in scene.Root)
        {
            var staticBody = entity.StaticBody;
            if (staticBody != null)
            {
                if (staticBody.Shape == null)
                {
                    Console.WriteLine($"No shape on entity: {entity.Name}");
                    continue;
                }

                staticBody.ShapeId = staticBody.Shape switch
                {
                    Sphere sphere => Simulation.Shapes.Add(sphere),
                    Capsule capsule => Simulation.Shapes.Add(capsule),
                    Box box => Simulation.Shapes.Add(box),
                    _ => throw new Exception($"Not supported shape on {entity.Name}")
                };


                var rotation = entity.Transform.Quaternion;
                var position = entity.Transform.Position + Vector3D.Transform(staticBody.Offset, rotation);
                staticBody.Description = new StaticDescription(
                    position.ToSystem(),
                    rotation.ToSystem(),
                    staticBody.ShapeId
                );

                Simulation.Statics.Add(staticBody.Description);
            }

            var rigidBody = entity.RigidBody;
            if (rigidBody != null)
            {
                rigidBody.BodyHandle = Simulation.Bodies.Add(rigidBody.Body);
                switch (rigidBody.Shape)
                {
                    case Box box:
                        rigidBody.Collidable.Shape = Simulation.Shapes.Add(box);
                        break;
                    case Capsule capsule:
                        rigidBody.Collidable.Shape = Simulation.Shapes.Add(capsule);
                        break;
                }
            }
        }
    }

    public void Dispose()
    {
        Simulation.Dispose();
    }
}