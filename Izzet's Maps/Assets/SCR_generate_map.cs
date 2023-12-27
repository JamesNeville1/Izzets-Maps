using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;

public class SCR_generate_map : MonoBehaviour {

    [Header("Require Dev Input")]

    [Tooltip("Tile needed to generate")][SerializeField]
    private GameObject tilePrefab;

    [Tooltip("Height of grid")][SerializeField]
    private int height;

    [Tooltip("Width of grid")][SerializeField]
    private int width;

    [Tooltip("Number of iterations while generating")][SerializeField]
    private int iterations;

    [Tooltip("")][SerializeField]
    private int islandSize;

    [Tooltip("")][SerializeField]
    private List<Color> tileColours = new List<Color>();

    [Tooltip("Should start in centre? If true begin in centre, if false begin with random tile")][SerializeField]
    private bool startInCentre;

    //Hold entire map in dictionary, easy way of finding near tiles from base tile
    private Dictionary<Vector2, GameObject> mapData = new Dictionary<Vector2, GameObject>();

    private void Awake() {
        tileColours.Add(Color.red);
    }
    #region Buttons
    public void walkCycleButton() {
        deleteMap();
        createBlank();
        walkCycleMain();
        adjustCamera();
    }
    public void perlinNoiseButton() {
        deleteMap();
        createBlank();
        perlinNoiseMain();
        adjustCamera();
    }
    public void reload() {
        SceneManager.LoadScene(0);
    }
    #endregion
    #region Perlin Noise Algorthim
    private void perlinNoiseMain() {
        Vector2 currentPos = returnStartingPos();

        for (int i = 0; i < mapData.Count; i++) {
            print(
                mapData.ElementAt(i).Value.name + 
                " " +
                Mathf.PerlinNoise(mapData.ElementAt(i).Key.x, mapData.ElementAt(i).Key.y));
        }
    }
    #endregion
    #region Walk Cycle Algorithm
    private void walkCycleMain() {
        //Generate Map, iterate until completed
        Vector2 currentPos = returnStartingPos();

        mapData[currentPos].GetComponent<SpriteRenderer>().color = tileColours[0];

        int currentIslandSize = 0;
        for (int i = 0; i < iterations; i++) {
            Vector2 dir = returnRandomDir();
            if (mapData.ContainsKey(currentPos + dir)) {
                if(currentIslandSize <= islandSize) {
                    currentPos = currentPos + dir;
                    currentIslandSize++;
                }
                else {
                    currentPos = returnRandomTile();
                    currentIslandSize = 0;
                }
            }
            else {
                currentPos = returnRandomTile();
                currentIslandSize = 0;
            }
            mapData[currentPos].gameObject.GetComponent<SpriteRenderer>().color = tileColours[0];
        }
    }
    #endregion
    #region Map Utils
    private Vector2 returnStartingPos() {
        Vector2 currentPos;
        if (startInCentre) currentPos = returnCentre();
        else currentPos = returnRandomTile();

        currentPos.x = MathF.Round(currentPos.x);
        currentPos.y = MathF.Round(currentPos.y);

        return currentPos;
    }
    private void deleteMap() {
        for (int i = 0; i < mapData.Count; i++) {
            Destroy(mapData.ElementAt(i).Value);
        }
        mapData.Clear();
    }
    private Vector2 returnCentre() {
        Vector2 v = (mapData.Keys.Last() + mapData.Keys.First())/2;
        return v;
    }

    private void adjustCamera() {
        Vector2 centre = returnCentre();
        Camera.main.transform.position = new Vector3(centre.x, centre.y, -10);

        if (height > width) Camera.main.orthographicSize = (height / 2) + 15;
        else Camera.main.orthographicSize = (width / 2) + 15;
    }

    private Vector2 returnRandomTile() {
        Vector2 v = mapData.ElementAt(UnityEngine.Random.Range(0, mapData.Count)).Key;
        return v;
    }

    private Vector2 returnRandomDir() {
        int i = UnityEngine.Random.Range(1, 5);
        switch (i) {
            case 1: return Vector2.left;
            case 2: return Vector2.right;
            case 3: return Vector2.up;
            case 4: return Vector2.down;
        }
        return Vector2.left;
    }
    private void createBlank() {
        if (height < 2) height = 2;
        if (width < 2) width = 2;
        for (int x = 1; x < width + 1; x++) {
            for (int y = 1; y < height + 1; y++) {
                Vector2 tilePos = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity);
                tile.name = tilePrefab.name + tilePos;
                tile.transform.parent = transform;
                mapData.Add(tilePos, tile);
            }
        }
    }
    #endregion
}
