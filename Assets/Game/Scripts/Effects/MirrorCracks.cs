using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MirrorCracks : MonoBehaviour
{
    public HealthSystem health;
    public Renderer target;   

    public int Width = 1024;
    public int Height = 698;   
    public float GrowTime = 0.3f;   

    public Color CrackColor = new Color(1f, 1f, 1f, 0.9f);
    public Color ShadowColor = new Color(0f, 0f, 0f, 0.5f);   

    // where the hits land (0..1 on the mirror), one per lost HP; the last one is the big shatter
    public Vector2[] ImpactPoints = { new Vector2(0.28f, 0.68f), new Vector2(0.74f, 0.38f), new Vector2(0.5f, 0.52f) };

    struct Seg
    {
        public Vector2 a, b;
        public float width;
        public float order;   // distance from impact, used to grow the crack outwards
    }

    Texture2D tex;
    Color32[] pixels;
    int hitsShown;
    Coroutine growRoutine;
    readonly Queue<List<Seg>> waiting = new Queue<List<Seg>>();

    void Awake()
    {
        tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        pixels = new Color32[Width * Height];
        Material mat = target.material;   // own copy, so the material asset isn't changed
        mat.SetTexture("_BaseMap", tex);
        mat.mainTexture = tex;
        mat.SetColor("_BaseColor", Color.white);   // the material asset is invisible (alpha 0) so the editor shows no white sheet
        ClearCracks();
    }

    void OnEnable()  { health.OnHPChanged += HPChanged; }
    void OnDisable() { health.OnHPChanged -= HPChanged; }

    void HPChanged(int hp)
    {
        int hits = health.MaxHP - hp;

        if (hits <= 0)   // new game -> clean mirror
        {
            ClearCracks();
            return;
        }

        while (hitsShown < hits)
        {
            hitsShown++;
            bool shatter = hitsShown >= health.MaxHP;
            Vector2 p = ImpactPoints[Mathf.Min(hitsShown - 1, ImpactPoints.Length - 1)];
            p += Random.insideUnitCircle * 0.05f;   // a bit different every game
            waiting.Enqueue(BuildHit(new Vector2(p.x * Width, p.y * Height), shatter));
        }

        if (growRoutine == null) growRoutine = StartCoroutine(Grow());
    }

    public void ClearCracks()
    {
        if (growRoutine != null) StopCoroutine(growRoutine);
        growRoutine = null;
        waiting.Clear();
        hitsShown = 0;

        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 0);
        tex.SetPixels32(pixels);
        tex.Apply();
    }

    IEnumerator Grow()
    {
        while (waiting.Count > 0)
        {
            List<Seg> segs = waiting.Dequeue();
            segs.Sort((x, y) => x.order.CompareTo(y.order));

            int drawn = 0;
            float elapsedTime = 0f;
            while (drawn < segs.Count)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / GrowTime);
                int goal = Mathf.CeilToInt(segs.Count * (1f - (1f - t) * (1f - t)));   // fast start, slow end

                for (; drawn < goal; drawn++) DrawSeg(segs[drawn]);

                tex.SetPixels32(pixels);
                tex.Apply();
                yield return null;
            }
        }
        growRoutine = null;
    }


    List<Seg> BuildHit(Vector2 center, bool shatter)
    {
        var segs = new List<Seg>();
        float scale = Height / 698f;

        int rays = shatter ? Random.Range(16, 21) : Random.Range(9, 13);
        float minLen = (shatter ? 350f : 120f) * scale;
        float maxLen = (shatter ? 950f : 360f) * scale;
        float[] ringRadii = shatter
            ? new[] { 30f * scale, 80f * scale, 160f * scale, 270f * scale }
            : new[] { 25f * scale, 65f * scale };

        // ring points: where each ray crosses each ring radius
        var ringPoints = new Vector2?[ringRadii.Length, rays];

        for (int i = 0; i < rays; i++)
        {
            float angle = (i + Random.Range(-0.3f, 0.3f)) / rays * Mathf.PI * 2f;
            float length = Random.Range(minLen, maxLen);
            Ray(center, angle, length, shatter ? 3.2f : 2.6f, center, segs, i, ringRadii, ringPoints, 0);
        }

        // web rings between neighbour rays
        for (int r = 0; r < ringRadii.Length; r++)
        {
            for (int i = 0; i < rays; i++)
            {
                Vector2? a = ringPoints[r, i];
                Vector2? b = ringPoints[r, (i + 1) % rays];
                if (a == null || b == null || Random.value < 0.25f) continue;

                Vector2 mid = (a.Value + b.Value) * 0.5f + Random.insideUnitCircle * 6f * scale;
                float w = r == 0 ? 1.6f : 1.1f;
                Add(segs, a.Value, mid, w, center);
                Add(segs, mid, b.Value, w, center);
            }
        }

        // crushed glass right at the impact
        int bits = shatter ? 40 : 18;
        for (int i = 0; i < bits; i++)
        {
            Vector2 p = center + Random.insideUnitCircle * 14f * scale;
            Add(segs, p, p + Random.insideUnitCircle * 9f * scale, 1.2f, center);
        }

        return segs;
    }

    void Ray(Vector2 start, float angle, float length, float width, Vector2 center, List<Seg> segs,
             int rayIndex, float[] ringRadii, Vector2?[,] ringPoints, int depth)
    {
        Vector2 p = start;
        float traveled = 0f;

        while (traveled < length)
        {
            float step = Random.Range(14f, 34f);
            angle += Random.Range(-0.22f, 0.22f);   // cracks wobble a little
            Vector2 next = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * step;
            float w = Mathf.Lerp(width, 0.7f, traveled / length);   // thinner towards the end

            Add(segs, p, next, w, center);

            if (depth == 0)   // remember where the main ray crosses each ring
            {
                float d0 = (p - center).magnitude, d1 = (next - center).magnitude;
                for (int r = 0; r < ringRadii.Length; r++)
                {
                    if (ringPoints[r, rayIndex] == null && d0 <= ringRadii[r] && d1 > ringRadii[r])
                        ringPoints[r, rayIndex] = next;
                }
            }

            if (depth < 2 && Random.value < 0.1f)   // side branch
            {
                float side = Random.value < 0.5f ? -1f : 1f;
                Ray(next, angle + side * Random.Range(0.4f, 0.9f), (length - traveled) * Random.Range(0.25f, 0.5f),
                    w * 0.75f, center, segs, rayIndex, ringRadii, ringPoints, depth + 1);
            }

            p = next;
            traveled += step;

            if (p.x < -20 || p.y < -20 || p.x > Width + 20 || p.y > Height + 20) break;   // left the mirror
        }
    }

    void Add(List<Seg> segs, Vector2 a, Vector2 b, float width, Vector2 center)
    {
        segs.Add(new Seg { a = a, b = b, width = width, order = (a - center).magnitude });
    }

    // ---------- drawing ----------

    void DrawSeg(Seg s)
    {
        Line(s.a + new Vector2(1.2f, -1.2f), s.b + new Vector2(1.2f, -1.2f), s.width + 1.4f, ShadowColor);
        Line(s.a, s.b, s.width, CrackColor);
    }

    void Line(Vector2 a, Vector2 b, float width, Color color)
    {
        float length = (b - a).magnitude;
        int steps = Mathf.Max(1, Mathf.CeilToInt(length / 0.75f));
        for (int i = 0; i <= steps; i++)
        {
            Dot(Vector2.Lerp(a, b, i / (float)steps), width * 0.5f, color);
        }
    }

    void Dot(Vector2 c, float radius, Color color)
    {
        int x0 = Mathf.Max(0, Mathf.FloorToInt(c.x - radius - 1));
        int x1 = Mathf.Min(Width - 1, Mathf.CeilToInt(c.x + radius + 1));
        int y0 = Mathf.Max(0, Mathf.FloorToInt(c.y - radius - 1));
        int y1 = Mathf.Min(Height - 1, Mathf.CeilToInt(c.y + radius + 1));

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                float cover = Mathf.Clamp01(radius + 0.5f - d);   // smooth edge
                if (cover <= 0f) continue;

                float a = color.a * cover;
                int i = y * Width + x;
                Color old = pixels[i];
                Color mixed = Color.Lerp(old, color, a);
                mixed.a = Mathf.Max(old.a, a);
                pixels[i] = mixed;
            }
        }
    }
}
