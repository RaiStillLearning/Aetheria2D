using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefab")]
    public GameObject platformPrefab;

    [Header("Spawn Settings")]
    public int initialPlatforms = 12;
    public float minX = -3f;
    public float maxX = 3f;
    public float minYGap = 1.3f;
    public float maxYGap = 2.0f;
    public float minWidth = 1.5f;
    public float maxWidth = 3.5f;

    List<GameObject> _platforms = new List<GameObject>();
    float _nextSpawnY;
    Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        // Spawn platform bawah awal (ground start area)
        SpawnAt(new Vector2(0f, -2.5f), 5f);
        _nextSpawnY = -1f;

        for (int i = 0; i < initialPlatforms; i++)
            SpawnNext();
    }

    void Update()
    {
        if (_cam == null) return;

        // Spawn platform baru di atas viewport
        float camTop = _cam.transform.position.y + _cam.orthographicSize + 2f;
        while (_nextSpawnY < camTop + 4f)
            SpawnNext();

        // Hapus platform yang sudah di bawah viewport
        float camBottom = _cam.transform.position.y - _cam.orthographicSize - 2f;
        for (int i = _platforms.Count - 1; i >= 0; i--)
        {
            if (_platforms[i] == null || _platforms[i].transform.position.y < camBottom)
            {
                if (_platforms[i] != null) Destroy(_platforms[i]);
                _platforms.RemoveAt(i);
            }
        }
    }

    void SpawnNext()
    {
        float x = Random.Range(minX, maxX);
        float w = Random.Range(minWidth, maxWidth);
        SpawnAt(new Vector2(x, _nextSpawnY), w);
        _nextSpawnY += Random.Range(minYGap, maxYGap);
    }

    void SpawnAt(Vector2 pos, float width)
    {
        if (platformPrefab == null) return;
        var go = Instantiate(platformPrefab, pos, Quaternion.identity);

        // Set SpriteRenderer tiled size
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(width, 0.5f);
        }

        // Set BoxCollider2D sesuai size
        var bc = go.GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            bc.size = new Vector2(width, 0.5f);
            bc.offset = Vector2.zero;
        }

        // Pastikan layer Ground
        go.layer = LayerMask.NameToLayer("Ground");

        _platforms.Add(go);
    }
}
