using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChunkSpawner : MonoBehaviour
{
    public GameObject ChunkPrefab;
    public float TotalSquareFeet = 100;
    public float ChunkOffset = 10;

    // Start is called before the first frame update
    void Start()
    {
        SpawnChunks();
    }

    private void SpawnChunks()
    {
        int numChunksPerRow = Mathf.RoundToInt(Mathf.Sqrt(TotalSquareFeet));

        for (int i = 0; i < numChunksPerRow; i++)
        {
            for (int j = 0; j < numChunksPerRow; j++)
            {
                GameObject SpawnedChunk = Instantiate(ChunkPrefab);

                float xOffset = i * ChunkOffset;
                float yOffset = j * ChunkOffset;

                float XPosition = transform.position.x + xOffset;
                float YPosition = transform.position.y + yOffset;

                SpawnedChunk.transform.position = new Vector3(XPosition, 0, YPosition);
            }
        }
    }
}
