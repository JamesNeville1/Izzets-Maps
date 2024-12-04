using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SCR_utils {
    public class customAttributes {
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
    public class functions {
        public static int validateIntFromString(string invalid, int max = 10000) {
            int valid = 0;
            string validString = "";

            for (int i = 0; i < invalid.Length; i++) {
                if (char.IsNumber(invalid[i])) { 
                    validString += invalid[i];
                }
            }

            if (validString.Length > 0) {
                if(!int.TryParse(validString, out valid)) {
                    valid = max;
                }
            }
            else valid = 0;

            return valid;
        }

        public static void InitiateDownload()
        {
            ScreenCapture.CaptureScreenshot("test.png");
        }

    }
    public class monoFunctions : MonoBehaviour {
        public static void createButton(string name, Action onClick, GameObject prefab, GameObject parent) {
            Button newButton = Instantiate(prefab, parent.transform).GetComponent<Button>();
            newButton.gameObject.name = name + " Button";
            newButton.onClick.AddListener(delegate { onClick(); });
            newButton.transform.SetParent(parent.transform);
            newButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = name;
        }
        public static TMP_InputField createField(string name, GameObject prefab, GameObject parent, Action onEndExit = null) {
            TMP_InputField newField = Instantiate(prefab, parent.transform).GetComponent<TMP_InputField>();
            newField.gameObject.name = name + " Field";
            if(onEndExit != null) newField.onEndEdit.AddListener(delegate { onEndExit(); });
            newField.transform.SetParent(parent.transform);
            newField.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = name;
            return newField;
        }
        public static TMP_Text createText(string info, string inspectorName, GameObject prefab, GameObject parent)
        {
            TMP_Text newText = Instantiate(prefab, parent.transform).GetComponent<TMP_Text>();
            newText.gameObject.name = inspectorName + " Text";
            newText.text = info;
            return newText;
        }
    }
}
