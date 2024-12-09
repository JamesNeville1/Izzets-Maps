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

    [Tooltip("How many different colours?")][SerializeField]
    private int perlinScale;

    [Tooltip("Colour variation")][SerializeField]
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
    [Tooltip("Base colour, hue is scaled from here")][SerializeField]
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

    InputField sizeField; //TMP_InputField has formatting errors
    InputField inputField;
    TMP_Text currentGenerationSystemText;
    TMP_Text sizeMaxText;
    TMP_Text inputMaxText;

    //MAIN VARS HERE
    private int input;
    private int height;
    private int width;
    private GenerationID generationID = GenerationID.PERLIN_NOISE;

    #endregion

    private void Start()
    {
        buttonsParent = GameObject.Find("Buttons");
        SCR_utils.MonoFunctions.CreateButton("Random Walk", RandomWalkButton, buttonPrefab, buttonsParent);
        SCR_utils.MonoFunctions.CreateButton("Perlin Noise", PerlinNoiseButton, buttonPrefab, buttonsParent);
        SCR_utils.MonoFunctions.CreateButton("Generate", GenerateButton, buttonPrefab, buttonsParent);
        SCR_utils.MonoFunctions.CreateButton("Export", ExportButton, buttonPrefab, buttonsParent); //ToDo

        fieldParent = GameObject.Find("Fields");
        inputField = SCR_utils.MonoFunctions.CreateField("Input Field", fieldPrefab, fieldParent);
        sizeField = SCR_utils.MonoFunctions.CreateField("Size Field", fieldPrefab, fieldParent);
        inputField.onEndEdit.AddListener(delegate { OnUpdateField(inputField, UpdateInputField, inputMax); });
        sizeField.onEndEdit.AddListener(delegate { OnUpdateField(sizeField, UpdateSizeField, sizeMax, 2); });

        infoParent = GameObject.Find("Info");
        currentGenerationSystemText = SCR_utils.MonoFunctions.CreateText("", "Current Generation System", infoPrefab, infoParent);
        sizeMaxText = SCR_utils.MonoFunctions.CreateText("", "Size Max", infoPrefab, infoParent);
        inputMaxText = SCR_utils.MonoFunctions.CreateText("", "Input Max", infoPrefab, infoParent);

        PerlinNoiseButton();

        Camera.main.orthographicSize = 1;
    }

    #region UI
    public void RandomWalkButton()
    {
        GenerationSpecifics(GenerationID.RANDOM_WALK, randomWalkRestrictions);
    }
    public void PerlinNoiseButton()
    {
        GenerationSpecifics(GenerationID.PERLIN_NOISE, perlinNoiseRestrictions);
    }
    private void GenerationSpecifics(GenerationID id, GenerationSystemRestrictions restrictions)
    {
        generationID = id;

        inputMax = restrictions.inputMax;
        sizeMax = restrictions.sizeMax;

        inputField.onEndEdit.Invoke("");
        sizeField.onEndEdit.Invoke("");

        currentGenerationSystemText.text = generationID.ToString();
        inputMaxText.text = "Input Max: " + inputMax.ToString();
        sizeMaxText.text = "Size Max: " + sizeMax.ToString();
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
        if (map.texture != null)
        {
            Texture2D imageToPass = new Texture2D(map.texture.height, map.texture.width);
            Texture2D display = (Texture2D)map.texture;

            //-90 degrees rot
            for (int y = 0; y < map.texture.height; ++y)
            {
                for (int x = 0; x < map.texture.width; ++x)
                {
                    imageToPass.SetPixel(y, x, display.GetPixel(x, y));
                }
            }

            imageToPass.Apply();

            SCR_utils.Functions.ExportImage(imageToPass);
        }
    }
    public void OnUpdateField(InputField field,Action<int> action, int max, int min = 0) {
        int input = SCR_utils.Functions.ValidateIntFromString(field.text);

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

        //Loop through texture
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < width; y++) 
            {
                //Get ID
                int id = GetPerlinID(new Vector2(y, x), rand); //(Across x, Up y)

                if (id == 0) 
                {
                    //Normal Colour
                    texture.SetPixel(y, x, backgroundPerlin);
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

                    //Just in case, this shouldn't happen
                    h = Mathf.Clamp(h, 0, 360);

                    //Back to normalised
                    h /= 360;

                    //Convert back
                    scaledColour = Color.HSVToRGB(h, s, v);

                    //Set
                    texture.SetPixel(y, x, scaledColour);
                }
            }
        }

        //Apply
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        map.texture = texture;
    }
    private int GetPerlinID(Vector2 v, Vector2 rand) 
    {
        int magnification = input * (height / 10);

        //Calc Perlin
        float perlin = Mathf.PerlinNoise(
            (v.x + rand.x) / magnification,
            (v.y + rand.y) / magnification
        );
        
        //Scale to Range
        float scaledPerlin = perlin * perlinScale;

        return Mathf.RoundToInt(scaledPerlin);
    }

    #endregion

    #region Random Walk

    private void RandomWalkMain() 
    {
        //Setup
        Vector2Int currentpos = ReturnMid();
        Texture2D texture = new Texture2D(width, height);

        Color[] pixels = Enumerable.Repeat(backgroundRandomWalk, width * height).ToArray();
        texture.SetPixels(pixels);

        //Iterate
        for (int i = 0; i < input; i++) 
        {
            Vector2Int dir = ReturnRandomDir();

            currentpos = currentpos + dir;
            bool atBounds = currentpos.x > width - 2 || currentpos.x < 1 || currentpos.y > height - 2 || currentpos.y < 1;
            
            if (atBounds)
            {
                currentpos = ReturnMid();
            }
            texture.SetPixel(currentpos.y, currentpos.x, tilesRandomWalk);
        }

        //Apply
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        map.texture = texture;
    }

    private Vector2Int ReturnMid() 
    {
        Vector2Int v = new Vector2Int((width - 1) / 2, (height - 1) / 2);
        return v;
    }

    private Vector2Int ReturnRandomDir()
    {
        int i = UnityEngine.Random.Range(1, 5);
        switch (i)
        {
            case 1: return Vector2Int.left;
            case 2: return Vector2Int.right;
            case 3: return Vector2Int.up;
            case 4: return Vector2Int.down;
        }

        //This should not happen
        return Vector2Int.left;
    }

    #endregion
}
