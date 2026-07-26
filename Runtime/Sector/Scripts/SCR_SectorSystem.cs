using System;
using UnityEngine;

namespace Core.Sector
{
    using static CoreUtility;

    public class SectorSystem : MonoBehaviour
    {
        private enum GridSize : int 
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048, 
            _4096 = 4096 
        }

        private enum TileCount : int 
        { 
            _2 = 2,
            _4 = 4,
            _8 = 8,
            _16 = 16,
            _32 = 32
        }

        private const int NEIGHBOR_COUNT = 8;
        private const float SHIFT_THRESHOLD = 512;

        public static Vector3 OriginOffset { get; private set; }
        public static event Action<Vector3> OnOriginShift = null;

        [Header("_")]
        [SerializeField, Info("If not visible please generate sectors.\nasdsadasda test ediyoruz")] private bool showGizmos = false;
        [SerializeField] private GridSize gridSize = GridSize._4096;
        [SerializeField] private TileCount tileCount = TileCount._4;

        private static Sector[] sectors;
        private static Transform target = null;
        private static Transform root = null;
        private static int[] neighbors;
        private static readonly (int dx, int dy)[] directions =
        {
            (0, 1),     // N
            (1, 1),     // NE
            (1, 0),     // E
            (1, -1),    // SE
            (0, -1),    // S
            (-1, -1),   // SW
            (-1, 0),    // W
            (-1, 1)     // NW
        };
        private static int totalSize;
        private static int sectorSize;
        private static int sectorCount;
        private static int gridCount;
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            OnOriginShift = null;
            OriginOffset = Vector3.zero;

            target = null;
            root = null;

            sectors = null;
            neighbors = null;

            isInitialized = false;
        }

        private void Awake() => Initialize();
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (root == null)
            {
                return;
            }

            Vector3 position = target.position;

            if (position.sqrMagnitude > SHIFT_THRESHOLD * SHIFT_THRESHOLD)
            {
                ShiftOrigin(position);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos)
            {
                return;
            }

            if (!isInitialized)
            {
                return;
            }

            for (int i = 0; i < sectorCount; i++)
            {
                Sector sector = GetSector(i);
                Vector3 center = sector.Position - OriginOffset;

                Gizmos.color = COLOR_BLUE;
                Gizmos.DrawRay(center, Vector3.up * sector.Size.x);
                Gizmos.DrawWireSphere(center, 16);

                Gizmos.color = COLOR_YELLOW;
                Gizmos.DrawWireCube(center, sector.Size);
            }
        }
#endif

        private void ShiftOrigin(Vector3 offset)
        {
            OriginOffset += offset;

            root.position -= offset;
            target.position = Vector3.zero;

            OnOriginShift?.Invoke(offset);
        }
        private static void ValidateID(int id)
        {
            if (id >= sectorCount || id < 0)
            {
                throw new ArgumentOutOfRangeException("sector id is not exists!");
            }
        }

        [ContextMenu("Generate")]
        public void Initialize()
        {
            totalSize = (int)gridSize;
            gridCount = (int)tileCount;

            sectorSize = totalSize / gridCount;
            sectorCount = gridCount * gridCount;

            sectors = new Sector[sectorCount];
            neighbors = new int[sectorCount * NEIGHBOR_COUNT];

            GenerateSectors();
        }

        private static void GenerateSectors()
        {
            for (int y = 0; y < gridCount; y++)
            {
                for (int x = 0; x < gridCount; x++)
                {
                    int index = y * gridCount + x;
                    float width = (sectorSize * x) + (sectorSize * 0.5f);
                    float height = (sectorSize * y) + (sectorSize * 0.5f);

                    Vector3 position = new(width, 0, height);
                    Vector3 size = new(sectorSize, 1, sectorSize);

                    sectors[index] = new(index, position, size, new());
                }
            }

            GenerateNeighbors();
            isInitialized = true;
        }
        public static Sector GetSector(Vector3 position)
        {
            for (int i = 0; i < sectorCount; i++)
            {
                Sector sector = GetSector(i);

                if (IsInsideSector(sector, position))
                {
                    return sector;
                }
            }

            Debug.LogError($"sector not found around [{position}]");
            return null;
        }
        public static Sector GetSector(int x, int y)
        {
            if (x >= gridCount || x < 0 || y >= gridCount || y < 0)
            {
                throw new ArgumentOutOfRangeException("sector x,y out of bounds!");
            }

            return sectors[y * gridCount + x];
        }
        public static Sector GetSector(int id)
        {
            ValidateID(id);

            return sectors[id];
        }

        private static void GenerateNeighbors()
        {
            for (int y = 0; y < gridCount; y++)
            {
                for (int x = 0; x < gridCount; x++)
                {
                    int index = y * gridCount + x;
                    int baseOffset = index * NEIGHBOR_COUNT;

                    for (int d = 0; d < NEIGHBOR_COUNT; d++)
                    {
                        int nx = x + directions[d].dx;
                        int ny = y + directions[d].dy;

                        bool valid = nx >= 0 && nx < gridCount && ny >= 0 && ny < gridCount;

                        neighbors[baseOffset + d] = valid ? ny * gridCount + nx : -1;
                    }
                }
            }
        }
        public static Sector GetNeighbor(int id, Direction direction)
        {
            ValidateID(id);

            int neighborID = neighbors[id * NEIGHBOR_COUNT + (int)direction];

            return neighborID == -1 ? null : sectors[neighborID];
        }
        public static Sector GetNeighbor(Sector sector, Direction direction) => GetNeighbor(sector.ID, direction);
        public static int GetNeighbors(int id, Sector[] buffer)
        {
            ValidateID(id);

            if (buffer.Length < NEIGHBOR_COUNT)
            {
                throw new ArgumentException($"buffer must be at least {NEIGHBOR_COUNT} length!");
            }

            int baseOffset = id * NEIGHBOR_COUNT;
            int count = 0;

            for (int d = 0; d < NEIGHBOR_COUNT; d++)
            {
                int neighborID = neighbors[baseOffset + d];

                if (neighborID != -1)
                {
                    buffer[count] = sectors[neighborID];

                    count++;
                }
            }

            return count;
        }
        public static Sector[] GetNeighbors(int id)
        {
            Sector[] buffer = new Sector[NEIGHBOR_COUNT];

            int count = GetNeighbors(id, buffer);

            if (count == NEIGHBOR_COUNT)
            {
                return buffer;
            }

            Array.Resize(ref buffer, count);
            return buffer;
        }
        public static bool AreNeighbors(int a, int b)
        {
            int baseOffset = a * NEIGHBOR_COUNT;

            for (int d = 0; d < NEIGHBOR_COUNT; d++)
            {
                if (neighbors[baseOffset + d] == b)
                {
                    return true;
                }
            }

            return false;
        }

        public static Direction? GetDirectionTo(int idA, int idB)
        {
            int baseOffset = idA * NEIGHBOR_COUNT;

            for (int d = 0; d < NEIGHBOR_COUNT; d++)
            {
                if (neighbors[baseOffset + d] == idB)
                {
                    return (Direction)d;
                }
            }

            return null;
        }

        public static bool IsInsideSector(int id, Vector3 position) => IsInsideSector(GetSector(id), position);
        private static bool IsInsideSector(Sector sector, Vector3 position)
        {
            Vector3 half = sector.Size * 0.5f;
            Vector3 center = sector.Position - OriginOffset;

            return position.x >= center.x - half.x &&
                   position.x <= center.x + half.x &&
                   position.y >= center.y - half.y &&
                   position.y <= center.y + half.y &&
                   position.z >= center.z - half.z &&
                   position.z <= center.z + half.z;
        }

        public static void SetTarget(Transform transform) => target = transform == null ? throw new ArgumentNullException(nameof(transform)) : transform;

        public static Transform GetRoot() => root;
        public static void SetRoot(Transform transform) => root = transform == null ? throw new ArgumentNullException(nameof(transform)) : transform;
    }
}
