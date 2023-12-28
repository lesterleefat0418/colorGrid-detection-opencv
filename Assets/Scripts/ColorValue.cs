using UnityEngine;
using UnityEngine.UI;

public class ColorValue : MonoBehaviour
{
    public Text color, differentValue_1, differentValue_2, differentValue_3, H, S, V;

    public void setDifferentValue(string _color, float _value1, float _value2, float _value3)
    {
        if(this.color != null) this.color.text = _color;
        if(differentValue_1 != null) differentValue_1.text = "R: " + _value1.ToString("0.00");
        if(differentValue_2 != null) differentValue_2.text = "B: " + _value2.ToString("0.00");
        if(differentValue_3 != null) differentValue_3.text = "Y: " + _value3.ToString("0.00");
    }

    public void setHSVValue(string _color, float _h, float _s, float _v)
    {
        if (this.color != null) this.color.text = _color;
        if (H != null) H.text = "h: " + _h.ToString("0.00");
        if (S != null) S.text = "s: " + _s.ToString("0.00");
        if (V != null) V.text = "v: " + _v.ToString("0.00");
    }

}
