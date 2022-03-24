using System;
using UnityEngine;

namespace MarchingCubes {

    [ExecuteAlways]
    sealed class Texture3DVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 64, 64);
        [SerializeField] float _gridScale = 4.0f / 64;
        [SerializeField] int _triangleBudget = 65536;
        [SerializeField] float _targetValue = 0;
        [SerializeField] Texture3D texture;

        #endregion

        #region Project asset references

        [SerializeField] ComputeShader _volumeCompute = null;
        [SerializeField] ComputeShader _builderCompute = null;

        #endregion

        #region Private members

        int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

        ComputeBuffer _voxelBuffer;
        MeshBuilder _builder;

        #endregion

        #region MonoBehaviour implementation

        void OnEnable()
        {
            _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
            _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
        }

        void OnDestroy()
        {
            _voxelBuffer.Dispose();
            _builder.Dispose();
        }

        void Update()
        {
            _volumeCompute.SetInts("Dims", _dimensions);
            _volumeCompute.SetBuffer(0, "Voxels", _voxelBuffer);
            _volumeCompute.SetTexture(0, "Tex", texture);
            _volumeCompute.DispatchThreads(0, _dimensions);

            // Isosurface reconstruction
            _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
            GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
        }

        #endregion
    }

} // namespace MarchingCubes