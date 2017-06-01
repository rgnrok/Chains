using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField]
    GameObject rectangle;

    private Camera mainCamera;

    private float rectangleHalfWidth;
    private float rectangleHalfHeight;

    private int lastRectangleId;

    private float prevClickTime;

    private Rectangle currentRelationRectangle;

    private static GameController instance;
    public static GameController GetInstance() {
        if (instance == null) {
            instance = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }
        return instance;
    }


    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;

        BoxCollider2D collider = rectangle.GetComponent<BoxCollider2D>();
        rectangleHalfHeight = collider.size.y / 2;
        rectangleHalfWidth = collider.size.x / 2;

    }

    void Update() {

        if (Input.GetMouseButtonUp(0)) {
            //Double click
            if (Time.time - prevClickTime < 0.3f) {
                prevClickTime = 0;
                DestroyRectangleOnHit();
            }
            else {
                prevClickTime = Time.time;
                if (currentRelationRectangle != null) {
                    CreateRectangleRelation();
                }
                else {
                    if (CreateRectangle(MousePositionToWord())) {
                        prevClickTime = 0;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            if (currentRelationRectangle != null) {
                currentRelationRectangle.ClearRelation();
                currentRelationRectangle = null;
            }
            currentRelationRectangle = GetRectangleOnMousePosition();
        }

        if (currentRelationRectangle != null) {
            currentRelationRectangle.DrawRelation(MousePositionToWord());
        }
    }

    protected Rectangle GetRectangleOnMousePosition() {
        RaycastHit2D hit = Physics2D.Raycast(MousePositionToWord(), Vector2.zero);
        if (hit.collider != null) {
            return hit.collider.GetComponent<Rectangle>();
        }
        return null;
    }

    protected void CreateRectangleRelation() {
        Rectangle selectedRectangle = GetRectangleOnMousePosition();

        if (selectedRectangle != null) {
            currentRelationRectangle.AddChild(selectedRectangle);
        }
        else {
            currentRelationRectangle.ClearRelation();
        }
        currentRelationRectangle = null;

    }

    protected bool CreateRectangle(Vector3 position) {
        if (CanCreateRectangle(position)) {
            Rectangle rectangleInstance = Instantiate(rectangle, position, Quaternion.identity).GetComponent<Rectangle>();
            rectangleInstance.Id = lastRectangleId++;
            return true;
        }
        return false;
    }

    protected bool CanCreateRectangle(Vector3 centerPoint) {
        RaycastHit2D hit = Physics2D.Raycast(centerPoint, Vector2.zero);
        if (hit.collider != null) {
            return false;
        }

        Vector3 rectangleBoundsOffset = new Vector3(rectangleHalfWidth, rectangleHalfHeight, 0);
        Collider2D overlap = Physics2D.OverlapArea(centerPoint - rectangleBoundsOffset, centerPoint + rectangleBoundsOffset);

        return overlap == null;
    }

    protected void DestroyRectangleOnHit() {
        Rectangle selectedRectangle = GetRectangleOnMousePosition();
        if (selectedRectangle != null) {
            selectedRectangle.Destroy();
        }
    }

    public Vector3 MousePositionToWord() {
        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;

        return position;
    }

}
