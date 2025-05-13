using System.Collections.Generic;
using FrameLabs.AI.Blackboard;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.AI.System
{

	public class NodeSettings : BlackboardData
	{
		[SerializeField] private List<Component> settingsList = null;
		public List<Component> SettingsList => settingsList;
		void OnEnable()
		{
#if UNITY_EDITOR
			settingsList = new List<Component>();
			settingsList.Add(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/System/NodeSettings.prefab").GetComponent<FrameLabs.AI.System.NodeExecutionTracker>());
#endif
		}
		public override BlackboardData Clone()
		{
			var clone = CreateInstance<NodeSettings>();
			clone.settingsList = settingsList;
			return clone;
		}
	}
}
