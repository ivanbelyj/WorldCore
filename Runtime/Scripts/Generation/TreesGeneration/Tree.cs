using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Вид дерева, не зависящий от биома
/// </summary>
[CreateAssetMenu(fileName = "New Tree", menuName = "Procedural World/Tree", order = 51)]
public class Tree : ScriptableObject  //, IEquatable<Tree>
{
    [SerializeField]
    private GameObject[] treePrefabs;
    /// <summary>
    /// Дерево представлено различными вариантами моделей
    /// </summary>
    public GameObject[] TreePrefabs { get => treePrefabs; set => treePrefabs = value; }

    [SerializeField]
    private float scaleMultiplier = 1f;
    public float ScaleMultiplier => scaleMultiplier;

    [SerializeField]
    private float minSize = 1f;
    public float MinSize => minSize;

    [SerializeField]
    private float maxSize = 1f;
    public float MaxSize => maxSize;

    // private Guid treeId = default;
    // private Guid TreeId {
    //     get {
    //         if (treeId == default) {
    //             treeId = Guid.NewGuid();
    //         }
    //         return treeId;
    //     }
    // }

    // public override bool Equals(object obj)
    // {
    //     if (obj == null || GetType() != obj.GetType())
    //     {
    //         return false;
    //     }

    //     return Equals(obj as Tree);
    // }

    // public bool Equals(Tree other)
    // {
    //     if (other == null)
    //     {
    //         return false;
    //     }

    //     return TreeId == other.TreeId;
    // }

    // public override int GetHashCode()
    // {
    //     return TreeId.GetHashCode();
    // }
}
