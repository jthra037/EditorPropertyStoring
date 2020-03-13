using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

[CreateAssetMenu(fileName = "AnimatorDB", menuName = "ScriptableObjects/AnimatorDatabase", order = 1)]
public class AnimatorDB : ScriptableObject
{
	public string AnimatorAssetPath;

	[HideInInspector]
	public string SerializedStateMap;

	[System.Serializable]
	public struct AnimatorStateInfo
	{
		public string Name;
		public bool HasTransitions;
		public bool IsCombat;
	}

	private Dictionary<int, AnimatorStateInfo> _animationStateMap;
	public Dictionary<int, AnimatorStateInfo> AnimationStateMap
	{
		get
		{
			if(_animationStateMap == null)
			{
				_animationStateMap = deserializeBase64StoreIntoDict();
			}
			return _animationStateMap;
		}
	}

	public bool StateHasTransitions(int fullPathHash)
	{
		AnimatorStateInfo info;
		if(AnimationStateMap != null && AnimationStateMap.TryGetValue(fullPathHash, out info))
		{
			return info.HasTransitions;
		}

		return false;
	}

	public string GetStateName(int fullPathHash)
	{
		AnimatorStateInfo info;
		if (AnimationStateMap != null && AnimationStateMap.TryGetValue(fullPathHash, out info))
		{
			return info.Name;
		}

		return null;
	}

	public bool StateIsFitness(int fullPathHash)
	{
		AnimatorStateInfo info;
		if (AnimationStateMap != null && AnimationStateMap.TryGetValue(fullPathHash, out info))
		{
			return info.Name.ToLower().Contains("fitness_");
		}

		return false;
	}

	public bool StateIsFloor(int fullPathHash)
	{
		AnimatorStateInfo info;
		if (AnimationStateMap != null && AnimationStateMap.TryGetValue(fullPathHash, out info))
		{
			return info.IsCombat;
		}

		return false;
	}

	private Dictionary<int, AnimatorStateInfo> deserializeBase64StoreIntoDict()
	{
		Dictionary<int, AnimatorStateInfo> result = new Dictionary<int, AnimatorStateInfo>();

		if (SerializedStateMap != null)
		{
			byte[] array = System.Convert.FromBase64String(SerializedStateMap);
			MemoryStream stream = new MemoryStream(array);
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				result = formatter.Deserialize(stream) as Dictionary<int, AnimatorStateInfo>;
			}
			catch (SerializationException e)
			{
				Debug.Log("Failed to deserialize. Reason: " + e.Message);
				throw;
			}
			finally
			{
				stream.Close();
			}
		}

		return result;
	}
}
