using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class SCR_utils {
    public class Functions {
        public static int ValidateIntFromString(string invalid) {
            int valid = 0;
            string validString = "";

            for (int i = 0; i < invalid.Length; i++) {
                if (char.IsNumber(invalid[i])) { 
                    validString += invalid[i];
                }
            }

            if (validString.Length > 0) {
                if(!int.TryParse(validString, out valid)) {
                    valid = int.MaxValue; //Input Cap, if I needed more I would convert to using "long" value
                }
            }
            else valid = 0;

            return valid;
        }

        [DllImport("__Internal")]
        private static extern void DownloadImage(string base64Data, int length, string fileName); //.jslib handle

        public static void ExportImage(Texture2D texture)
        {
            //Convert the Texture2D to a PNG byte array
            byte[] imageData = texture.EncodeToPNG();
            imageData.Reverse();

            //Convert the byte array to a Base64 string
            string base64Image = System.Convert.ToBase64String(imageData);

            //Call the JavaScript function to trigger the download
            DownloadImage(base64Image, base64Image.Length, "MapOutput.png"); //Only works on web, throws error in inspector
        }
    }
    public class MonoFunctions : MonoBehaviour {
        public static void CreateButton(string name, Action onClick, GameObject prefab, GameObject parent) {
            Button newButton = Instantiate(prefab, parent.transform).GetComponent<Button>();
            newButton.gameObject.name = name + " Button";
            newButton.onClick.AddListener(delegate { onClick(); });
            newButton.transform.SetParent(parent.transform);
            newButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = name;
        }
        public static InputField CreateField(string name, GameObject prefab, GameObject parent, Action onEndExit = null) {
            InputField newField = Instantiate(prefab, parent.transform).GetComponent<InputField>();
            newField.gameObject.name = name + " Field";
            newField.transform.SetParent(parent.transform);
            newField.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = name;
            return newField;
        }
        public static TMP_Text CreateText(string info, string inspectorName, GameObject prefab, GameObject parent)
        {
            TMP_Text newText = Instantiate(prefab, parent.transform).GetComponent<TMP_Text>();
            newText.gameObject.name = inspectorName + " Text";
            newText.text = info;
            return newText;
        }
    }
}
