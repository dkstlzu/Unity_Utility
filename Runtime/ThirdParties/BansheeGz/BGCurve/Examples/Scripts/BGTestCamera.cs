using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BansheeGz.BGSpline.Example
{
    //camera movements
    public class BGTestCamera : MonoBehaviour
    {
        private const int Speed = 100;

        private GUIStyle style;

        // Update is called once per frame
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.aKey.isPressed) transform.RotateAround(Vector3.zero, Vector3.up, Speed*Time.deltaTime);
            else if (Keyboard.current.dKey.isPressed) transform.RotateAround(Vector3.zero, Vector3.up, -Speed*Time.deltaTime);
#else
            if (Input.GetKey(KeyCode.A)) transform.RotateAround(Vector3.zero, Vector3.up, Speed*Time.deltaTime);
            else if (Input.GetKey(KeyCode.D)) transform.RotateAround(Vector3.zero, Vector3.up, -Speed*Time.deltaTime);
#endif
        }

        private void OnGUI()
        {
            if (style == null) style = new GUIStyle(GUI.skin.label) {fontSize = 24};

            GUILayout.BeginHorizontal();

            GUILayout.Label("Use A and D to rotate camera", style);

            if (BGTestMainMenu.Inited && GUILayout.Button("To Main Menu")) SceneManager.LoadScene("BGCurveMainMenu");

            GUILayout.EndHorizontal();
        }
    }
}