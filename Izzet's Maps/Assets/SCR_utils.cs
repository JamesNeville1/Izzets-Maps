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
using static UnityEngine.GraphicsBuffer;

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
        public static float ConvertBetweenScales_original(float old_value, float first_scale_min, float first_scale_max, float second_scale_min, float second_scale_max)
        {
            /** Given a chosen value on one scale, find it's equivalent value on another scale. **/

            float first_scale_length = first_scale_max - first_scale_min;
            float second_scale_length = second_scale_max - second_scale_min;

            // Shift to Origin
            float offset_value = old_value - first_scale_min;
            // Normalise
            float normalised_value = offset_value / first_scale_length;
            // Upscale
            float upscaled_value = normalised_value * second_scale_length;
            // Shift from Origin
            float new_value = upscaled_value + second_scale_min;

            return new_value;
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
    }
}
