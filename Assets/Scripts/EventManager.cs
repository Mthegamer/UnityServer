using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour 
{
	private Dictionary<string, UnityEvent> eventDictionary;

	private static EventManager EVENT_MANAGER;

	public static EventManager instance
	{
		get
		{
			if(!EVENT_MANAGER)
			{
				EVENT_MANAGER = FindObjectOfType<EventManager>() as EventManager;
				if(!EVENT_MANAGER)
				{
					Debug.LogError("No Event Manager found");
				}else
				{
					EVENT_MANAGER.Init();
				}
			}
			return EVENT_MANAGER;
		}
	}

	void Init()
	{
		if(eventDictionary == null)
		{
			eventDictionary = new Dictionary<string, UnityEvent>();
		}
	}

	public static void StartListening(string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}else
		{
			thisEvent = new UnityEvent();
			thisEvent.AddListener(listener);
			instance.eventDictionary.Add(eventName, thisEvent);
		}
	}

	public static void StopListening(string eventName, UnityAction listener)
	{
		if (EVENT_MANAGER == null)
			return;
		UnityEvent thisEvent = null;
		if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}

	public static void TriggerEvent(string eventName)
	{
		UnityEvent thisEvent = null;
		if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke();
		}
	}
}
