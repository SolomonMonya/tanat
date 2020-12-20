using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngineInternal;

namespace UnityEngine
{
	public sealed class GameObject : Object
	{
		public bool isStatic
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Transform transform
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Rigidbody rigidbody
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Camera camera
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Light light
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Animation animation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ConstantForce constantForce
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Renderer renderer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AudioSource audio
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public GUIText guiText
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public NetworkView networkView
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete("Please use guiTexture instead")]
		public GUIElement guiElement
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public GUITexture guiTexture
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Collider collider
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public HingeJoint hingeJoint
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ParticleEmitter particleEmitter
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int layer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool active
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public string tag
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public GameObject gameObject => this;

		public GameObject(string name)
		{
			Internal_CreateGameObject(this, name);
		}

		public GameObject()
		{
			Internal_CreateGameObject(this, null);
		}

		public GameObject(string name, params Type[] components)
		{
			Internal_CreateGameObject(this, name);
			foreach (Type componentType in components)
			{
				AddComponent(componentType);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern GameObject CreatePrimitive(PrimitiveType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		[WrapperlessIcall]
		public extern Component GetComponent(Type type);

		public T GetComponent<T>() where T : Component
		{
			return GetComponent(typeof(T)) as T;
		}

		public Component GetComponent(string type)
		{
			return GetComponentByName(type);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Component GetComponentByName(string type);

		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public Component GetComponentInChildren(Type type)
		{
			if (active)
			{
				Component component = GetComponent(type);
				if (component != null)
				{
					return component;
				}
			}
			Transform transform = this.transform;
			if (transform != null)
			{
				foreach (Transform item in transform)
				{
					Component componentInChildren = item.gameObject.GetComponentInChildren(type);
					if (componentInChildren != null)
					{
						return componentInChildren;
					}
				}
			}
			return null;
		}

		public T GetComponentInChildren<T>() where T : Component
		{
			return GetComponentInChildren(typeof(T)) as T;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Component[] GetComponents(Type type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Component[] GetComponentsWithCorrectReturnType(Type type);

		public T[] GetComponents<T>() where T : Component
		{
			return (T[])GetComponentsWithCorrectReturnType(typeof(T));
		}

		private void GetComponentsInChildrenRecurse(Type type, ArrayList array, bool includeInactive)
		{
			if (includeInactive || active)
			{
				array.AddRange(GetComponents(type));
			}
			Transform transform = this.transform;
			if (!(transform != null))
			{
				return;
			}
			foreach (Transform item in transform)
			{
				item.gameObject.GetComponentsInChildrenRecurse(type, array, includeInactive);
			}
		}

		public Component[] GetComponentsInChildren(Type type)
		{
			bool includeInactive = false;
			return GetComponentsInChildren(type, includeInactive);
		}

		public Component[] GetComponentsInChildren(Type type, bool includeInactive)
		{
			ArrayList arrayList = new ArrayList();
			GetComponentsInChildrenRecurse(type, arrayList, includeInactive);
			return (Component[])arrayList.ToArray(typeof(Component));
		}

		public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component
		{
			ArrayList arrayList = new ArrayList();
			GetComponentsInChildrenRecurse(typeof(T), arrayList, includeInactive);
			return (T[])arrayList.ToArray(typeof(T));
		}

		public T[] GetComponentsInChildren<T>() where T : Component
		{
			return GetComponentsInChildren<T>(includeInactive: false);
		}

		public void SetActiveRecursively(bool state)
		{
			foreach (Transform item in this.transform)
			{
				item.gameObject.SetActiveRecursively(state);
			}
			active = state;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool CompareTag(string tag);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern GameObject FindGameObjectWithTag(string tag);

		public static GameObject FindWithTag(string tag)
		{
			return FindGameObjectWithTag(tag);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern GameObject[] FindGameObjectsWithTag(string tag);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SendMessageUpwards(string methodName, object value, SendMessageOptions options);

		public void SendMessageUpwards(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			SendMessageUpwards(methodName, value, options);
		}

		public void SendMessageUpwards(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			SendMessageUpwards(methodName, value, options);
		}

		public void SendMessageUpwards(string methodName, SendMessageOptions options)
		{
			SendMessageUpwards(methodName, null, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SendMessage(string methodName, object value, SendMessageOptions options);

		public void SendMessage(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			SendMessage(methodName, value, options);
		}

		public void SendMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			SendMessage(methodName, value, options);
		}

		public void SendMessage(string methodName, SendMessageOptions options)
		{
			SendMessage(methodName, null, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void BroadcastMessage(string methodName, object parameter, SendMessageOptions options);

		public void BroadcastMessage(string methodName, object parameter)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			BroadcastMessage(methodName, parameter, options);
		}

		public void BroadcastMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object parameter = null;
			BroadcastMessage(methodName, parameter, options);
		}

		public void BroadcastMessage(string methodName, SendMessageOptions options)
		{
			BroadcastMessage(methodName, null, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Component AddComponent(string className);

		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public Component AddComponent(Type componentType)
		{
			return Internal_AddComponentWithType(componentType);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Component Internal_AddComponentWithType(Type componentType);

		public T AddComponent<T>() where T : Component
		{
			return AddComponent(typeof(T)) as T;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_CreateGameObject(GameObject mono, string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SampleAnimation(AnimationClip animation, float time);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[Obsolete("gameObject.PlayAnimation is not supported anymore. Use animation.Play")]
		[WrapperlessIcall]
		public extern void PlayAnimation(AnimationClip animation);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[Obsolete("gameObject.StopAnimation is not supported anymore. Use animation.Stop")]
		[WrapperlessIcall]
		public extern void StopAnimation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern GameObject Find(string name);
	}
}
