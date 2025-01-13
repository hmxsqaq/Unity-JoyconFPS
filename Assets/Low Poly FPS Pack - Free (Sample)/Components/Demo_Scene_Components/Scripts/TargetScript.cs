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
	[SerializeField] protected AudioClip upSound;
	[SerializeField] protected AudioClip downSound;
	[SerializeField] protected AudioSource audioSource;

	protected virtual void Update()
	{
		//Generate random time based on min and max time values
		_randomTime = Random.Range(minTime, maxTime);

		//If the target is hit
		if (!isHit) return;
		if (_routineStarted) return;
		gameObject.GetComponent<Animation>().Play("target_down");
		audioSource.GetComponent<AudioSource>().clip = downSound;
		audioSource.Play();

		//Start the timer
		StartCoroutine(DelayTimer());
		_routineStarted = true;
	}

	//Time before the target pops back up
	private IEnumerator DelayTimer()
	{
		yield return new WaitForSeconds(_randomTime);
		gameObject.GetComponent<Animation>().Play("target_up");
		audioSource.GetComponent<AudioSource>().clip = upSound;
		audioSource.Play();

		isHit = false;
		_routineStarted = false;
	}
}
