using UnityEngine;

public class ColliderMerger : MonoBehaviour
{
    void MergeColliders()
    {
        GameObject parent = gameObject;

        Collider[] childColliders = parent.GetComponentsInChildren<Collider>();

        foreach (var childCol in childColliders)
        {
            if (childCol.gameObject == parent) continue;

            if (childCol is BoxCollider box)
            {
                BoxCollider newCol = parent.AddComponent<BoxCollider>();
                //newCol.center = parent.transform.InverseTransformPoint(childCol.transform.TransformPoint(box.center));

                Vector3 worldScale = childCol.transform.lossyScale;
                newCol.size = Vector3.Scale(box.size, new Vector3(
                    worldScale.x / parent.transform.lossyScale.x,
                    worldScale.y / parent.transform.lossyScale.y,
                    worldScale.z / parent.transform.lossyScale.z));
                newCol.isTrigger = box.isTrigger;
            }
            else if (childCol is SphereCollider sphere)
            {
                SphereCollider newCol = parent.AddComponent<SphereCollider>();
                newCol.center = parent.transform.InverseTransformPoint(childCol.transform.TransformPoint(sphere.center));
                newCol.radius = sphere.radius * (childCol.transform.lossyScale.x / parent.transform.lossyScale.x);
                newCol.isTrigger = sphere.isTrigger;
            }
            else if (childCol is MeshCollider mesh)
            {
                MeshCollider newCol = parent.AddComponent<MeshCollider>();
                newCol.sharedMesh = mesh.sharedMesh;
                newCol.convex = mesh.convex;
                newCol.isTrigger = mesh.isTrigger;
            }

            childCol.enabled = false;
        }
        Debug.Log("Коллайдеры успешно перенесены на " + parent.name);
    }

    private void Awake()
    {
        MergeColliders();
    }
}
