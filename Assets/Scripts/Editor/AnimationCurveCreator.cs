using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationCurveCreator : EditorWindow
{
    private Vector2 point0 = Vector2.zero;
    private Vector2 point1 = Vector2.zero;
    private Vector2 point2 = Vector2.one;
    private Vector2 point3 = Vector2.one;
    private AnimationCurve curve = new AnimationCurve();

    private List<Vector3> points = new List<Vector3>();

    [MenuItem("Tools/AnimationCurveCreator")]
    public static void Open()
    {
        var window = GetWindow<AnimationCurveCreator>();
        window.Convert();
    }

    public void OnGUI()
    {
        point0 = EditorGUILayout.Vector2Field("point0", point0);
        point1 = EditorGUILayout.Vector2Field("point1", point1);
        point2 = EditorGUILayout.Vector2Field("point2", point2);
        point3 = EditorGUILayout.Vector2Field("point3", point3);
        curve = EditorGUILayout.CurveField(curve);

        Handles.DrawPolyLine(points.ToArray());

        if (GUILayout.Button("変換"))
            Convert();
    }

    private void Convert()
    {
        Debug.LogError("convert");
        var start = point0;
        var end = point3;
        CalcCurveSlope(point0, point1, out var outTan0, out var outWeight0);
        CalcCurveSlope(point2, point3, out var inTan1, out var inWeight1);
        var keyFrame0 = new Keyframe(start.x, start.y, 0f, outTan0, 0f, outWeight0);
        var keyFrame1 = new Keyframe(end.x, end.y, inTan1, 0f, inWeight1, 0f);
        curve.ClearKeys();
        curve.AddKey(keyFrame0);
        curve.AddKey(keyFrame1);
        points.Clear();
        for (var frame = 0f; frame < 1f; frame += 0.001f)
            points.Add(new Vector3(frame * 800f + 10f, curve.Evaluate(frame) * -500f + 800f));
    }

    private void CalcCurveSlope(Vector2 p0, Vector2 p1, out float tan, out float weight)
    {
        var d = p1 - p0;
        var dxSign = Mathf.Sign(d.x);
        var dxAbs = dxSign * d.x;
        if (Mathf.Approximately(dxAbs, 0f))
        {
            dxAbs += 0.000001f;
            d.x = dxSign * dxAbs;
        }
        tan = d.y / d.x;
        weight = dxAbs;
    }
}
