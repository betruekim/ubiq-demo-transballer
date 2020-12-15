using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Transballer.PlaceableObjects
{
    public class ElectromagnetMenu : MonoBehaviour
    {
        public Toggle timerToggle;
        public Toggle powerToggle;
        public Slider onSlider;
        public Slider offSlider;
        public Text onLabel;
        public Text offLabel;

        public bool timerController = true;
        public bool manualOn = false;
        public float onDuration = 1;
        public float offDuration = 1;

        public delegate void ElectromagnetUpdate(bool timerController, bool manualOn, float onDuration, float offDuration);
        public event ElectromagnetUpdate onElectromagnetUpdate;

        private void OnEnable()
        {
            timerToggle.onValueChanged.AddListener((bool toggle) => { timerController = toggle; SendUpdate(); });
            powerToggle.onValueChanged.AddListener((bool toggle) => { manualOn = toggle; SendUpdate(); });
            onSlider.onValueChanged.AddListener((float duration) => { onDuration = duration; SendUpdate(); });
            offSlider.onValueChanged.AddListener((float duration) => { offDuration = duration; SendUpdate(); });
        }

        private void OnDisable()
        {
            timerToggle.onValueChanged.RemoveAllListeners();
            powerToggle.onValueChanged.RemoveAllListeners();
            onSlider.onValueChanged.RemoveAllListeners();
            offSlider.onValueChanged.RemoveAllListeners();
        }

        void SendUpdate()
        {
            powerToggle.interactable = !timerController;
            onSlider.interactable = timerController;
            offSlider.interactable = timerController;
            onElectromagnetUpdate?.Invoke(timerController, manualOn, onDuration, offDuration);
        }
    }
}
