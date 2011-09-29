
using OpenTK;

namespace WorldFramework.Controller
{
    public class Particles
    {

        private static int maxParticleCount = 1000;

        private int visibleParticleCount;

        public struct ParticleAttribute
        {
            public bool Active;
            public float Age;
            public Vector3 Direction;
            public Vector3 Position;
            public Vector4 Color;
        }
    }
}
