using System.Collections;
using UnityEngine;

public class CombatAnimation : StateMachineBehaviour
{
	[SerializeField]
	private bool isCombat = true;

	public bool IsFCombat => isCombat;
}