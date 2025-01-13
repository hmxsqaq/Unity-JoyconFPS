using Hmxs.Scripts;
using Hmxs.Toolkit;
using UnityEngine;

namespace FPS
{
	public class RaceTarget : TargetScript
	{
		private Animation _animation;
		private bool _hasTriggered;

		private void Start()
		{
			_animation = gameObject.GetComponent<Animation>();
			_animation.Play("target_down");
		}

		public void Up()
		{
			_animation.Play("target_up");
			audioSource.GetComponent<AudioSource>().clip = upSound;
			audioSource.Play();
			isHit = false;
			_hasTriggered = false;
		}

		protected override void Update()
		{
			if (!isHit || _hasTriggered) return;
			Debug.Log("Target is hit");
			_hasTriggered = true;
			_animation.Play("target_down");
			audioSource.GetComponent<AudioSource>().clip = downSound;
			audioSource.Play();
			Events.Trigger(EventNames.HitTarget);
		}
	}
}
