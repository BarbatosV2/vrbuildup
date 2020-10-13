using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RadialMenu : MonoBehaviour
{
    [Header("Scene")]
    public SelectionNodes selectionTransform = null;
    public Transform cursorTransform = null;

    public Hand interactingHand;

    [Header("Events")]
    public RadialSection one = null;
    public RadialSection two = null;
    public RadialSection three = null;
    public RadialSection four = null;
    public RadialSection five = null;

    private Vector2 touchPosition = Vector2.zero;
    private List<RadialSection> radialSections = null;
    private RadialSection highlightedSection = null;

    private readonly float degreeIncrement = 72.0f;

    private void Awake()
    {
        //interactingHand = transform.parent.GetComponent<Hand>();
        CreateAndSetupSections();
    }

    private void CreateAndSetupSections()
    {
        radialSections = new List<RadialSection>()
       {
            one,
            two,
            three,
            four,
            five
       };
    }

    private void Start()
    {
        Show(false);
    }

    public void Show(bool value)
    {
        gameObject.SetActive(value);
    }

    private void Update()
    {
        Vector2 direction = Vector2.zero + touchPosition;
        float rotation = GetDegree(direction);

        SetCursorPosition();
        SetSelectionRotation(rotation);
        SetSelectedEvent(rotation);
    }

    private float GetDegree(Vector2 direction)
    {
        float value = Mathf.Atan2(direction.x, direction.y);
        value *= Mathf.Rad2Deg;

        if(value < 0)
        {
            value += 360.0f;
        }
        return value;
    }

    private void SetCursorPosition()
    {
        cursorTransform.localPosition = touchPosition;
    }

    private void SetSelectionRotation(float newRotation)
    {
       // float snappedRotation = SnapRotation(newRotation);
       // selectionTransform.localEulerAngles = new Vector3(0, 0, -snappedRotation);
    }

    private float SnapRotation(float rotation)
    {
        return GetNearestIncrement(rotation) * degreeIncrement;
    }

    private int GetNearestIncrement(float rotation)
    {
        return Mathf.RoundToInt(rotation / degreeIncrement);
    }

    private void SetSelectedEvent(float currentRotation)
    {
        int index = GetNearestIncrement(currentRotation);

        if(index == 5)
        {
            index = 0;
        }
        print(index);
        selectionTransform.Hide();
        selectionTransform.Show(index, true);
        highlightedSection = radialSections[index];
    }

    public void SetTouchPosition(Vector2 newValue)
    {
        touchPosition = newValue;
    }

    public void ActivateHighlightedSection()
    {
        highlightedSection.onPress.Invoke();
    }
}
