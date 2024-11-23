using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public partial class BarChart : VisualElement
{

    private List<double> columnValues;
    private double MaxValue;
    private double MinValue;

    public void AddColumn(double value)
    {
        columnValues.Add(value);
        MaxValue = Math.Max(this.MaxValue, value);
        MinValue = Math.Min(this.MinValue, value);

        MarkDirtyRepaint();
    }

    public BarChart()
    {
        this.columnValues = new List<double>();
        this.MinValue = double.MaxValue;
        this.MaxValue = double.MinValue;
        generateVisualContent += DrawCanvas;
    }

    public BarChart(double MinValue)
    {
        this.columnValues = new List<double>();
        this.MinValue = MinValue;
        this.MaxValue = double.MinValue;
        generateVisualContent += DrawCanvas;
    }


    //void DrawCanvas(MeshGenerationContext ctx)
    //{
    //    if (columnValues.Count == 0)
    //        return;

    //    var painter = ctx.painter2D;
    //    painter.strokeColor = Color.white;
    //    painter.fillColor = new Color(0.49f, 0.81f, 0.98f);  // Light blue

    //    float chartWidth = this.resolvedStyle.width;
    //    float chartHeight = this.resolvedStyle.height;
    //    float barWidth = chartWidth * barWidthPct;

    //    for (int i = 0; i < columnValues.Count; i++)
    //    {
    //        float xCoordinateStart = i * (barWidth + 10);  // Gap between bars
    //        float barHeight = chartHeight * (float)(columnValues[i] / MaxValue);

    //        painter.BeginPath();
    //        painter.MoveTo(new Vector2(xCoordinateStart, chartHeight));
    //        painter.LineTo(new Vector2(xCoordinateStart, chartHeight - barHeight));
    //        painter.LineTo(new Vector2(xCoordinateStart + barWidth, chartHeight - barHeight));
    //        painter.LineTo(new Vector2(xCoordinateStart + barWidth, chartHeight));
    //        painter.ClosePath();
    //        painter.Fill();
    //        painter.Stroke();
    //    }
    //}

    void DrawCanvas(MeshGenerationContext ctx)
    {
        if (columnValues.Count == 0)
            return;

        var painter = ctx.painter2D;
        painter.strokeColor = Color.black;
        painter.fillColor = new Color(0.49f, 0.81f, 0.98f);  // Light blue

        float chartWidth = this.resolvedStyle.width;
        float chartHeight = this.resolvedStyle.height;

        float barWidth = chartWidth/columnValues.Count;

        for (int i = 0; i < columnValues.Count; i++)
        {
            float xCoordinateStart = i * barWidth;  // Gap between bars
            float barHeight = chartHeight * (float)(columnValues[i] / MaxValue);

            painter.BeginPath();
            painter.MoveTo(new Vector2(xCoordinateStart, chartHeight)); //bottom left corner
            painter.LineTo(new Vector2(xCoordinateStart, chartHeight - barHeight)); //upper left corner
            painter.LineTo(new Vector2(xCoordinateStart + barWidth, chartHeight - barHeight)); //upper right corner
            painter.LineTo(new Vector2(xCoordinateStart + barWidth, chartHeight)); //bottom right corner
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();

            // Draw the text
            string valueText = columnValues[i].ToString("0.##");
            Vector2 textPosition = new Vector2(xCoordinateStart + barWidth / 2, chartHeight - barHeight - 15); // Position above the bar
            ctx.DrawText(valueText, textPosition, 12, Color.black);
        }
    }
}