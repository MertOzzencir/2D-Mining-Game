using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop SO", menuName = "Create New Drop Data/New Drop Data")]
public class DropSO : ScriptableObject
{
    public Mesh Mesh;
    public Material Material;
    public DropType DropType;
    public Sprite UI_Image;
    public string UI_Name;
}

[Serializable]
public enum DropType
{
    Colorful
}
