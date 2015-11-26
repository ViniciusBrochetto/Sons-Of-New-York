using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderToText : MonoBehaviour {

    public Slider slider;
    public InputField textField;
    public string convertionMask;
	
	void Update ()
    {
        if (!textField.isFocused)
            textField.text = slider.value.ToString(convertionMask);
        else
        {
            if (float.Parse(textField.text) < slider.minValue)
            {
                textField.text = slider.minValue.ToString(convertionMask);
            }

            if (float.Parse(textField.text) > slider.maxValue)
            {
                textField.text = slider.maxValue.ToString(convertionMask);
            }

            slider.value = float.Parse(textField.text);
        }
    }
}
