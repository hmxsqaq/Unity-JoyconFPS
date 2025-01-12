using System;
using System.Collections.Generic;

namespace FPS
{
	public static class RankingRecorder
	{
		private static readonly List<TimeSpan> Ranking = new();

		public static void RecordRanking(TimeSpan time) => Ranking.Add(time);

		public static void GetTopRanking(int count, out List<TimeSpan> topRanking)
		{
			Ranking.Sort();
			topRanking = new List<TimeSpan>();
			for (var i = 0; i < Math.Min(count, Ranking.Count); i++)
				topRanking.Add(Ranking[i]);
		}
	}
}
