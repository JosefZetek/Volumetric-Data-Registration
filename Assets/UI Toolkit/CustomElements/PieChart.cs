using UnityEngine;
using UnityEngine.UIElements;

public partial class PieChart : VisualElement
{
    private float radius = 100.0f;
    private float progress = 0.0f;


    public float diameter => radius * 2.0f;

    public float value
    {
        get { return progress; }
        set { progress = value; MarkDirtyRepaint(); }
    }

    public PieChart()
    {
        generateVisualContent += DrawCanvas;
    }

    void DrawCanvas(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        painter.strokeColor = Color.white;
        painter.fillColor = Color.white;

        var percentage = progress;

        var percentages = new float[] {
            percentage, 100 - percentage
        };
        var colors = new Color32[] {
            new Color32(182,235,122,255),
            new Color32(251,120,19,255)
        };
        float angle = 0.0f;
        float anglePct = 0.0f;
        int k = 0;
        foreach (var pct in percentages)
        {
            anglePct += 360.0f * (pct / 100);

            painter.fillColor = colors[k++];
            painter.BeginPath();
            painter.MoveTo(new Vector2(radius, radius));
            painter.Arc(new Vector2(radius, radius), radius, angle, anglePct);
            painter.Fill();

            angle = anglePct;
        }
    }
}