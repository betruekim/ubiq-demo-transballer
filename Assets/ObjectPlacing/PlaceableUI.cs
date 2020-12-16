using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubik.Samples;
using Ubik.Messaging;

namespace Transballer.PlaceableObjects
{
    public class PlaceableUI : MonoBehaviour
    {
        // list out variables and types that need to be controlled
        // auto generate ui for it
        // define custom logic for how ui reacts to updates to variables
        // update variables in placable on change
        // define custom logic for placable on variable update

        public Dictionary<int, string> booleans = new Dictionary<int, string>(); // string holds the name of the variable to change in the placable
        public Dictionary<int, (string, float, float)> floats = new Dictionary<int, (string, float, float)>();
        int count = 0;

        Dictionary<string, Toggle> booleanUIs;
        Dictionary<string, (Slider, Text)> floatUIs;

        public Placeable placeable;
        public System.Type placeableType;

        public Text title;
        public Transform container;
        public GameObject floatPrefab;
        public GameObject boolPrefab;

        public delegate void UpdatedUI();
        public event UpdatedUI OnUiUpdate;

        public void GenerateUI()
        {
            booleanUIs = new Dictionary<string, Toggle>();
            floatUIs = new Dictionary<string, (Slider, Text)>();
            int i = 0;
            while (true)
            {
                if (booleans.ContainsKey(i))
                {
                    GameObject booleanItem = GameObject.Instantiate(boolPrefab, container);
                    booleanItem.GetComponentInChildren<Text>().text = booleans[i];
                    booleanUIs[booleans[i]] = booleanItem.GetComponentInChildren<Toggle>();
                }
                else if (floats.ContainsKey(i))
                {
                    GameObject floatItem = GameObject.Instantiate(floatPrefab, container);
                    Text[] labels = floatItem.GetComponentsInChildren<Text>();
                    labels[0].text = floats[i].Item1;
                    Slider slider = floatItem.GetComponentInChildren<Slider>();
                    slider.minValue = floats[i].Item2;
                    slider.maxValue = floats[i].Item3;
                    floatUIs[floats[i].Item1] = (slider, labels[1]);
                }
                else
                {
                    break;
                }
                i++;
            }

            // set each items value
            foreach (var varName in booleanUIs.Keys)
            {
                FieldInfo info = placeableType.GetField(varName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Debug.Log(varName);
                Debug.Log(info);
                bool value = (bool)info.GetValue(placeable);
                booleanUIs[varName].isOn = value;
                booleanUIs[varName].onValueChanged.AddListener((bool val) => { ValueChangeListener(varName, val); });
            }

            // set each items value
            foreach (var varName in floatUIs.Keys)
            {
                FieldInfo info = placeableType.GetField(varName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                float value = (float)info.GetValue(placeable);
                floatUIs[varName].Item1.value = value;
                floatUIs[varName].Item1.onValueChanged.AddListener((float val) => { ValueChangeListener(varName, val); });
            }
        }

        public void AddBoolean(string name)
        {
            booleans[count] = name;
            count++;
        }

        public void AddFloat(string name, float min, float max)
        {
            floats[count] = (name, min, max);
            count++;
        }

        void ValueChangeListener(string name, float value)
        {
            placeable.UISendMessage($"UI$float${name}${value}");
            SetValue(name, value);
        }

        void ValueChangeListener(string name, bool value)
        {
            placeable.UISendMessage($"UI$bool${name}${value}");
            SetValue(name, value);
        }


        public void SetValue(string name, float value)
        {
            FieldInfo info = placeableType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            info.SetValue(placeable, value);
            // TODO test tomorrow - does removing this just leave the UI out of sync? and the infinite updates stops?
            floatUIs[name].Item1.value = value;
            floatUIs[name].Item2.text = $"{Mathf.Round(value * 10) / 10f}";
        }

        public void SetValue(string name, bool value)
        {
            FieldInfo info = placeableType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            info.SetValue(placeable, value);
            booleanUIs[name].isOn = value;
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string[] components = message.ToString().Split('$');
            string type = components[1];
            string name = components[2];
            string val = components[3];
            switch (type)
            {
                case "float":
                    float floatParsed = float.Parse(val);
                    SetValue(name, floatParsed);

                    break;
                case "bool":
                    bool boolParsed = bool.Parse(val);
                    SetValue(name, boolParsed);
                    break;
                default:
                    throw new System.Exception($"unknown type {type}");
            }
            OnUiUpdate?.Invoke();
        }
    }
}