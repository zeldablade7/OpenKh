using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;

namespace OpenKh.Engine.Physics
{
    public class Kh2PhysicsProcessor
    {
        private Coct _coct;

        public void Load(Coct coct)
        {
            _coct = coct;
        }

        public Vector3 DoesCollide(Vector3 point)
        {
            var normal = Vector3.Zero;
            DoesCollide(point, 0, ref normal);
            return normal;
        }

        public bool KeepInBound(ref Vector3 point)
        {
            return false;
        }

        private bool DoesCollide(Vector3 point, int meshGroupIndex, ref Vector3 normal)
        {
            if (meshGroupIndex == -1)
                return false;

            var meshGroup = _coct.CollisionMeshGroupList[meshGroupIndex];
            if (Intersects(point, meshGroup.BoundingBox))
            {
                var isLeaf = meshGroup.Child1 == -1;
                if (!isLeaf)
                    return DoesCollide(point, meshGroup.Child1, ref normal) ||
                        DoesCollide(point, meshGroup.Child2, ref normal) ||
                        DoesCollide(point, meshGroup.Child3, ref normal) ||
                        DoesCollide(point, meshGroup.Child4, ref normal) ||
                        DoesCollide(point, meshGroup.Child5, ref normal) ||
                        DoesCollide(point, meshGroup.Child6, ref normal) ||
                        DoesCollide(point, meshGroup.Child7, ref normal) ||
                        DoesCollide(point, meshGroup.Child8, ref normal);

                for (var i = meshGroup.CollisionMeshStart; i < meshGroup.CollisionMeshEnd; i++)
                {
                    var mesh = _coct.CollisionMeshList[i];
                    if (Intersects(point, mesh.BoundingBox))
                    {
                        for (var j = mesh.CollisionStart; j < mesh.CollisionEnd; j++)
                        {
                            var collision = _coct.CollisionList[j];
                            var bb = _coct.BoundingBoxList[collision.BoundingBoxIndex];
                            if (Intersects2(point, 100f, bb))
                            {
                                normal = _coct.PlaneList[collision.PlaneIndex].Normal;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool Intersects(Vector3 point, BoundingBoxInt16 boundingBox) =>
            point.X > boundingBox.Minimum.X &&
            point.X < boundingBox.Maximum.X &&
            -point.Y > boundingBox.Minimum.Y &&
            -point.Y < boundingBox.Maximum.Y &&
            point.Z > boundingBox.Minimum.Z &&
            point.Z < boundingBox.Maximum.Z;

        private bool Intersects2(Vector3 point, float width, BoundingBoxInt16 boundingBox) =>
            point.X - width > boundingBox.Minimum.X &&
            point.X + width < boundingBox.Maximum.X &&
            -point.Y + width > boundingBox.Minimum.Y &&
            -point.Y - width < boundingBox.Maximum.Y &&
            point.Z - width > boundingBox.Minimum.Z &&
            point.Z + width < boundingBox.Maximum.Z;
    }
}
