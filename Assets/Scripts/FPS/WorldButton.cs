using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
	public class WorldButton : MonoBehaviour
	{
		[SerializeField] private float delay = 0.3f;
		[SerializeField] private UnityEvent onShoot;

		public void OnTrigger()
		{
			StartCoroutine(Trigger());
		}

		private IEnumerator Trigger()
		{
			yield return new WaitForSeconds(delay);
			onShoot?.Invoke();
		}
	}
}
