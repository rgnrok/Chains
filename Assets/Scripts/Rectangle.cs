using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Random = UnityEngine.Random;

public class Rectangle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Id { get; set; }

    //Drag and drop
    private Vector3 offsetToMouse;
    private Vector3 prevPosition;
    private Collider2D[] overlap;

    //Relations
    private Dictionary<int, Rectangle> childs;
    private Dictionary<int, Rectangle> parents;

    private LineRenderer lineRender;
    private BoxCollider2D boxCollider;

    private void Awake() {
        lineRender = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        childs = new Dictionary<int, Rectangle>();
        parents = new Dictionary<int, Rectangle>();

        Color32 color = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);
        lineRender.startColor = color;
        lineRender.endColor = color;

        GetComponent<SpriteRenderer>().color = color;
    }


    public void AddParent(Rectangle relationRectangle) {
        parents.Add(relationRectangle.Id, relationRectangle);
    }

    public void AddChild(Rectangle relationRectangle) {
        DrawRelation(relationRectangle); //Update only new relation
        childs.Add(relationRectangle.Id, relationRectangle);
        relationRectangle.AddParent(this);
    }

    public void DrawParentRelations() {
        foreach (Rectangle parent in parents.Values) {
            parent.DrawChildRelations();
        }
    }

    public void DrawChildRelations() {
        lineRender.positionCount = childs.Count * 2;
        int i = 0;
        foreach (Rectangle child in childs.Values) {
            lineRender.SetPosition(i, transform.position);
            lineRender.SetPosition(i + 1, child.transform.position);
            i += 2;
        }
    }

    //Clear not completed relation
    public void ClearRelation() {
        lineRender.positionCount = childs.Count * 2;
    }

    //Draw not complted relation
    public void DrawRelation(Vector3 toPosition) {
        toPosition.z = 0;
        lineRender.positionCount = childs.Count * 2 + 2;
        lineRender.SetPosition(lineRender.positionCount - 2, transform.position);
        lineRender.SetPosition(lineRender.positionCount - 1, toPosition);
    }

    public void DrawRelation(Rectangle another) {
        DrawRelation(another.transform.position);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        offsetToMouse = transform.position - GameController.GetInstance().MousePositionToWord();
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = GameController.GetInstance().MousePositionToWord() + offsetToMouse;

        overlap = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max);
        if (overlap.Length > 1) {
            transform.position = prevPosition;
        }

        DrawChildRelations();
        DrawParentRelations();

        prevPosition = transform.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        offsetToMouse = Vector3.zero;
    }

    //Destroy rectangle and relations
    public void Destroy() {
        foreach (Rectangle parent in parents.Values) {
            parent.RemoveChild(this);
        }
        foreach (Rectangle child in childs.Values) {
            child.RemoveParent(this);
        }
        Destroy(this.gameObject);
    }

    public void RemoveChild(Rectangle child) {
        if (childs.ContainsKey(child.Id)) {
            childs.Remove(child.Id);
        }
        DrawChildRelations();
    }

    public void RemoveParent(Rectangle parent) {
        if (parents.ContainsKey(parent.Id)) {
            parents.Remove(parent.Id);
        }
    }
}
