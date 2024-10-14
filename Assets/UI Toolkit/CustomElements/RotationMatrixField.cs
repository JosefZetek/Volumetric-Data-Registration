using UnityEngine.UIElements;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using System;
using System.Reflection.Emit;

public class RotationMatrixField : VisualElement
{
    private const int DIMENSION = 3;

    TextCell[][] textCells;
    Matrix<double> inputMatrix;

    TextCell selectedCell;

    public RotationMatrixField(int size, bool isRelative)
    {
        SetDefaultValues();
        SetEventHandlers();
        MakeView(size, isRelative);
    }

    private void SetEventHandlers()
    {
        this.RegisterCallback<ClickEvent>(evt => CellClicked(evt));
        this.RegisterCallback<KeyDownEvent>(evt => ButtonPressed(evt));
    }

    private void ButtonPressed(KeyDownEvent evt)
    {
        if (selectedCell == null)
            return;

        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            selectedCell.ToggleEditMode();
            Debug.Log("Cell content is: " + selectedCell.GetContent());
            selectedCell = null;
            return;
        }

        if (evt.keyCode == KeyCode.Backspace)
        {
            selectedCell.RemoveLastElement();
            return;
        }


        selectedCell.AddContent(evt.character);
    }

    private void CellClicked(ClickEvent evt)
    {
        float widthSegment = this.resolvedStyle.width/DIMENSION;
        float heightSegment = this.resolvedStyle.height/DIMENSION;

        int i = (int)(evt.localPosition.y / heightSegment);
        int j = (int)(evt.localPosition.x / widthSegment);

        Debug.Log("This is selected cell " + i + ";" + j);

        if (selectedCell != null)
            selectedCell.ToggleEditMode();

        if (!textCells[i][j].IsEditing())
            textCells[i][j].ToggleEditMode();

        selectedCell = textCells[i][j];
    }


    /// <summary>
    /// Sets default transformation to Identity
    /// </summary>
    private void SetDefaultValues()
    {
        inputMatrix = Matrix<double>.Build.DenseIdentity(DIMENSION);
        textCells = new TextCell[DIMENSION][];
        selectedCell = null;
        this.focusable = true;

        this.tooltip = "Click to enter edit mode. After inserting value, press Enter.";
    }

    private void MakeView(int size, bool isRelative)
    {

        this.style.flexWrap = Wrap.Wrap;
        this.style.justifyContent = Justify.Center;
        this.style.alignItems = Align.Center;
        this.style.alignSelf = Align.Center;
        this.style.flexDirection = FlexDirection.Row;

        SetSize(this, size, isRelative);

        for(int i = 0; i<DIMENSION; i++)
        {
            textCells[i] = new TextCell[DIMENSION];
            for(int j = 0; j<DIMENSION; j++)
            {
                textCells[i][j] = new TextCell(100 / DIMENSION, true);
                this.Add(textCells[i][j]);
            }
        }
    }

    private void SetSize(VisualElement element, int size, bool isRelative)
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
    }
}
