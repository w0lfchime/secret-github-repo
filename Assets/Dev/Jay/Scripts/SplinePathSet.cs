using System;
using UnityEngine;
using UnityEngine.Splines;

public class SplinePathSet : MonoBehaviour
{
	[Serializable]
	public struct PathRef
	{
		public SplineContainer container;
		public int splineIndex;
	}

	public PathRef[] paths;

	public int Count => paths == null ? 0 : paths.Length;
}
