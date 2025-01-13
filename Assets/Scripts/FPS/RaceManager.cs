using System;
using System.Collections;
using Hmxs.Scripts;
using Hmxs.Scripts.UI;
using Hmxs.Toolkit;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace FPS
{
	public class RaceManager : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI timeCountText;
		[SerializeField] private RankingManager rankingPanel;
		[SerializeField] private float maxRaceTimeInSeconds = 180;
		[SerializeField] private GameObject worldButton;

		private bool _raceWasEnd;
		private DateTime _startTime;
		private TimeSpan _elapsedTime;

		private void Start()
		{
			worldButton.SetActive(true);
			timeCountText.gameObject.SetActive(false);
			rankingPanel.transform.parent.gameObject.SetActive(true);
			rankingPanel.ShowRanking();
		}

		private void OnEnable() => Events.AddListener<bool>(EventNames.RaceEnd, EndRace);
		private void OnDisable() => Events.RemoveListener<bool>(EventNames.RaceEnd, EndRace);

		[Button]
		public void StartRace()
		{
			_raceWasEnd = false;
			rankingPanel.transform.parent.gameObject.SetActive(false);
			timeCountText.gameObject.SetActive(true);
			worldButton.SetActive(false);
			Events.Trigger(EventNames.RaceStart);
			StartCoroutine(CountDown());
		}

		private IEnumerator CountDown()
		{
			timeCountText.text = "3";
			yield return new WaitForSeconds(1);
			timeCountText.text = "2";
			yield return new WaitForSeconds(1);
			timeCountText.text = "1";
			yield return new WaitForSeconds(1);
			timeCountText.text = "GO!";
			yield return new WaitForSeconds(1);
			_startTime = DateTime.Now;
			var maxRaceTime = TimeSpan.FromSeconds(maxRaceTimeInSeconds);
			timeCountText.text = maxRaceTime.ToString("mm':'ss'.'ff");
			_elapsedTime = DateTime.Now - _startTime;
			while (_elapsedTime < maxRaceTime)
			{
				yield return null;
				_elapsedTime = DateTime.Now - _startTime;
				timeCountText.text = (maxRaceTime - _elapsedTime).ToString("mm':'ss'.'ff");
			}
			Events.Trigger(EventNames.RaceEnd, false);
		}

		private void EndRace(bool win)
		{
			if (_raceWasEnd) return;
			_raceWasEnd = true;
			StopAllCoroutines();
			StartCoroutine(End(win));
		}

		private IEnumerator End(bool win)
		{
			if (win)
			{
				timeCountText.text = "You Win!";
				RankingRecorder.RecordRanking(_elapsedTime);
			}
			else
				timeCountText.text = "Time Over!";

			yield return new WaitForSeconds(2f);

			worldButton.SetActive(true);
			timeCountText.gameObject.SetActive(false);
			rankingPanel.transform.parent.gameObject.SetActive(true);
			rankingPanel.ShowRanking();
		}
	}
}
