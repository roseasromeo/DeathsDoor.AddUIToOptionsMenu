using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDoor.AddUITOOptionsMenu;
public static class Util
{
    public static GameObject GetByPath(string parentScene, string path)
    {
		string[] elements = path.Trim('/').Split('/');
		Scene activeScene = SceneManager.GetSceneByName(parentScene);
		GameObject[] rootObjects = activeScene.GetRootGameObjects();

		GameObject root = rootObjects.First((go) => go.name == elements[0]);
        GameObject current = root;
        foreach (string element in elements.Skip(1))
        {
            current = current.transform.Cast<Transform>()
            .First((t) => t.name == element)
            .gameObject;
        }
        return current;
    }
}