using UnityEngine;
using UnityEngine.InputSystem; // new input system

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(SpriteRenderer))] // Ensure we have a SpriteRenderer component
public class TerritorioHandler : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("The color to use when the mouse is hovering over this territory")]
    public Color hoverColor = new Color(1f, 0.5f, 0.5f, 1f); // Light red color for hover

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the original color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Convert mouse position to world space
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Check if the mouse overlaps this collider
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (spriteRenderer != null)
        {
            if (hit != null && hit.gameObject == gameObject)
            {
                // Change to hover color
                spriteRenderer.color = hoverColor;
            }
            else
            {
                // Revert to original color
                spriteRenderer.color = originalColor;
            }
        }
    }
}