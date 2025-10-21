using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundImage;
    [SerializeField] private float parallaxStrength = 15f;
    private Vector2 initialPosition;

    private void Start()
    {
        if (backgroundImage != null) {
            initialPosition = backgroundImage.anchoredPosition;
        }
    }

    private void Update()
    {
        if (backgroundImage != null) {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float percentX = mousePosition.x / Screen.width - 0.5f;
            float percentY = mousePosition.y / Screen.height - 0.5f;

            float newX = initialPosition.x - (percentX * parallaxStrength);
            float newY = initialPosition.y - (percentY * parallaxStrength);

            Vector2 newPosition = new Vector2(newX, newY);

            backgroundImage.anchoredPosition = Vector2.Lerp(backgroundImage.anchoredPosition, newPosition, Time.deltaTime * 5f);
        }
    }
}
