using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public Transform tilePrefab;
    public Vector2 mapSize;

    private void OnDrawGizmos()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        string holderName = "Environment";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
                Gizmos.DrawSphere(tilePosition,0.1f);
                
                //Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
                //newTile.parent = mapHolder;
            }
        }
    }
}
