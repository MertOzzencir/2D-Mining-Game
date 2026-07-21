using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop SO", menuName = "Create New Drop Data/New Drop Data")]
public class DropSO : ScriptableObject
{
    public Mesh Mesh;
    public Material Material;
    public DropType DropType;
}

[Serializable]
public enum DropType
{
    Iron
}
