using UnityEngine;
using System.Linq; // Add LINQ namespace

public static class FindMultiTag
{
    /// <summary>
    /// Finds the first GameObject in the scene that has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>The first GameObject with the specified tag, or null if none are found.</returns>
    public static GameObject FindMultiTagByTag(string tag)//
    {
        // Use LINQ to find the first GameObject with the tag
        return GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .FirstOrDefault(obj => obj != null && obj.HasTag(tag)); // Return first matching GameObject
    }
}



//public static class FindMultiTag
//{
//    /// <summary>
//    /// Finds the first GameObject in the scene that has the specified tag.
//    /// </summary>
//    /// <param name="tag">The tag to search for.</param>
//    /// <returns>The first GameObject with the specified tag, or null if none are found.</returns>
//    public static GameObject FindMultiTagByTag(string tag)
//    {
//        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
//        {
//            if (obj != null && obj.HasTag(tag)) // Use Unity's CompareTag for efficiency
//            {
//                return obj; // Return the first GameObject found with the tag
//            }
//        }
//        return null; // Return null if no matching GameObject is found
//    }
//}
