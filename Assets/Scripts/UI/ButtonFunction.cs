using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hmxs.Scripts.UI
{
	public class ButtonFunction : MonoBehaviour
	{
		[SerializeField] private float delay = 0.3f;

		public void LoadScene(int sceneIndex) => StartCoroutine(Delay(sceneIndex));

		public void LoadScene(string sceneName) => StartCoroutine(Delay(sceneName));

		public void Quit() => Application.Quit();

		private IEnumerator Delay(int sceneIndex)
		{
			yield return new WaitForSeconds(delay);
			SceneManager.LoadScene(sceneIndex);
		}

		private IEnumerator Delay(string sceneName)
		{
			yield return new WaitForSeconds(delay);
			SceneManager.LoadScene(sceneName);
		}

	}
}
