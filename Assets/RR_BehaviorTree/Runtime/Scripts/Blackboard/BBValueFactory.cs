using UnityEditor;
using UnityEngine;

namespace RR.AI
{
	public static class BBValueFactory
	{
		private static System.Collections.Generic.Dictionary<System.Type, System.Type> _typeToValSODict = 
		new System.Collections.Generic.Dictionary<System.Type, System.Type>()
		{
			{ typeof(int), typeof(BBInt) },
			{ typeof(float), typeof(BBFloat) },
			{ typeof(bool), typeof(BBBool) },
			{ typeof(string), typeof(BBString) },
			{ typeof(Vector2), typeof(BBVector2) },
			{ typeof(Vector3), typeof(BBVector3) }
		};

		/// <summary>
		///  Create a ScriptableObject that holds a value on disk
		/// </summary>
		public static ScriptableObject New<T>(ScriptableObject assetObj, object value)
		{
			if (!_typeToValSODict.TryGetValue(typeof(T), out var BBValType))
			{
				Debug.LogError($"Unidentified BBValue type {typeof(T)}");
				return null;
			}

			var obj = CreateValueObject(assetObj, BBValType);

			if (obj == null)
			{
				return null;
			}
			
			(obj as IBBValue).SetValue(value);
			return obj;
		}

		private static ScriptableObject CreateValueObject(ScriptableObject assetObj, System.Type BBValType)
		{
			if (!typeof(IBBValue).IsAssignableFrom(BBValType) || !typeof(IBBValue).IsAssignableFrom(BBValType))
			{
				Debug.LogError($"{BBValType} must derived from ScriptableObject and IBBValue");
				return null;
			}

			var SO = ScriptableObject.CreateInstance(BBValType);
			AssetDatabase.AddObjectToAsset(SO, assetObj);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(SO));
			return SO;
		}
	}
}