using UnityEngine;
using UnityEngine.Assertions;

namespace MeshCreation
{
    public sealed class MeshCreatorContext
    {
        private MeshCreator _meshCreator;

        public MeshCreatorContext(MeshCreator target)
        {
            Set(target);
        }

        public void Set(MeshCreator target)
        {
            Assert.IsNotNull(target);
            _meshCreator = target;
        }

        public Mesh CreateMesh(in MeshData data)
        {
            return _meshCreator.CreateMesh(data);
        }
    }
}