using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Computers
{
    public class ComputerEvent
    {
        public string eventType;
        public object data1;
        public object data2;
        public object data3;
        private float lifetime = 2f;

        public ComputerEvent(string eType, object d1, object d2, object d3, float duration)
        {
            eventType = eType;
            data1 = d1;
            data2 = d2;
            data3 = d3;
            lifetime = duration;
        }

        public void Decay(float time)
        {
            lifetime -= time;
        }
        public bool Expired()
        {
            return lifetime <= 0f;
        }
        public LuaTable ToLuaTable(Lua lua) 
        {
            var table = Utility.CreateTable(lua);
            table["data1"] = data1;
            table["data2"] = data2;
            table["data3"] = data3;
            table["type"] = eventType;


            return table;
        }
        //Converts Comptuer Event into a LuaTable 
        public static LuaTable ComptuerEventToTable(Lua lua, ComputerEvent ev)
        {
            if (ev == null) return null;


            var table = Utility.CreateTable(lua, "event");
            table["data1"] = ev.data1;
            table["data2"] = ev.data2;
            table["data3"] = ev.data3;
            table["type"] = ev.eventType;


            return table;
        }
    }

    public class EventSystem
    {
        private Computer host;
        private List<ComputerEvent> events;

        public EventSystem(Computer host)
        {
            events = new List<ComputerEvent>();
            this.host = host;
        }

        public void Purge()
        {
            events.Clear();
        }
        public void Update(float deltaTime)
        {
            GetInputEvents();
            DecayAndExpireEvents(deltaTime);
        }

        public void AddEvent(ComputerEvent _event)
        {
            events.Add(_event);
        }

        public void DecayAndExpireEvents(float time)
        {
            for (int i = events.Count - 1; i >= 0; i--)
            {
                if (events[i] == null) continue;

                events[i].Decay(time);
                if (events[i].Expired())
                {
                    events.RemoveAt(i);
                }
            }
        }

        public IEnumerator PullEventCoroutine(string type, float timeout, Action<ComputerEvent> callback)
        {
            DateTime endTime = DateTime.Now.AddSeconds(timeout);
            ComputerEvent _event = events.Find(c => c.eventType == type);

            while (_event == null && DateTime.Now < endTime)
            {
                yield return null; // Yield to allow other processes to run
                _event = events.Find(c => c.eventType == type);
            }

            if (_event != null) events.Remove(_event);
            callback?.Invoke(_event);
        }



        public ComputerEvent PullEvent(params string[] types)
        {
            List<string> typesList = types.ToList<string>();
            //Debug.Log((events == null).ToString() + (typesList == null).ToString());
            ComputerEvent _event = events.Find(c => typesList.Contains(c?.eventType));

            if (_event != null) events.Remove(_event);

            return _event;
        }
        public ComputerEvent PullEvent(string type)
        {
            ComputerEvent _event = events.Find(c => c.eventType == type);

            if (_event != null) events.Remove(_event);

            return _event;
        }
        public ComputerEvent PullEvent(string type, Predicate<ComputerEvent> match)
        {
            //Should allow us to define predicate for the event so we can deteine what kind of event we want to pull before its removed from event Queue
            //This is uselfull for pulling "luanet_send"/receive events as they have a port included in the event
            //If i pull the evnt and then check port then the event will already have been removed. This noooo good, so instead this way we can
            if (events == null) return null;

            ComputerEvent _event = events.Find(c => c?.eventType == type && match(c));


            if (_event != null) events.Remove(_event);

            return _event;
        }

        public void GetInputEvents()
        {
            if (!host.IsFocus()) return; //Return if the computer is not the current focus of the players inputs
            if (events == null) return;

            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            bool controlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (keyCode == KeyCode.LeftShift) continue;
                if (Input.GetKey(keyCode))
                {
                    AddEvent(new ComputerEvent("key_hold", Utility.KeyCodeToChar(keyCode, shiftPressed), shiftPressed, controlPressed, 0.05f));
                }
                if (Input.GetKeyDown(keyCode))
                {
                    AddEvent(new ComputerEvent("key_down", Utility.KeyCodeToChar(keyCode, shiftPressed), shiftPressed, controlPressed, 0.05f));
                }
                if (Input.GetKeyUp(keyCode))
                {
                    AddEvent(new ComputerEvent("key_up", Utility.KeyCodeToChar(keyCode, shiftPressed), shiftPressed, controlPressed, 0.05f));
                }
            }
        }
    }
}