using System;
using FPS;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hmxs.Scripts.UI
{
	public class RankingManager : MonoBehaviour
	{
		[SerializeField, AssetsOnly] private RankingElement rankingPrefab;
		[SerializeField] private int maxRanking = 7;

		private void ClearChildren()
		{
			foreach (Transform child in transform)
				Destroy(child.gameObject);
		}

		[Button]
		public void ShowRanking()
		{
			ClearChildren();
			RankingRecorder.GetTopRanking(maxRanking, out var ranking);
			for (var i = 0; i < ranking.Count; i++)
			{
				var rankingElement = Instantiate(rankingPrefab.gameObject, transform);
				rankingElement.GetComponent<RankingElement>().Set(i + 1, ranking[i]);
			}
		}

		[Button]
		private void TestRecord(int times = 10)
		{
			for (int i = 0; i < times; i++)
			{
				RankingRecorder.RecordRanking(new TimeSpan(0, Random.Range(0, 5), Random.Range(0, 59)));
			}
		}
	}
}
