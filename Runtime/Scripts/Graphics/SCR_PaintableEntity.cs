using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Graphics
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class PaintableEntity : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool showGizmosCells = false;
        [SerializeField] private bool showGizmosPoints = false;

        [Header("_")]
        [SerializeField, Required] private ComputeShader computeShader = null;
        [SerializeField, Required] private Material targetMaterial;

        [Header("_")]
        [SerializeField] private PaintResolution resolution = PaintResolution._32;

        private MeshFilter thisFilter = null;
        private MeshRenderer thisRenderer = null;
        private Material thisMaterial = null;
        private Transform thisTransform = null;
        private RenderTexture thisTexture = null;
        private Bounds thisBounds = default;
        private int thisKernel = 0;
        private static readonly int ResultID = Shader.PropertyToID("Result");
        private static readonly int PositionID = Shader.PropertyToID("Position");
        private static readonly int DirectionID = Shader.PropertyToID("Direction");
        private static readonly int RadiusID = Shader.PropertyToID("Radius");
        private static readonly int LengthID = Shader.PropertyToID("Length");
        private static readonly int StrengthID = Shader.PropertyToID("Strength");
        private static readonly int ShapeID = Shader.PropertyToID("Shape");
        private static readonly int BlendID = Shader.PropertyToID("Blend");
        private static readonly int ResolutionID = Shader.PropertyToID("Resolution");
        private static readonly int CellID = Shader.PropertyToID("Cell");
        private static readonly int PaintMapID = Shader.PropertyToID("_PaintMap");
        private static readonly int PaintBoundsMin = Shader.PropertyToID("_PaintBoundsMin");
        private static readonly int PaintBoundsSize = Shader.PropertyToID("_PaintBoundsSize");

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            thisFilter = GetComponent<MeshFilter>();
            thisRenderer = GetComponent<MeshRenderer>();

            if (thisFilter.sharedMesh == null)
            {
                throw new MissingReferenceException($"PaintableEntity.Awake() [{nameof(PaintableEntity)}]");
            }

            if (computeShader == null)
            {
                throw new MissingReferenceException($"PaintableEntity.Awake() [{nameof(PaintableEntity)}]");
            }

            if (targetMaterial == null)
            {
                throw new MissingReferenceException($"PaintableEntity.Awake() [{nameof(PaintableEntity)}]");
            }

            Create();

            thisMaterial = new Material(targetMaterial);
            thisRenderer.sharedMaterial = thisMaterial;

            thisMaterial.SetTexture(PaintMapID, thisTexture);
            thisMaterial.SetVector(PaintBoundsMin, thisBounds.min);
            thisMaterial.SetVector(PaintBoundsSize, thisBounds.size);
        }
        private void OnDestroy() => Destroy();
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showGizmosCells && !showGizmosPoints)
            {
                return;
            }

            if (thisFilter == null)
            {
                thisFilter = GetComponent<MeshFilter>();
            }

            if (thisFilter.sharedMesh == null)
            {
                Debug.LogWarning("PaintableEntity() no mesh assigned!");
                return;
            }

            int resolution = (int)this.resolution;

            Gizmos.matrix = transform.localToWorldMatrix;

            Bounds bounds = thisFilter.sharedMesh.bounds;

            Vector3 size = bounds.size / resolution;
            Vector3 start = bounds.min;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 center = start + new Vector3((x + 0.5f) * size.x, (y + 0.5f) * size.y, (z + 0.5f) * size.z);

                        if (showGizmosCells)
                        {
                            Gizmos.color = COLOR_BLUE;
                            Gizmos.DrawWireCube(center, size);
                        }

                        if (showGizmosPoints)
                        {
                            Gizmos.color = COLOR_RED;
                            Gizmos.DrawWireSphere(center, size.x * 0.1f);
                        }
                    }
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
        private Vector3 LocalPositionToUVW(Vector3 position)
        {
            Vector3 min = thisBounds.min;
            Vector3 size = thisBounds.size;

            float x = Mathf.Clamp01(Mathf.InverseLerp(min.x, min.x + size.x, position.x));
            float y = Mathf.Clamp01(Mathf.InverseLerp(min.y, min.y + size.y, position.y));
            float z = Mathf.Clamp01(Mathf.InverseLerp(min.z, min.z + size.z, position.z));

            return new(x, y, z);
        }
        private Vector3 LocalDirectionToUVW(Vector3 direction)
        {
            return new Vector3
            (
                direction.x / thisBounds.size.x,
                direction.y / thisBounds.size.y,
                direction.z / thisBounds.size.z
            ).normalized;
        }
        private Vector3 GetShapeExtent(in PaintParams @params)
        {
            float radius = @params.Radius;

            return @params.Shape switch
            {
                PaintShape.SPHERE => new
                (
                    radius, 
                    radius, 
                    radius
                ),
                PaintShape.CAPSULE => new
                (
                    @params.Length + radius,
                    @params.Length + radius,
                    @params.Length + radius
                ),
                _ => new
                (
                    radius, 
                    radius, 
                    radius
                )
            };
        }

        public void RecalculateBounds()
        {
            thisBounds = thisFilter.sharedMesh.bounds;
            thisMaterial.SetVector(PaintBoundsMin, thisBounds.min);
            thisMaterial.SetVector(PaintBoundsSize, thisBounds.size);
        }

        private void Create()
        {
            thisKernel = computeShader.FindKernel("CSMain");
            thisBounds = thisFilter.sharedMesh.bounds;

            int resolution = (int)this.resolution;

            thisTexture = new(resolution, resolution, 0, RenderTextureFormat.RFloat)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = resolution,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            thisTexture.Create();

            Clear();
        }
        private void Destroy()
        {
            if (thisTexture != null)
            {
                thisTexture.Release();
                thisTexture = null;
            }
        }
        public void Paint(Vector3 worldPosition, Vector3 worldDirection, in PaintParams @params)
        {
            if (thisTexture == null)
            {
                return;
            }

            int resolution = (int)this.resolution;

            Vector3 localPosition = thisTransform.InverseTransformPoint(worldPosition);
            Vector3 localDirection = thisTransform.InverseTransformDirection(worldDirection).normalized;
            Vector3 uvwPosition = LocalPositionToUVW(localPosition);
            Vector3 uvwDirection = LocalDirectionToUVW(localDirection);

            computeShader.SetTexture(thisKernel, ResultID, thisTexture);
            computeShader.SetVector(PositionID, uvwPosition);
            computeShader.SetVector(DirectionID, uvwDirection);
            computeShader.SetFloat(RadiusID, @params.Radius);
            computeShader.SetFloat(LengthID, @params.Length);
            computeShader.SetFloat(StrengthID, @params.Strength);
            computeShader.SetInt(ShapeID, (int)@params.Shape);
            computeShader.SetInt(BlendID, (int)@params.Blend);
            computeShader.SetInt(ResolutionID, resolution);

            Vector3 shapeExtent = GetShapeExtent(@params);

            Vector3Int minCellSize = Vector3Int.Max
            (
                Vector3Int.zero,
                new
                (
                    Mathf.FloorToInt((uvwPosition.x - shapeExtent.x) * resolution) - 1,
                    Mathf.FloorToInt((uvwPosition.y - shapeExtent.y) * resolution) - 1,
                    Mathf.FloorToInt((uvwPosition.z - shapeExtent.z) * resolution) - 1
                )
            );

            Vector3Int maxCellSize = Vector3Int.Min
            (
                new(resolution - 1, resolution - 1, resolution - 1),
                new
                (
                    Mathf.CeilToInt((uvwPosition.x + shapeExtent.x) * resolution) + 1,
                    Mathf.CeilToInt((uvwPosition.y + shapeExtent.y) * resolution) + 1,
                    Mathf.CeilToInt((uvwPosition.z + shapeExtent.z) * resolution) + 1
                )
            );

            computeShader.SetInts(CellID, minCellSize.x, minCellSize.y, minCellSize.z);

            Vector3Int regionSize = maxCellSize - minCellSize + Vector3Int.one;
            int gx = Mathf.CeilToInt(regionSize.x / 8f);
            int gy = Mathf.CeilToInt(regionSize.y / 8f);
            int gz = Mathf.CeilToInt(regionSize.z / 8f);
            computeShader.Dispatch(thisKernel, gx, gy, gz);
        }
        public void Clear()
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = thisTexture;

            GL.Clear(false, true, Color.clear);

            RenderTexture.active = active;
        }
    }
}