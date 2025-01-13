using System;
using System.Collections;
using System.Collections.Generic;
using Hmxs.Scripts;
using Hmxs.Toolkit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FPS
{
	public class RaceTargetManager : MonoBehaviour
	{
		[SerializeField, MinMaxSlider(1, 5)] private Vector2 spawnInterval = new(1, 5);
		[SerializeField] private List<GameObject> targets = new();

		[SerializeField, ReadOnly] private int targetNumber;
		[SerializeField, ReadOnly] private List<GameObject> tempTargets;
		private bool _raceWasEnd;

		private void OnEnable()
		{
			Events.AddListener(EventNames.RaceStart, OnRaceStart);
			Events.AddListener(EventNames.HitTarget, OnHitTarget);
			Events.AddListener<bool>(EventNames.RaceEnd, OnRaceEnd);
		}

		private void OnDisable()
		{
			Events.RemoveListener(EventNames.RaceStart, OnRaceStart);
			Events.RemoveListener(EventNames.HitTarget, OnHitTarget);
			Events.RemoveListener<bool>(EventNames.RaceEnd, OnRaceEnd);
		}

		private IEnumerator SpawnTarget()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(spawnInterval.x, spawnInterval.y));
			var target = GetRandomTarget();
			if (!target)
			{
				Debug.Log("No more targets to spawn");
				yield break;
			}
			target.Up();
			StartCoroutine(SpawnTarget());
		}

		private RaceTarget GetRandomTarget()
		{
			if (tempTargets.Count <= 0) return null;
			var targetObj = tempTargets[UnityEngine.Random.Range(0, tempTargets.Count)];
			var target = targetObj.GetComponentInChildren<RaceTarget>();
			tempTargets.Remove(targetObj);
			return target;
		}

		private void OnRaceStart()
		{
			_raceWasEnd = false;
			tempTargets = new List<GameObject>(targets);
			targetNumber = tempTargets.Count;
			StartCoroutine(SpawnTarget());
		}

		private void OnHitTarget()
		{
			if (_raceWasEnd) return;
			targetNumber -= 1;
			if (targetNumber <= 0)
				Events.Trigger(EventNames.RaceEnd, true);
		}

		private void OnRaceEnd(bool win)
		{
			if (_raceWasEnd) return;
			_raceWasEnd = true;
			StopAllCoroutines();
			foreach (var target in tempTargets)
				target.GetComponentInChildren<RaceTarget>().Up();
		}
	}
}
