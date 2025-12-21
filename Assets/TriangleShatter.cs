using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TetraShatter : MonoBehaviour
{
    [Header("Trigger (optional test)")]
    public KeyCode testKey = KeyCode.K;

    [Header("Shard Lifetime")]
    [Min(0.01f)] public float shardLifetime = 1.0f;

    [Header("Resolution")]
    [Min(1)] public int trianglesPerShard = 8;

    [Header("Chunk Thickness")]
    [Range(0.01f, 2.0f)] public float thicknessFactor = 0.20f;

    [Header("Physics (NO colliders)")]
    public float impulseStrength = 2.5f;
    public Vector3 impulseBias = Vector3.up;
    public float directionalForceWeight = 1.0f;
    public float randomTorque = 5f;
    [Min(0.0001f)] public float shardMass = 0.05f;
    public bool useGravity = true;
    public float linearDrag = 0f;
    public float angularDrag = 0.05f;

    [Header("Rendering")]
    public bool disableShadowsOnShards = true;

    [Header("Safety / Performance")]
    public int maxTrianglesTotal = 6000;
    public bool hideOriginalRenderers = true;
    public bool disableOriginalColliders = true;
    public bool useContainer = true;

    [Header("Per-object shader property (MPB)")]
    public string alphaProperty = "_Alpha";
    [Range(0f, 1f)] public float iced = 0f;

    private Renderer rend;
    private MaterialPropertyBlock mpb;     // CREATED IN AWAKE (not field init)
    private int alphaId;
    private float lastIced = float.NaN;

    // Reuse lists (avoid GC)
    private List<Vector3> verts;
    private List<int> tris;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock(); // ✅ allowed here
        alphaId = Shader.PropertyToID(alphaProperty);

        verts = new List<Vector3>(trianglesPerShard * 6);
        tris  = new List<int>(trianglesPerShard * 24);

        // Initialize MPB safely
        ApplyMPBIfChanged(force: true);
    }

    void Update()
    {
        ApplyMPBIfChanged(force: false);

        if (Input.GetKeyDown(testKey))
            Shatter();
    }

    private void ApplyMPBIfChanged(bool force)
    {
        if (!force && Mathf.Approximately(lastIced, iced))
            return;

        lastIced = iced;

        // Safely read current block, then set alpha
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(alphaId, iced);
        rend.SetPropertyBlock(mpb);
    }

    public void Shatter() => ShatterInternal(gameObject, Vector3.zero, false, Vector3.zero);
    public void Shatter(Vector3 forceDirectionWorld) => ShatterInternal(gameObject, forceDirectionWorld, false, Vector3.zero);
    public void Shatter(Vector3 forceDirectionWorld, Vector3 forceOriginWorld) => ShatterInternal(gameObject, forceDirectionWorld, true, forceOriginWorld);

    private void ShatterInternal(GameObject root, Vector3 forceDirWorld, bool useOrigin, Vector3 originWorld)
    {
        if (!root) return;

        // Ensure mpb exists (in case something calls before Awake)
        if (mpb == null) mpb = new MaterialPropertyBlock();
        if (rend == null) rend = GetComponent<Renderer>();

        // Capture layer and current MPB ONCE at shatter time
        int sourceLayer = root.layer;
        rend.GetPropertyBlock(mpb);

        var meshFilters = root.GetComponentsInChildren<MeshFilter>(false);
        if (meshFilters.Length == 0) return;

        if (hideOriginalRenderers)
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                r.enabled = false;

        if (disableOriginalColliders)
            foreach (var c in root.GetComponentsInChildren<Collider>())
                c.enabled = false;

        Transform rootT = root.transform;

        GameObject container = null;
        if (useContainer)
        {
            container = new GameObject(root.name + "_CHUNK_SHARDS");
            container.layer = sourceLayer;
            container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        Vector3 biasNorm = impulseBias.sqrMagnitude > 0.00001f ? impulseBias.normalized : Vector3.zero;
        Vector3 dirNorm = forceDirWorld.sqrMagnitude > 0.00001f ? forceDirWorld.normalized * directionalForceWeight : Vector3.zero;

        int triBudgetRemaining = maxTrianglesTotal;

        foreach (var mf in meshFilters)
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (!mr) continue;

            Mesh src = mf.sharedMesh;
            if (!src || !src.isReadable) continue;

            int subCount = src.subMeshCount;
            Vector3[] v = src.vertices;
            Transform t = mf.transform;

            for (int sub = 0; sub < subCount; sub++)
            {
                int[] srcTris = src.GetTriangles(sub);
                int triCount = srcTris.Length / 3;
                if (triCount > triBudgetRemaining) continue;
                triBudgetRemaining -= triCount;

                Material mat = (mr.sharedMaterials != null && sub < mr.sharedMaterials.Length) ? mr.sharedMaterials[sub] : null;

                int triIndex = 0;
                int shardIndex = 0;

                while (triIndex < triCount)
                {
                    int take = Mathf.Min(trianglesPerShard, triCount - triIndex);
                    verts.Clear();
                    tris.Clear();

                    Vector3 centroid = Vector3.zero;
                    Vector3 avgNormal = Vector3.zero;
                    float avgEdge = 0f;

                    for (int k = 0; k < take; k++)
                    {
                        int i = (triIndex + k) * 3;
                        Vector3 w0 = t.TransformPoint(v[srcTris[i]]);
                        Vector3 w1 = t.TransformPoint(v[srcTris[i + 1]]);
                        Vector3 w2 = t.TransformPoint(v[srcTris[i + 2]]);

                        centroid += (w0 + w1 + w2) / 3f;

                        Vector3 nrm = Vector3.Cross(w1 - w0, w2 - w0);
                        if (nrm.sqrMagnitude > 0.000001f) avgNormal += nrm.normalized;

                        avgEdge += (w1 - w0).magnitude + (w2 - w1).magnitude + (w0 - w2).magnitude;
                    }

                    centroid /= take;
                    if (avgNormal.sqrMagnitude > 0.000001f) avgNormal.Normalize();
                    avgEdge /= (take * 3f);
                    float thickness = Mathf.Max(0.0002f, avgEdge * thicknessFactor);

                    for (int k = 0; k < take; k++)
                    {
                        int i = (triIndex + k) * 3;
                        Vector3 w0 = t.TransformPoint(v[srcTris[i]]);
                        Vector3 w1 = t.TransformPoint(v[srcTris[i + 1]]);
                        Vector3 w2 = t.TransformPoint(v[srcTris[i + 2]]);

                        Vector3 nrm = Vector3.Cross(w1 - w0, w2 - w0);
                        nrm = (nrm.sqrMagnitude > 0.000001f) ? nrm.normalized : (avgNormal.sqrMagnitude > 0.000001f ? avgNormal : Vector3.up);

                        Vector3 off = nrm * (thickness * 0.5f);

                        Vector3 f0 = (w0 - centroid) + off;
                        Vector3 f1 = (w1 - centroid) + off;
                        Vector3 f2 = (w2 - centroid) + off;

                        Vector3 b0 = (w0 - centroid) - off;
                        Vector3 b1 = (w1 - centroid) - off;
                        Vector3 b2 = (w2 - centroid) - off;

                        int start = verts.Count;
                        // no allocations: add individually instead of AddRange(new[])
                        verts.Add(f0); verts.Add(f1); verts.Add(f2);
                        verts.Add(b0); verts.Add(b1); verts.Add(b2);

                        // Same tri block, no array allocation
                        tris.Add(start);   tris.Add(start + 1); tris.Add(start + 2);
                        tris.Add(start + 5); tris.Add(start + 4); tris.Add(start + 3);

                        tris.Add(start);   tris.Add(start + 1); tris.Add(start + 4);
                        tris.Add(start);   tris.Add(start + 4); tris.Add(start + 3);

                        tris.Add(start + 1); tris.Add(start + 2); tris.Add(start + 5);
                        tris.Add(start + 1); tris.Add(start + 5); tris.Add(start + 4);

                        tris.Add(start + 2); tris.Add(start);   tris.Add(start + 3);
                        tris.Add(start + 2); tris.Add(start + 3); tris.Add(start + 5);
                    }

                    GameObject shard = new GameObject($"chunk_{mf.name}_{sub}_{shardIndex++}");
                    shard.layer = sourceLayer; // ✅ match source layer
                    if (useContainer) shard.transform.SetParent(container.transform, true);
                    shard.transform.position = centroid;

                    Mesh m = new Mesh();
                    m.SetVertices(verts);
                    m.SetTriangles(tris, 0);
                    m.RecalculateNormals();
                    m.MarkDynamic();

                    shard.AddComponent<MeshFilter>().sharedMesh = m;

                    var shardRenderer = shard.AddComponent<MeshRenderer>();
                    shardRenderer.sharedMaterial = mat;

                    // ✅ apply per-object MPB (like _Alpha) to shard
                    shardRenderer.SetPropertyBlock(mpb);

                    if (disableShadowsOnShards)
                    {
                        shardRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        shardRenderer.receiveShadows = false;
                    }

                    var rb = shard.AddComponent<Rigidbody>();
                    rb.mass = shardMass;
                    rb.useGravity = useGravity;

#if UNITY_2022_2_OR_NEWER
                    rb.linearDamping = linearDrag;
                    rb.angularDamping = angularDrag;
#else
                    rb.drag = linearDrag;
                    rb.angularDrag = angularDrag;
#endif

                    Vector3 baseDir = useOrigin ? (centroid - originWorld).normalized : (centroid - rootT.position).normalized;
                    Vector3 finalDir = (baseDir + dirNorm + biasNorm).normalized;

                    rb.AddForce(finalDir * impulseStrength, ForceMode.Impulse);

                    if (randomTorque > 0f)
                        rb.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Impulse);

                    Destroy(shard, shardLifetime);
                    triIndex += take;
                }
            }
        }

        if (useContainer && container)
            Destroy(container, shardLifetime + 0.1f);
    }
}
