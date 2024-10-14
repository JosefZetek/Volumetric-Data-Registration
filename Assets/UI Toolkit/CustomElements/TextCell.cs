using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TextCell : VisualElement
{
    private Color DEFAULT_BORDER_COLOR = Color.black;
    private const string DEFAULT_VALUE = "0";
    private bool editMode;
    private Label label;

    public TextCell(int size, bool isRelative)
    {
        SetDefaultValues();
        MakeView(size, isRelative);
    }

    private void SetDefaultValues()
    {
        this.label = new Label();
        //label.style.fontSize = new StyleLength(1);
        this.label.text = DEFAULT_VALUE;
        this.editMode = false;
    }

    private void MakeView(int size, bool isRelative)
    {
        SetParameters(this, size, isRelative);
        this.style.backgroundColor = Color.clear;

        label.style.backgroundColor = Color.white;
        SetBorder(this, 1, DEFAULT_BORDER_COLOR);
        SetParameters(label, 100, isRelative);
        this.Add(label);
    }

    private void SetBorder(VisualElement element, int borderWidth, Color borderColor)
    {
        element.style.borderTopWidth = borderWidth;
        element.style.borderBottomWidth = borderWidth;
        element.style.borderRightWidth = borderWidth;
        element.style.borderLeftWidth = borderWidth;

        element.style.borderTopColor = borderColor;
        element.style.borderBottomColor = borderColor;
        element.style.borderRightColor = borderColor;
        element.style.borderLeftColor = borderColor;
    }

    private void SetParameters(VisualElement element, int size, bool isRelative)
    {
        /* Sets padding to 0 */
        element.style.paddingTop = 0;
        element.style.paddingBottom = 0;
        element.style.paddingRight = 0;
        element.style.paddingLeft = 0;

        /* Sets margin to 0 */
        element.style.marginTop = 0;
        element.style.marginBottom = 0;
        element.style.marginRight = 0;
        element.style.marginLeft = 0;

        StyleLength styleLength = isRelative ? new StyleLength(Length.Percent(size)) : size;

        element.style.width = styleLength;
        element.style.height = styleLength;

        element.style.flexWrap = Wrap.Wrap;
        element.style.justifyContent = Justify.Center;
        element.style.alignItems = Align.Center;
    }

    public bool IsEditing()
    {
        return editMode;
    }

    public void ToggleEditMode()
    {
        editMode = !editMode;

        if (editMode)
        {
            SetBorder(label, 1, Color.red);
            return;
        }

        SetBorder(label, 1, DEFAULT_BORDER_COLOR);


        double value;
        bool success = Double.TryParse(label.text, out value);


        label.text = success ? value.ToString() : DEFAULT_VALUE;
    }

    public void AddContent(char appendedCharacter)
    {
        if (!char.IsLetterOrDigit(appendedCharacter) && !char.IsPunctuation(appendedCharacter))
            return;

       label.text += (appendedCharacter == ',') ? "." : appendedCharacter;
    }

    public void RemoveLastElement()
    {
        Debug.Log("Removing last element");

        if (label.text.Length == 0)
            return;

        label.text = label.text.Remove(label.text.Length - 1);
    }

    public string GetContent()
    {
        return label.text;
    }

}
