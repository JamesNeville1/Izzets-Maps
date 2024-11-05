using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling.LowLevel;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.Mathematics;

public class SCR_generate_map : MonoBehaviour {

    [Header("Require Dev Input")]

    [Tooltip("Tile needed to generate")][SerializeField]
    private RawImage map;

    [Tooltip("")][SerializeField]
    private Sprite sprite;

    [Tooltip("Size of grid X")] [SerializeField]
    private int height;
    [Tooltip("Size of grid Y")] [SerializeField]
    private int width;

    [Tooltip("Number of iterations while generating")][SerializeField]
    private int iterations;

    [Tooltip("")][SerializeField]
    private int perlinScale;

    [Tooltip("")][SerializeField]
    private int scaleHueBy;

    [Tooltip("")][SerializeField]
    private int islandSize;

    [Header("Max (No Extreme Loading)")]
    [Tooltip("")][SerializeField]
    private int sizeMax;
    [Tooltip("")][SerializeField]
    private int iterationsMax;
    [Tooltip("")][SerializeField]
    private int islandSizeMax;

    [Header("Colours")]
    [Tooltip("")][SerializeField]
    private Color backgroundPerlin;
    [Tooltip("")][SerializeField]
    private Color backgroundRandomWalk;
    [Tooltip("")][SerializeField]
    private Color tilesRandomWalk;

    [Header("UI")]
    [Tooltip("")][SerializeField]
    private GameObject buttonsParent;
    [Tooltip("")][SerializeField]
    private GameObject buttonPrefab;
    [Tooltip("")][SerializeField]
    private GameObject fieldParent;
    [Tooltip("")][SerializeField]
    private GameObject fieldPrefab;

    [Header("")]
    TMP_InputField sizeField;
    TMP_InputField iterationField;
    TMP_InputField islandSizeField;

    private void Start() {
        buttonsParent = GameObject.Find("Buttons");
        SCR_utils.monoFunctions.createButton("Walk Cycle", WalkCycleButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Perlin Noise", PerlinNoiseButton, buttonPrefab, buttonsParent);
        //SCR_utils.monoFunctions.createButton("Save As", DisplayAsImage, buttonPrefab, buttonsParent); //ToDo

        fieldParent = GameObject.Find("Fields");
        sizeField = SCR_utils.monoFunctions.createField("Size Field", fieldPrefab, fieldParent);
        iterationField = SCR_utils.monoFunctions.createField("Iteration Field", fieldPrefab, fieldParent);
        islandSizeField = SCR_utils.monoFunctions.createField("Island Size Field", fieldPrefab, fieldParent);
        sizeField.onEndEdit.AddListener(delegate { OnUpdateField(sizeField, UpdateSize, sizeMax, 2); });
        iterationField.onEndEdit.AddListener(delegate { OnUpdateField(iterationField, UpdateIteration, iterationsMax); });
        islandSizeField.onEndEdit.AddListener(delegate { OnUpdateField(islandSizeField, UpdateIslandSize, islandSizeMax); });
    }
    #region Buttons
    public void WalkCycleButton() {
        walkcyclemain();
    }
    public void PerlinNoiseButton() {
        islandSizeField.gameObject.SetActive(true);
        PerlinNoiseMain();
    }
    public void OnUpdateField(TMP_InputField field,Action<int> action, int max, int min = 0) {
        int input = SCR_utils.functions.validateIntFromString(field.text);

        input = Mathf.Clamp(input, min, max);

        field.text = input.ToString();

        action(input);
    }
    public void UpdateSize(int newValue) {
        width = newValue;
        height = newValue;
    }
    public void UpdateIteration(int newValue) {
        iterations = newValue;
    }
    public void UpdateIslandSize(int newValue) {
        islandSize = newValue;
    }
    public void Reload() {
        SceneManager.LoadScene(0);
    }
    #endregion
    #region Main
    private void PerlinNoiseMain() {
        Vector2 rand = new Vector2(UnityEngine.Random.Range(1,10000), UnityEngine.Random.Range(1, 10000));

        Texture2D texture = new Texture2D(width, height);

        for (int xTest = 0; xTest < width; xTest++) {
            for (int yTest = 0; yTest < width; yTest++) {
                int id = GetPerlinID(new Vector2(yTest, xTest), rand); //(Across x, Up y)

                if (id == 0) {
                    texture.SetPixel(yTest, xTest, backgroundPerlin);
                    //Debug.Log($"x: {x}, y: {y}, colour: {background}");
                }
                else {
                    //Base Colour
                    Color scaledColour = backgroundPerlin;

                    //Get HSV
                    float h;
                    float s;
                    float v;
                    Color.RGBToHSV(scaledColour, out h, out s, out v);

                    //Scale to 360
                    h *= 360;

                    //Get new hue using id
                    h = h - (scaleHueBy * id);

                    //
                    h = Mathf.Clamp(h, 0, 360);

                    //Back to normalised
                    h /= 360;

                    //Convert back
                    scaledColour = Color.HSVToRGB(h, s, v);

                    //Set
                    texture.SetPixel(yTest, xTest, scaledColour);

                    //Log
                    Debug.Log($"x: {xTest}, y: {yTest}, colour: {scaledColour}");
                }
            }
        }

        texture.filterMode = FilterMode.Point;

        texture.Apply();
        map.texture = texture;

        Camera.main.orthographicSize = 1;
    }
    private int GetPerlinID(Vector2 v, Vector2 rand) {
        float raw_perlin = Mathf.PerlinNoise(
            (v.x + rand.x) / islandSize,
            (v.y + rand.y) / islandSize
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * perlinScale;

        return Mathf.RoundToInt(scaled_perlin);
    }

    private void walkcyclemain() {
        //generate map, iterate until completed
        int rand = UnityEngine.Random.Range(1, 10000);

        Vector2Int currentpos = ReturnMid();
        Texture2D texture = new Texture2D(width, height);

        Color[] pixels = Enumerable.Repeat(backgroundRandomWalk, width * height).ToArray();
        texture.SetPixels(pixels);

        for (int i = 0; i < iterations; i++) {
            Vector2Int dir = ReturnRandomDir(rand);

            currentpos = currentpos + dir;
            bool atBounds = currentpos.x > width - 2 || currentpos.x < 1 || currentpos.y > height - 2 || currentpos.y < 1;
            
            if (atBounds) { //texture..containskey(currentpos + dir)
                currentpos = ReturnMid();
            }
            texture.SetPixel(currentpos.y, currentpos.x, tilesRandomWalk);
        }

        texture.filterMode = FilterMode.Point;

        texture.Apply();
        map.texture = texture;

        Camera.main.orthographicSize = 1;
    }

    private Vector2Int ReturnMid() {
        Vector2Int v = new Vector2Int((width - 1) / 2, (height - 1) / 2);
        return v;
    }

    private Vector2Int ReturnRandomDir(int randSeed) { //Make Seeded
        int i = UnityEngine.Random.Range(1, 5);
        switch (i)
        {
            case 1: return Vector2Int.left;
            case 2: return Vector2Int.right;
            case 3: return Vector2Int.up;
            case 4: return Vector2Int.down;
        }
        return Vector2Int.left;
    }

    #endregion
}
