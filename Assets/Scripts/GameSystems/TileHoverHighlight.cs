using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHoverHighlight : MonoBehaviour
{
    public Material highlightMaterial; 
    private Material[] originalMaterials;  
    private Renderer tileRenderer;
    private int occupiedLayer;
    private bool isHighlighted = false;

    private bool isPaused = false;

    void OnEnable()
    {
        // subscribe to onPause event
        FindObjectOfType<PauseMenu>().OnPause.AddListener(HandlePauseStateChange);
    }

    void OnDisable()
    {  
        if (FindObjectOfType<PauseMenu>())
        {
            // unsub from onPause
            FindObjectOfType<PauseMenu>().OnPause.RemoveListener(HandlePauseStateChange);
        }
    }

    private void HandlePauseStateChange(bool isPaused)
    {
        this.isPaused = isPaused;
    }

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();

        if (tileRenderer != null)
        {
            originalMaterials = tileRenderer.materials;
        }

        occupiedLayer = LayerMask.NameToLayer("OccupiedTiles");
    }
    void Update()
    {
        if (tileRenderer != null && gameObject.layer != occupiedLayer && !isPaused)
        {
            // perform a raycast from the camera to the mouse position
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            int layerMask = 1 << gameObject.layer; // layer mask for this tile's layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // check if the hit normal is pointing upwards (top face)
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.9f)
                    {
                        if (!isHighlighted)
                        {
                            Material[] highlightMaterials = new Material[tileRenderer.materials.Length];
                            for (int i = 0; i < highlightMaterials.Length; i++)
                            {
                                highlightMaterials[i] = highlightMaterial;
                            }
                            tileRenderer.materials = highlightMaterials;
                            isHighlighted = true;
                        }
                    }
                    else
                    {
                        // if the mouse is over the side, remove the highlight
                        if (isHighlighted)
                        {
                            tileRenderer.materials = originalMaterials;
                            isHighlighted = false;
                        }
                    }
                }
                else
                {
                    // mouse is not over this tile, remove highlight if necessary
                    if (isHighlighted)
                    {
                        tileRenderer.materials = originalMaterials;
                        isHighlighted = false;
                    }
                }
            }
            else
            {
                // no hit, remove highlight if necessary
                if (isHighlighted)
                {
                    tileRenderer.materials = originalMaterials;
                    isHighlighted = false;
                }
            }
        }
    }
}
