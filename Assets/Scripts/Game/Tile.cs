using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour {
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer hightlightSpriteRenderer;
    [SerializeField] private Color baseColor, offsetColor, nonHoverColor, hoverColor, PlaceableColor, NonPlaceableColor;
    [SerializeField] public GameObject DebugArrow;
    [SerializeField] public TMP_Text G;
    [SerializeField] public TMP_Text F;
    [SerializeField] public TMP_Text H;
    private bool isOffset;
    public Vector2 pos;
    private GameScript main;
    public bool isOccupied;
    public Tower tower;
    public bool isInteractable = true;
    public bool isInvisible = false;
    public bool isSelected = false;
    public void Init(GameScript main, bool isOffset, Transform parent, Vector2 pos, bool isOccupied)
    {
        this.isOffset = isOffset;
        spriteRenderer.color = isOffset ? offsetColor : baseColor;
        transform.SetParent(parent);
        this.isOccupied = isOccupied;
        this.pos = pos;
        this.main = main;
    }
    public void SetSpecialTile(bool isInteractable, bool isInvisible)
    {
        this.isInteractable = isInteractable;
        this.isInvisible = isInvisible;
        if (isInvisible)
        {
            spriteRenderer.color = new Color(0, 0, 0, 0);
        }
    }
    public void ResetColor()
    {
        spriteRenderer.color = isOffset ? offsetColor : baseColor;
    }
    void Start()
    {
    }

    void Update()
    {
    }
    public void SelectTile(bool selected)
    {
        if (!isInteractable)
        {
            return;
        }
        isSelected = selected;
        if (selected)
        {
            hightlightSpriteRenderer.enabled = true;
            hightlightSpriteRenderer.color = hoverColor;
        }
        else
        {
            hightlightSpriteRenderer.enabled = false;
        }
    }
    private void OnMouseEnter()
    {
        if (!isInteractable)
        {
            return;
        }
        if (!isSelected)
        {
            hightlightSpriteRenderer.enabled = true;
            hightlightSpriteRenderer.color = hoverColor;
        }
    }
    private void OnMouseExit()
    {
        if (!isInteractable)
        {
            return;
        }
        if (!isSelected)
        {
            hightlightSpriteRenderer.enabled = false;
        }
    }
    private void OnMouseOver()
    {
        if (!isInteractable)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            main.InteractTile(this);
        }
    }
}
