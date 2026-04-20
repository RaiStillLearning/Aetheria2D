using UnityEngine;
using UnityEngine.UI;
public class HeightTracker : MonoBehaviour
{
    public Transform player;
    public Text heightText;
    public Text bestText;
    public float groundY = -4f;
    private float bestHeight = 0f;
    void Update()
    {
        if (!player) return;
        float h = Mathf.Max(0f, player.position.y - groundY);
        if (h > bestHeight) bestHeight = h;
        if (heightText) heightText.text = "Height: " + h.ToString("F1") + "m";
        if (bestText)   bestText.text   = "Best: "   + bestHeight.ToString("F1") + "m";
    }
}