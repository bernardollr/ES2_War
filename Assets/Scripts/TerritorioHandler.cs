using UnityEngine;
using UnityEngine.InputSystem; // new input system
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(SpriteRenderer))] // Ensure we have a SpriteRenderer component
public class TerritorioHandler : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("The color to use when the mouse is hovering over this territory")]
    public Color hoverColor = new Color(1f, 0.5f, 0.5f, 1f); // Light red color for hover

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool wasHovering = false;

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
        bool isHovering = (hit != null && hit.gameObject == gameObject);

        if (spriteRenderer != null)
        {
            if (isHovering)
            {
                // Change to hover color
                spriteRenderer.color = hoverColor;

                // Print colliding objects only once when starting to hover
                if (!wasHovering)
                {
                    PrintCollidingObjects();
                }
            }
            else
            {
                // Revert to original color
                spriteRenderer.color = originalColor;
            }
        }

        wasHovering = isHovering;
    }

    void PrintCollidingObjects()
    {
        // Get all colliders that are touching this object's collider
        List<Collider2D> collidingColliders = new List<Collider2D>();
        ContactFilter2D filter = ContactFilter2D.noFilter;

        // Use OverlapCollider to find all colliders touching this one
        GetComponent<Collider2D>().Overlap(filter, collidingColliders);

        // Remove self from the list
        collidingColliders.RemoveAll(collider => collider.gameObject == gameObject);

        if (collidingColliders.Count > 0)
        {
            Debug.Log($"Objects colliding with {gameObject.name}:");
            foreach (Collider2D collider in collidingColliders)
            {
                Debug.Log($"- {collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} is not colliding with any other objects.");
        }
    }

    // Alternative method using OnCollision/OnTrigger events (commented out)
    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{gameObject.name} started colliding with: {other.gameObject.name}");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"{gameObject.name} stopped colliding with: {other.gameObject.name}");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{gameObject.name} started colliding with: {collision.gameObject.name}");
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"{gameObject.name} stopped colliding with: {collision.gameObject.name}");
    }
    */
}