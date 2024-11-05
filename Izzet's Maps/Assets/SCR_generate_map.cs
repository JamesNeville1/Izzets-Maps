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
using com.spacepuppy;

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

    [Tooltip("")][SerializeField]
    private int sizeMax;

    private int iterationsMax;
    private int islandSizeMax;

    [Tooltip("")][SerializeField]
    private Color background;

    [Tooltip("Should start in centre? If true begin in centre, if false begin with random tile")][SerializeField]
    private bool startInCentre;

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
        SCR_utils.monoFunctions.createButton("Walk Cycle", WalkCycleButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Perlin Noise", PerlinNoiseButton, buttonPrefab, buttonsParent);
        //SCR_utils.monoFunctions.createButton("Save As", DisplayAsImage, buttonPrefab, buttonsParent); //ToDo

        fieldParent = GameObject.Find("Fields");
        TMP_InputField sizeField = SCR_utils.monoFunctions.createField("Size Field", fieldPrefab, fieldParent);
        TMP_InputField iterationField = SCR_utils.monoFunctions.createField("Iteration Field", fieldPrefab, fieldParent);
        TMP_InputField islandSizeField = SCR_utils.monoFunctions.createField("Island Size Field", fieldPrefab, fieldParent);
        sizeField.onEndEdit.AddListener(delegate { OnUpdateField(sizeField, UpdateSize, sizeMax, 2); });
        iterationField.onEndEdit.AddListener(delegate { OnUpdateField(iterationField, UpdateIteration, iterationsMax); });
        islandSizeField.onEndEdit.AddListener(delegate { OnUpdateField(islandSizeField, UpdateIslandSize, islandSizeMax); });

        islandSize = 2;
        iterations = 2;

        islandSizeMax = 16000;
        iterationsMax = 16000;
    }
    #region Buttons
    public void WalkCycleButton() {
        //CreateBlank();
        //WalkCycleMain();
    }
    public void PerlinNoiseButton() {
        //CreateBlank();
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

        float yCounter = 0f;
        int x = 0;
        for (int i = 0; i < texture.GetPixels().Length; i++) {
            int y = Mathf.FloorToInt(Mathf.Round(yCounter * 10.0f) * .1f);

            Debug.Log($"Y int: {y}");
            
            int id = GetPerlinID(new Vector2(y, x), rand); //(Across x, Up y)

            if (id == 0) {
                texture.SetPixel(y, x, background);
                //Debug.Log($"x: {x}, y: {y}, colour: {background}");
            }
            else {
                //Base Colour
                Color scaledColour = background;

                //Get HSV
                float h;
                float s;
                float v;
                Color.RGBToHSV(scaledColour, out h, out s,  out v);

                //Scale to 360
                h *= 360;

                //Get new hue using id
                h =  h - (scaleHueBy * id);

                //
                h = Mathf.Clamp(h, 0, 360);

                //Back to normalised
                h /= 360;

                //Convert back
                scaledColour = Color.HSVToRGB(h, s, v);

                //Set
                texture.SetPixel(y, x, scaledColour);

                //Log
                //Debug.Log($"x: {x}, y: {y}, colour: {scaledColour}");
            }

            //texture.SetPixel(0, 0, Color.black);
            Debug.Log($"Y Counter Float {yCounter}");
            
            
            yCounter = yCounter + (1f / width); //
            x++;
            if (x > width -1) {
                x = 0;
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

        //if (scaled_perlin == perlinScale) {
        //    scaled_perlin = (perlinScale - 1);
        //}
        return Mathf.RoundToInt(scaled_perlin);
    }

    //private void WalkCycleMain() {
    //    //Generate Map, iterate until completed
    //    Vector2 currentPos = ReturnStartingPos();

    //    mapData[currentPos] = tileColours[1];

    //    int currentIslandSize = 0;
    //    for (int i = 0; i < iterations; i++) {
    //        Vector2 dir = ReturnRandomDir();
    //        if (mapData.ContainsKey(currentPos + dir)) {
    //            if(currentIslandSize <= islandSize) {
    //                currentPos = currentPos + dir;
    //                currentIslandSize++;
    //            }
    //            else {
    //                currentPos = ReturnRandomTile();
    //                currentIslandSize = 0;
    //            }
    //        }
    //        else {
    //            currentPos = ReturnRandomTile();
    //            currentIslandSize = 0;
    //        }
    //        mapData[currentPos] = tileColours[1];
    //    }
    //}
    //private Vector2 ReturnStartingPos() {
    //    Vector2 currentPos;
    //    if (startInCentre) currentPos = ReturnCentre();
    //    else currentPos = ReturnRandomTile();

    //    currentPos.x = MathF.Round(currentPos.x);
    //    currentPos.y = MathF.Round(currentPos.y);

    //    return currentPos;
    //}
    //private Vector2 ReturnCentre() {
    //    Vector2 v = (mapData.Keys.Last() + mapData.Keys.First())/2;
    //    return v;
    //}

    //private Vector2 ReturnRandomTile() {
    //    Vector2 v = mapData.ElementAt(UnityEngine.Random.Range(0, mapData.Count)).Key;
    //    return v;
    //}

    //private Vector2 ReturnRandomDir() {
    //    int i = UnityEngine.Random.Range(1, 5);
    //    switch (i) {
    //        case 1: return Vector2.left;
    //        case 2: return Vector2.right;
    //        case 3: return Vector2.up;
    //        case 4: return Vector2.down;
    //    }
    //    return Vector2.left;
    //}
    //private void createblank()
    //{
    //    if (size < 2) size = 2;
    //    height = size;
    //    width = size;
    //    for (int x = 1; x < width + 1; x++)
    //    {
    //        for (int y = 1; y < height + 1; y++)
    //        {
    //            vector2 tilepos = new vector2(x, y);
    //            mapdata.add(tilepos, tilecolours[0]);
    //        }
    //    }
    //}
    #endregion
}
