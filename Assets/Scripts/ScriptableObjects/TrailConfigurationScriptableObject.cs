using UnityEngine;

[CreateAssetMenu(fileName = "Trail Configuration", menuName ="Guns/Trail Configuration", order = 3)]
public class TrailConfigurationScriptableObject : ScriptableObject
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration;
    public float MinVertexDistance;
    public Gradient color;
    public float MissDistance;
    public float SimulationSpeed;
}
