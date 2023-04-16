using BepuPhysics;
using BepuPhysics.Collidables;

namespace MtgWeb.Core.Physics;

public class RigidBody
{
    public bool IsStatic = false;

    public RigidPose Pose = RigidPose.Identity;
    public BodyVelocity Velocity;
    public Collidable Collidable;

    public BodyDescription Body;
    public BodyHandle BodyHandle;
    public IShape[] Shapes;
    public IShape Shape;
}