using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SCR_generate_map : MonoBehaviour {

    [Header("Require Dev Input")]

    [Tooltip("Tile needed to generate")][SerializeField]
    private RawImage map;

    [Tooltip("")][SerializeField]
    private Sprite sprite;

    [Tooltip("Size of grid")] [SerializeField]
    private int size;

    private int height;
    private int width;

    [Tooltip("Number of iterations while generating")][SerializeField]
    private int iterations;

    [Tooltip("")][SerializeField]
    private int islandSize;

    [Tooltip("")][SerializeField]
    private int sizeMax;

    private int iterationsMax;
    private int islandSizeMax;


    [Tooltip("")][SerializeField]
    private List<Color> tileColours = new List<Color>();

    [Tooltip("Should start in centre? If true begin in centre, if false begin with random tile")][SerializeField]
    private bool startInCentre;

    //Hold entire map in dictionary, easy way of finding near tiles from base tile
    private Dictionary<Vector2, Color> mapData = new Dictionary<Vector2, Color>();

    //
    private GameObject spritesParent;

    [Header("UI")]
    [Tooltip("")][SerializeField]
    private GameObject buttonsParent;
    [Tooltip("")][SerializeField]
    private GameObject buttonPrefab;
    [Tooltip("")][SerializeField]
    private GameObject fieldParent;
    [Tooltip("")][SerializeField]
    private GameObject fieldPrefab;

    private void Start() {
        buttonsParent = GameObject.Find("Buttons");
        SCR_utils.monoFunctions.createButton("Walk Cycle", walkCycleButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Perlin Noise", perlinNoiseButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Save As", displayAsImage, buttonPrefab, buttonsParent);
        //SCR_utils.monoFunctions.createButton("Sprites", displayAsSprites, buttonPrefab, buttonsParent);

        fieldParent = GameObject.Find("Fields");
        TMP_InputField sizeField = SCR_utils.monoFunctions.createField("Size Field", fieldPrefab, fieldParent);
        TMP_InputField iterationField = SCR_utils.monoFunctions.createField("Iteration Field", fieldPrefab, fieldParent);
        TMP_InputField islandSizeField = SCR_utils.monoFunctions.createField("Island Size Field", fieldPrefab, fieldParent);
        sizeField.onEndEdit.AddListener(delegate { onUpdateField(sizeField, updateSize, sizeMax, 2); });
        iterationField.onEndEdit.AddListener(delegate { onUpdateField(iterationField, updateIteration, iterationsMax); });
        islandSizeField.onEndEdit.AddListener(delegate { onUpdateField(islandSizeField, updateIslandSize, islandSizeMax); });

        spritesParent = new GameObject("Sprites Parent");

        size = 2;
        islandSize = 2;
        iterations = 2;
    }
    #region Buttons
    public void walkCycleButton() {
        mapData.Clear();
        createBlank();
        walkCycleMain();
        displayAsImage();
    }
    public void perlinNoiseButton() {
        mapData.Clear();
        createBlank();
        perlinNoiseMain();
        displayAsImage();
    }
    public void onUpdateField(TMP_InputField field,Action<int> action, int max, int min = 0) {
        int input = SCR_utils.functions.validateIntFromString(field.text);

        input = Mathf.Clamp(input, min, max);

        field.text = input.ToString();

        action(input);
    }
    public void updateSize(int newValue) {
        size = newValue;
        iterationsMax = size * size;
        islandSizeMax = size * size;
    }
    public void updateIteration(int newValue) {
        iterations = newValue;
    }
    public void updateIslandSize(int newValue) {
        islandSize = newValue;
    }
    public void reload() {
        SceneManager.LoadScene(0);
    }
    #endregion
    #region Main
    private void perlinNoiseMain() {
        //Vector2 currentPos = returnStartingPos();
    }
    private void walkCycleMain() {
        //Generate Map, iterate until completed
        Vector2 currentPos = returnStartingPos();

        mapData[currentPos] = tileColours[1];

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
            mapData[currentPos] = tileColours[1];
        }
    }
    private void displayAsImage() {
        spritesParent.SetActive(false);
        map.gameObject.SetActive(true);

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(mapData.Values.ToArray());
        texture.filterMode = FilterMode.Point;

        texture.Apply();
        map.texture = texture;

        Camera.main.orthographicSize = 1;
    }
    private void displayAsSprites() {
        spritesParent.SetActive(true);
        map.gameObject.SetActive(false);

        Destroy(spritesParent);
        spritesParent = new GameObject("Sprites Parent");

        for (int i = 0; i < mapData.Count; i++) {
            GameObject obj = new GameObject("a", typeof (SpriteRenderer));
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = mapData.ElementAt(i).Value;
            obj.transform.position = mapData.ElementAt(i).Key;
            obj.transform.parent = spritesParent.transform;
        }
    }
    private Vector2 returnStartingPos() {
        Vector2 currentPos;
        if (startInCentre) currentPos = returnCentre();
        else currentPos = returnRandomTile();

        currentPos.x = MathF.Round(currentPos.x);
        currentPos.y = MathF.Round(currentPos.y);

        return currentPos;
    }
    private Vector2 returnCentre() {
        Vector2 v = (mapData.Keys.Last() + mapData.Keys.First())/2;
        return v;
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
        if (size < 2) size = 2;
        height = size;
        width = size;
        for (int x = 1; x < width + 1; x++) {
            for (int y = 1; y < height + 1; y++) {
                Vector2 tilePos = new Vector2(x, y);
                mapData.Add(tilePos, tileColours[0]);
            }
        }
    }
    #endregion
}
