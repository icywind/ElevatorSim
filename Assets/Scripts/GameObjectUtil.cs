using UnityEngine;
namespace SimElevator
{

    /// <summary>
    /// Game object util.
    /// </summary>
    public class GameObjectUtil
    {
        /// <summary>
        /// Instantiates the and anchor.
        /// </summary>
        /// <returns>The and anchor.</returns>
        /// <param name="prefab">Prefab.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="position">Position.</param>
        public static GameObject InstantiateAndAnchor(GameObject prefab, Transform parent, Vector3 position)
        {
            GameObject go = Object.Instantiate(prefab) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one;
            return go;
        }
    }
}
