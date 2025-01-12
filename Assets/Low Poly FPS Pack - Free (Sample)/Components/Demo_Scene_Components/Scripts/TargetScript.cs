using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class TargetScript : MonoBehaviour
{
	private float _randomTime;
	private bool _routineStarted;

	[SerializeField, ReadOnly] public bool isHit;

	[Header("Customizable Options")]
	//Minimum time before the target goes back up
	[SerializeField] private float minTime;
	//Maximum time before the target goes back up
	[SerializeField] private float maxTime;

	[Header("Audio")]
	[SerializeField] private AudioClip upSound;
	[SerializeField] private AudioClip downSound;
	[SerializeField] private AudioSource audioSource;

	private void Update()
	{
		//Generate random time based on min and max time values
		_randomTime = Random.Range(minTime, maxTime);

		//If the target is hit
		if (isHit)
		{
			if (_routineStarted == false)
			{
				//Animate the target "down"
				gameObject.GetComponent<Animation>().Play("target_down");

				//Set the downSound as current sound, and play it
				audioSource.GetComponent<AudioSource>().clip = downSound;
				audioSource.Play();

				//Start the timer
				StartCoroutine(DelayTimer());
				_routineStarted = true;
			}
		}
	}

	//Time before the target pops back up
	private IEnumerator DelayTimer()
	{
		//Wait for random amount of time
		yield return new WaitForSeconds(_randomTime);
		//Animate the target "up" 
		gameObject.GetComponent<Animation>().Play("target_up");

		//Set the upSound as current sound, and play it
		audioSource.GetComponent<AudioSource>().clip = upSound;
		audioSource.Play();

		//Target is no longer hit
		isHit = false;
		_routineStarted = false;
	}
}
