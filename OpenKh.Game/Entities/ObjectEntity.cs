using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System.Linq;

namespace OpenKh.Game.Entities
{
    public class ObjectEntity : IEntity
    {
        private const float TerminalFallingVelocity = 32.0f;
        private const float Gravity = 49.0f;

        public ObjectEntity(Kernel kernel, int objectId)
        {
            Kernel = kernel;
            ObjectId = objectId;
            Scaling = new Vector3(1, 1, 1);
        }

        public Kernel Kernel { get; }

        public int ObjectId { get; }

        public string ObjectName => Kernel.ObjEntries
            .FirstOrDefault(x => x.ObjectId == ObjectId)?.ModelName;

        public MeshGroup Mesh { get; private set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scaling { get; set; }

        public Vector3 Velocity { get; set; }

        public void Update(float deltaTime)
        {
            Velocity += new Vector3(0, Gravity * deltaTime, 0);
            if (Velocity.Y > TerminalFallingVelocity)
                Velocity = new Vector3(Velocity.X, TerminalFallingVelocity, Velocity.Z);

            Position -= Velocity;
        }

        public void LoadMesh(GraphicsDevice graphics)
        {
            var objEntry = Kernel.ObjEntries.FirstOrDefault(x => x.ObjectId == ObjectId);
            if (objEntry == null)
            {
                Log.Warn($"Object ID {ObjectId} not found.");
                return;
            }

            var fileName = $"obj/{objEntry.ModelName}.mdlx";

            using var stream = Kernel.DataContent.FileOpen(fileName);
            var entries = Bar.Read(stream);
            var model = entries.ForEntry(x => x.Type == Bar.EntryType.Model, Mdlx.Read);
            var texture = entries.ForEntry("tim_", Bar.EntryType.ModelTexture, ModelTexture.Read);
            Mesh = MeshLoader.FromKH2(graphics, model, texture);
        }

        public static ObjectEntity FromSpawnPoint(Kernel kernel, SpawnPoint.Entity spawnPoint) =>
            new ObjectEntity(kernel, spawnPoint.ObjectId)
            {
                Position = new Vector3(spawnPoint.PositionX, 500, -spawnPoint.PositionZ),
                Rotation = new Vector3(spawnPoint.RotationX, spawnPoint.RotationY, spawnPoint.RotationZ),
            };
    }
}
