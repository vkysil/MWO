using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// temporary scene changer for testing purposes
// can be removed when a proper UI is implemented
public class SceneChanger : MonoBehaviour
{
	// change scene
	public void ChangeScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	// exit application
	public void Exit()
	{
		Application.Quit();
	}
}