using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        if (Input.GetKeyDown(testKey))
            Shatter();
    }

    public void Shatter()
    {
        ShatterInternal(gameObject, Vector3.zero, false, Vector3.zero);
    }

    public void Shatter(Vector3 forceDirectionWorld)
    {
        ShatterInternal(gameObject, forceDirectionWorld, false, Vector3.zero);
    }

    public void Shatter(Vector3 forceDirectionWorld, Vector3 forceOriginWorld)
    {
        ShatterInternal(gameObject, forceDirectionWorld, true, forceOriginWorld);
    }

    private void ShatterInternal(GameObject root, Vector3 forceDirWorld, bool useOrigin, Vector3 originWorld)
    {
        if (!root) return;

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
            container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        Vector3 biasNorm = impulseBias.sqrMagnitude > 0.00001f ? impulseBias.normalized : Vector3.zero;
        Vector3 dirNorm = forceDirWorld.sqrMagnitude > 0.00001f ? forceDirWorld.normalized * directionalForceWeight : Vector3.zero;

        var verts = new List<Vector3>(trianglesPerShard * 6);
        var tris = new List<int>(trianglesPerShard * 24);

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
                Material mat = mr.sharedMaterials[sub];

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
                        avgNormal += Vector3.Cross(w1 - w0, w2 - w0).normalized;
                        avgEdge += (w1 - w0).magnitude + (w2 - w1).magnitude + (w0 - w2).magnitude;
                    }

                    centroid /= take;
                    avgNormal.Normalize();
                    avgEdge /= (take * 3f);
                    float thickness = Mathf.Max(0.0002f, avgEdge * thicknessFactor);

                    for (int k = 0; k < take; k++)
                    {
                        int i = (triIndex + k) * 3;
                        Vector3 w0 = t.TransformPoint(v[srcTris[i]]);
                        Vector3 w1 = t.TransformPoint(v[srcTris[i + 1]]);
                        Vector3 w2 = t.TransformPoint(v[srcTris[i + 2]]);

                        Vector3 nrm = Vector3.Cross(w1 - w0, w2 - w0).normalized;
                        Vector3 off = nrm * (thickness * 0.5f);

                        Vector3 f0 = (w0 - centroid) + off;
                        Vector3 f1 = (w1 - centroid) + off;
                        Vector3 f2 = (w2 - centroid) + off;

                        Vector3 b0 = (w0 - centroid) - off;
                        Vector3 b1 = (w1 - centroid) - off;
                        Vector3 b2 = (w2 - centroid) - off;

                        int start = verts.Count;
                        verts.AddRange(new[] { f0, f1, f2, b0, b1, b2 });

                        tris.AddRange(new[]
                        {
                            start, start+1, start+2,
                            start+5, start+4, start+3,
                            start, start+1, start+4, start, start+4, start+3,
                            start+1, start+2, start+5, start+1, start+5, start+4,
                            start+2, start, start+3, start+2, start+3, start+5
                        });
                    }

                    GameObject shard = new GameObject($"chunk_{mf.name}_{sub}_{shardIndex++}");
                    if (useContainer) shard.transform.SetParent(container.transform, true);
                    shard.transform.position = centroid;

                    Mesh m = new Mesh();
                    m.SetVertices(verts);
                    m.SetTriangles(tris, 0);
                    m.RecalculateNormals();
                    m.MarkDynamic();

                    shard.AddComponent<MeshFilter>().sharedMesh = m;

                    var r = shard.AddComponent<MeshRenderer>();
                    r.sharedMaterial = mat;
                    if (disableShadowsOnShards)
                    {
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        r.receiveShadows = false;
                    }

                    var rb = shard.AddComponent<Rigidbody>();
                    rb.mass = shardMass;
                    rb.useGravity = useGravity;
                    rb.linearDamping = linearDrag;
                    rb.angularDamping = angularDrag;

                    // ✅ ALWAYS APPLY IMPULSE
                    Vector3 baseDir;

                    if (useOrigin)
                        baseDir = (centroid - originWorld).normalized;
                    else
                        baseDir = (centroid - rootT.position).normalized;

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
