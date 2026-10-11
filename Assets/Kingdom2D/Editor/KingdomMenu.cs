using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kingdom2D.EditorTools
{
    /// <summary>
    /// Editör kısayolları. Oyun kendini kod ile kurduğu için zorunlu değildir;
    /// ayarları Inspector'dan değiştirmek isteyenler için hazır sahne üretir.
    /// </summary>
    public static class KingdomMenu
    {
        const string ScenePath = "Assets/Kingdom2D/Scenes/Kingdom.unity";

        [MenuItem("Kingdom 2D/Oyun Sahnesi Olustur", false, 1)]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var go = new GameObject("Kingdom2D");
            go.AddComponent<GameBootstrap>();
            Selection.activeGameObject = go;

            System.IO.Directory.CreateDirectory("Assets/Kingdom2D/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("Kingdom 2D sahnesi olusturuldu: " + ScenePath + "  (Play'e basip oynayabilirsiniz)");
        }

        [MenuItem("Kingdom 2D/Sahneyi Ac", false, 2)]
        public static void OpenScene()
        {
            if (System.IO.File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else CreateScene();
        }

        [MenuItem("Kingdom 2D/Kayitli En Iyi Gunu Sifirla", false, 20)]
        public static void ResetBest()
        {
            PlayerPrefs.DeleteKey("kingdom2d_best_day");
            PlayerPrefs.Save();
            Debug.Log("Kingdom 2D: en iyi gun kaydi silindi.");
        }
    }
}
