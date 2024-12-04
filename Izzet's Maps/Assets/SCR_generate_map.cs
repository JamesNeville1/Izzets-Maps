using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SCR_generate_map : MonoBehaviour {

    #region Structs & Enum

    [System.Serializable]
    struct GenerationSystemRestrictions
    {
        public int sizeMax;
        public int inputMax;
    };
    enum GenerationID
    {
        PERLIN_NOISE,
        RANDOM_WALK
    };

    #endregion

    #region Vars

    [Header("Require Dev Input")]
    [Tooltip("Tile needed to generate")][SerializeField]
    private RawImage map;

    [Tooltip("")][SerializeField]
    private int perlinScale;

    [Tooltip("")][SerializeField]
    private int scaleHueBy;
    //


    [Header("Max (No Extreme Loading)")]
    [Tooltip("")][SerializeField]
    private GenerationSystemRestrictions perlinNoiseRestrictions;
    [Tooltip("")][SerializeField]
    private GenerationSystemRestrictions randomWalkRestrictions;

    private int inputMax;
    private int sizeMax;
    //

    [Header("Colours")]
    [Tooltip("")][SerializeField]
    private Color backgroundPerlin;
    [Tooltip("")][SerializeField]
    private Color backgroundRandomWalk;
    [Tooltip("")][SerializeField]
    private Color tilesRandomWalk;

    //

    [Header("UI")]
    [Tooltip("")][SerializeField]
    private GameObject buttonsParent;
    [Tooltip("")][SerializeField]
    private GameObject buttonPrefab;
    [Tooltip("")][SerializeField]
    private GameObject fieldParent;
    [Tooltip("")][SerializeField]
    private GameObject fieldPrefab;
    [Tooltip("")][SerializeField]
    private GameObject infoParent;
    [Tooltip("")][SerializeField]
    private GameObject infoPrefab;

    TMP_InputField sizeField;
    TMP_InputField inputField;
    TMP_Text currentGenerationSystem;

    //MAIN VARS HERE
    private int input;
    private int height;
    private int width;
    private GenerationID generationID = GenerationID.PERLIN_NOISE;

    #endregion

    private void Start()
    {
        buttonsParent = GameObject.Find("Buttons");
        SCR_utils.monoFunctions.createButton("Random Walk", RandomWalkButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Perlin Noise", PerlinNoiseButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Generate", GenerateButton, buttonPrefab, buttonsParent);
        SCR_utils.monoFunctions.createButton("Export", ExportButton, buttonPrefab, buttonsParent); //ToDo

        fieldParent = GameObject.Find("Fields");
        sizeField = SCR_utils.monoFunctions.createField("Size Field", fieldPrefab, fieldParent);
        inputField = SCR_utils.monoFunctions.createField("Input Field", fieldPrefab, fieldParent);
        sizeField.onEndEdit.AddListener(delegate { OnUpdateField(sizeField, UpdateSizeField, sizeMax, 2); });
        inputField.onEndEdit.AddListener(delegate { OnUpdateField(inputField, UpdateInputField, inputMax); });

        infoParent = GameObject.Find("Info");
        currentGenerationSystem = SCR_utils.monoFunctions.createText("", "Current Generation System", infoPrefab, infoParent);

        PerlinNoiseButton();
    }

    #region UI
    public void RandomWalkButton()
    {
        generationID = GenerationID.RANDOM_WALK;

        inputMax = randomWalkRestrictions.inputMax;
        sizeMax = randomWalkRestrictions.sizeMax;

        inputField.onEndEdit.Invoke("");
        sizeField.onEndEdit.Invoke("");

        currentGenerationSystem.text = generationID.ToString();
    }
    public void PerlinNoiseButton()
    {
        generationID = GenerationID.PERLIN_NOISE;

        inputMax = perlinNoiseRestrictions.inputMax;
        sizeMax = perlinNoiseRestrictions.sizeMax;

        inputField.onEndEdit.Invoke("");
        sizeField.onEndEdit.Invoke("");

        currentGenerationSystem.text = generationID.ToString();
    }
    public void GenerateButton()
    {
        switch (generationID)
        {
            case GenerationID.PERLIN_NOISE:
                PerlinNoiseMain();
                break;
            case GenerationID.RANDOM_WALK:
                RandomWalkMain();
                break;
        }
    }
    public void ExportButton()
    {
        SCR_utils.functions.InitiateDownload();
    }
    public void OnUpdateField(TMP_InputField field,Action<int> action, int max, int min = 0) {
        int input = SCR_utils.functions.validateIntFromString(field.text);

        input = Mathf.Clamp(input, min, max);

        field.text = input.ToString();

        action(input);
    }
    public void UpdateSizeField(int newValue) 
    {
        width = newValue;
        height = newValue;
    }
    public void UpdateInputField(int newValue) 
    {
        input = newValue;
    }
    #endregion

    #region Perlin Noise

    private void PerlinNoiseMain() 
    {
        Vector2 rand = new Vector2(UnityEngine.Random.Range(1,10000), UnityEngine.Random.Range(1, 10000));

        Texture2D texture = new Texture2D(width, height);

        for (int xTest = 0; xTest < width; xTest++) 
        {
            for (int yTest = 0; yTest < width; yTest++) 
            {
                int id = GetPerlinID(new Vector2(yTest, xTest), rand); //(Across x, Up y)

                if (id == 0) 
                {
                    texture.SetPixel(yTest, xTest, backgroundPerlin);
                    //Debug.Log($"x: {x}, y: {y}, colour: {background}");
                }
                else 
                {
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
                    //Debug.Log($"x: {xTest}, y: {yTest}, colour: {scaledColour}");
                }
            }
        }

        texture.filterMode = FilterMode.Point;

        texture.Apply();
        map.texture = texture;

        Camera.main.orthographicSize = 1;
    }
    private int GetPerlinID(Vector2 v, Vector2 rand) 
    {
        float raw_perlin = Mathf.PerlinNoise(
            (v.x + rand.x) / input,
            (v.y + rand.y) / input
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * perlinScale;

        return Mathf.RoundToInt(scaled_perlin);
    }

    #endregion

    #region Random Walk

    private void RandomWalkMain() 
    {
        //generate map, iterate until completed
        int rand = UnityEngine.Random.Range(1, 10000);

        Vector2Int currentpos = ReturnMid();
        Texture2D texture = new Texture2D(width, height);

        Color[] pixels = Enumerable.Repeat(backgroundRandomWalk, width * height).ToArray();
        texture.SetPixels(pixels);

        for (int i = 0; i < input; i++) 
        {
            Vector2Int dir = ReturnRandomDir(rand);

            currentpos = currentpos + dir;
            bool atBounds = currentpos.x > width - 2 || currentpos.x < 1 || currentpos.y > height - 2 || currentpos.y < 1;
            
            if (atBounds) //texture..containskey(currentpos + dir)
            {
                currentpos = ReturnMid();
            }
            texture.SetPixel(currentpos.y, currentpos.x, tilesRandomWalk);
        }

        texture.filterMode = FilterMode.Point;

        texture.Apply();
        map.texture = texture;

        Camera.main.orthographicSize = 1;
    }

    private Vector2Int ReturnMid() 
    {
        Vector2Int v = new Vector2Int((width - 1) / 2, (height - 1) / 2);
        return v;
    }

    private Vector2Int ReturnRandomDir(int randSeed) //Make Seeded
    {
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
