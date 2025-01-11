using System;
using UnityEngine;

namespace Hmxs.Scripts.Utility
{
	public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
	{
		private static T _instance;

		public static T instance
		{
			get
			{
				if (_instance) return _instance;
				var instances = FindObjectsOfType(typeof(T));
				if (instances.Length > 0)
				{
					if (instances.Length > 1)
						Debug.LogWarning("[Singleton] Multiple instances of singleton found in the scene.");
					_instance = (T)instances[0];
					if (_instance) return _instance;
				}
				GameObject singletonObj = new GameObject()
				{
					name = $"{typeof(T)}_SingletonMono",
					hideFlags = HideFlags.DontSave
				};
				_instance = singletonObj.AddComponent<T>();
				Debug.Log($"[Singleton] An instance of {typeof(T)} is created.");
				return _instance;
			}
		}

		protected virtual void Awake()
		{
			if (!_instance) _instance = (T)this;
			else if (_instance != this)
				Destroy(gameObject);
		}

		protected virtual void OnDestroy()
		{
			if (_instance == this) _instance = null;
		}
	}
}
