using UnityEngine;

public class TetraShatter : MonoBehaviour
{
    [Header("Trigger (optional test)")]
    public KeyCode testKey = KeyCode.K;

    [Header("Shard Lifetime")]
    [Min(0.01f)] public float shardLifetime = 1.0f;

    [Header("Tetrahedron Shape")]
    [Tooltip("Scales how far the tetra apex is pushed along the triangle normal. " +
             "Computed as avgEdgeLength * tetraHeightFactor.")]
    [Range(0.05f, 2.0f)] public float tetraHeightFactor = 0.35f;

    [Header("Shard Physics")]
    public float impulseStrength = 2.5f;
    public Vector3 impulseBias = Vector3.up; // small upward pop
    public float randomTorque = 5f;
    [Min(0.0001f)] public float shardMass = 0.02f;

    [Header("Safety / Performance")]
    public int maxTrianglesTotal = 3000; // across all child meshes
    public bool hideOriginalRenderers = true;
    public bool disableOriginalColliders = true;

    void Update()
    {
        if (Input.GetKeyDown(testKey))
            Shatter();
    }

    public void Shatter()
    {
        ShatterHierarchy(gameObject);
    }
    public void ShatterHierarchy(GameObject root)
    {
        if (!root)
        {
            Debug.LogError("TetraShatter.ShatterHierarchy: root is null.");
            return;
        }

        var meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: false);
        if (meshFilters.Length == 0)
        {
            Debug.LogError($"TetraShatter: No MeshFilter found under '{root.name}'. If this is SkinnedMeshRenderer, you need a baked-mesh version.");
            return;
        }

        if (hideOriginalRenderers)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        if (disableOriginalColliders)
        {
            foreach (var c in root.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }

        GameObject container = new GameObject(root.name + "_TETRA_SHARDS");
        container.transform.position = Vector3.zero;
        container.transform.rotation = Quaternion.identity;

        int spawned = 0;
        int triBudgetRemaining = maxTrianglesTotal;

        foreach (var mf in meshFilters)
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (!mr) continue;

            Mesh srcMesh = mf.sharedMesh;
            if (!srcMesh) continue;

            if (!srcMesh.isReadable)
            {
                Debug.LogError($"TetraShatter: Mesh '{srcMesh.name}' on '{mf.name}' is NOT readable. Enable Read/Write Enabled in import settings.");
                continue;
            }

            // Count triangles in this mesh
            int meshTriCount = 0;
            for (int s = 0; s < srcMesh.subMeshCount; s++)
                meshTriCount += srcMesh.GetTriangles(s).Length / 3;

            if (meshTriCount <= 0) continue;

            if (meshTriCount > triBudgetRemaining)
            {
                Debug.LogWarning($"TetraShatter: Triangle budget exceeded. Skipping '{mf.name}'. Needs {meshTriCount}, remaining {triBudgetRemaining}.");
                continue;
            }

            triBudgetRemaining -= meshTriCount;

            Material[] mats = mr.sharedMaterials;
            Transform t = mf.transform;

            Vector3[] v = srcMesh.vertices;
            Vector3[] n = srcMesh.normals;
            Vector2[] uv = srcMesh.uv;

            bool hasNormals = n != null && n.Length == v.Length;
            bool hasUVs = uv != null && uv.Length == v.Length;

            for (int sub = 0; sub < srcMesh.subMeshCount; sub++)
            {
                int[] tris = srcMesh.GetTriangles(sub);
                Material mat = (sub < mats.Length) ? mats[sub] : null;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    int i0 = tris[i];
                    int i1 = tris[i + 1];
                    int i2 = tris[i + 2];

                    // World positions of triangle
                    Vector3 w0 = t.TransformPoint(v[i0]);
                    Vector3 w1 = t.TransformPoint(v[i1]);
                    Vector3 w2 = t.TransformPoint(v[i2]);

                    // Centroid
                    Vector3 centroid = (w0 + w1 + w2) / 3f;

                    // Triangle normal in world
                    Vector3 triNormal;
                    if (hasNormals)
                    {
                        // Average vertex normals (world)
                        triNormal = (t.TransformDirection(n[i0]) + t.TransformDirection(n[i1]) + t.TransformDirection(n[i2])).normalized;
                    }
                    else
                    {
                        triNormal = Vector3.Cross(w1 - w0, w2 - w0).normalized;
                    }

                    // Compute a "regular-ish" height based on average edge length
                    float e01 = Vector3.Distance(w0, w1);
                    float e12 = Vector3.Distance(w1, w2);
                    float e20 = Vector3.Distance(w2, w0);
                    float avgEdge = (e01 + e12 + e20) / 3f;

                    float height = Mathf.Max(0.0005f, avgEdge * tetraHeightFactor);

                    // Apex point (world)
                    Vector3 apexWorld = centroid + triNormal * height;

                    // Create shard object
                    GameObject shard = new GameObject($"tet_{mf.name}_{sub}_{i / 3}");
                    shard.transform.SetParent(container.transform, worldPositionStays: true);
                    shard.transform.position = centroid;
                    shard.transform.rotation = Quaternion.identity;

                    // Build tetra mesh in shard-local space (centroid as origin)
                    Vector3 l0 = w0 - centroid;
                    Vector3 l1 = w1 - centroid;
                    Vector3 l2 = w2 - centroid;
                    Vector3 l3 = apexWorld - centroid;

                    Mesh tetra = BuildTetraMesh(l0, l1, l2, l3, hasUVs ? uv[i0] : Vector2.zero, hasUVs ? uv[i1] : Vector2.zero, hasUVs ? uv[i2] : Vector2.zero);
                    tetra.name = shard.name + "_mesh";

                    var shardMF = shard.AddComponent<MeshFilter>();
                    shardMF.sharedMesh = tetra;

                    var shardMR = shard.AddComponent<MeshRenderer>();
                    shardMR.sharedMaterial = mat;

                    var rb = shard.AddComponent<Rigidbody>();
                    rb.mass = shardMass;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;

                    var col = shard.AddComponent<MeshCollider>();
                    col.sharedMesh = tetra;
                    col.convex = true; // now valid because tetra is a closed volume

                    // Impulse away from original mesh's transform position (or root)
                    Vector3 dir = (centroid - t.position).sqrMagnitude > 0.000001f
                        ? (centroid - t.position).normalized
                        : Random.onUnitSphere;

                    Vector3 impulse = (dir + impulseBias.normalized * 0.35f).normalized * impulseStrength;
                    rb.AddForce(impulse, ForceMode.Impulse);
                    rb.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Impulse);

                    Destroy(shard, shardLifetime);
                    spawned++;
                }
            }
        }

        Destroy(container, shardLifetime + 0.1f);
        Debug.Log($"TetraShatter: Spawned {spawned} tetra shards.");
    }

    /// <summary>
    /// Creates a closed tetrahedron mesh.
    /// Base is triangle (a,b,c), apex is d.
    /// Winding is chosen to face outward given the base normal direction.
    /// </summary>
    private static Mesh BuildTetraMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 uva, Vector2 uvb, Vector2 uvc)
    {
        // Decide winding so that base normal points AWAY from apex.
        // If base normal points toward apex, flip base winding.
        Vector3 baseNormal = Vector3.Cross(b - a, c - a);
        float side = Vector3.Dot(baseNormal, d - a);

        if (side > 0f)
        {
            // Flip base
            (b, c) = (c, b);
            (uvb, uvc) = (uvc, uvb);
            baseNormal = -baseNormal;
        }

        // For clean flat shading (low-poly look), we duplicate vertices per face (4 faces * 3 verts = 12 verts).
        // UVs: we map base triangle uvs to the base face, and use a simple placeholder UV mapping for side faces.
        Vector3[] verts = new Vector3[12]
        {
            // Base face (a,b,c)
            a,b,c,

            // Side face 1 (a,d,b)
            a,d,b,

            // Side face 2 (b,d,c)
            b,d,c,

            // Side face 3 (c,d,a)
            c,d,a
        };

        int[] tris = new int[12]
        {
            // Base
            0,1,2,

            // Sides
            3,4,5,
            6,7,8,
            9,10,11
        };

        Vector2[] uvs = new Vector2[12]
        {
            // Base uses original triangle uvs (if present)
            uva, uvb, uvc,

            // Sides: simple placeholder mapping (works fine for solid materials; tweak if you care about textures)
            new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
            new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
            new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0)
        };

        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = tris;
        m.uv = uvs;

        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
