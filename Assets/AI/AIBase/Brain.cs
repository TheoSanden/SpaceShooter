using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;

namespace AI 
{
    [System.Serializable]
    public struct Sequencer 
    {
        [SerializeField]
        public AIBehaviour Behaviour;
        [SerializeField]
        public AIBehaviour[] Children;
    }
    public class BlackBoard 
    {
        private Dictionary<string, object> ObjectDictionary = new Dictionary<string, object>();
        private Dictionary<string, float> FloatDictionary = new Dictionary<string, float>();
        private Dictionary<string, Vector2> VectorDictionary = new Dictionary<string, Vector2>();
        private Dictionary<string, bool> BoolDictionary = new Dictionary<string, bool>();
        public void SetValueAsObject<T>(string key, T value) where T: class 
        {
            if (ObjectDictionary.ContainsKey(key)) 
            {
                ObjectDictionary[key] = value;
                return;
            }
            ObjectDictionary.Add(key, value);
        }
        public T GetValueAsObject<T>(string key) where T : class 
        {
            return ObjectDictionary[key] as T;
        }
        public void RemoveObject(string key) 
        {
            ObjectDictionary.Remove(key);
        }

        public void SetValueAsFloat(string key,float value)
        {
            if (FloatDictionary.ContainsKey(key))
            {
                FloatDictionary[key] = value;
                return;
            }
            FloatDictionary.Add(key, value);
        }
        public float GetValueAsFloat(string key)
        {
            return FloatDictionary[key];
        }
        public void RemoveFloat(string key)
        {
            FloatDictionary.Remove(key);
        }

        public void SetValueAsVector(string key, Vector2 value)
        {
            if (VectorDictionary.ContainsKey(key))
            {
                VectorDictionary[key] = value;
                return;
            }
            VectorDictionary.Add(key, value);
        }
        public Vector2 GetValueAsVector(string key)
        {
            return VectorDictionary[key];
        }
        public void RemoveVector(string key)
        {
            VectorDictionary.Remove(key);
        }

        public void SetValueAsBool(string key, bool value)
        {
            if (BoolDictionary.ContainsKey(key))
            {
                BoolDictionary[key] = value;
                return;
            }
            BoolDictionary.Add(key, value);
        }
        public bool GetValueAsBool(string key)
        {
            return BoolDictionary[key];
        }
        public void RemoveBool(string key)
        {
            BoolDictionary.Remove(key);
        }
    }

    public class Brain : MonoBehaviour
    {
        [SerializeField]
        bool EnableDebugLog;
        [SerializeField]
        Sequencer[] Sequences;
        int CurrentSequenceIndex = 0;

        public BlackBoard Blackboard = new BlackBoard();
        bool FirstProcessUpdate = false;
        private void Start()
        {
            for (int i = 0; i < Sequences.Length; i++)
            {
                Sequences[i].Behaviour = Instantiate(Sequences[i].Behaviour);
                Sequences[i].Behaviour.Initalize(this);
                for (int o = 0; o < Sequences[i].Children.Length; o++)
                {
                    Sequences[i].Children[o] = Instantiate(Sequences[i].Children[o]);
                    Sequences[i].Children[o].Initalize(this);
                }
            }
        }
        private void Update()
        {
            if (Sequences.Length != 0) 
            {
                ProcessSequence(Sequences[CurrentSequenceIndex]);
            }
        }

        private void ProcessSequence(Sequencer Sequence) 
        {
            if (!FirstProcessUpdate)
            {
                FirstProcessUpdate = true;
                OnSequenceStart(Sequences[CurrentSequenceIndex]);
            }
            AIBehaviour.BehaviourState State = Sequence.Behaviour.Process(this);

            if (State == AIBehaviour.BehaviourState.Finished) 
            {
                OnSequenceEnd(Sequence);
                //Change this later to be based on where to go in the tree i guess but for now just loop
                CurrentSequenceIndex = (CurrentSequenceIndex + 2 > Sequences.Length)? 0: CurrentSequenceIndex + 1;

                FirstProcessUpdate = false;
                if (EnableDebugLog)
                {
                   // Debug.Log("moving to " + Sequences[CurrentSequenceIndex]);
                }
                return;
            }

            foreach (AIBehaviour child in Sequence.Children) 
            {
                child.Process(this);
            }
        }

        private void OnSequenceStart(Sequencer Sequence) 
        {
            Sequence.Behaviour.OnBehaviourStart(this);

            foreach(var child in Sequence.Children) 
            {
                child.OnBehaviourStart(this);
            }
        }
        private void OnSequenceEnd(Sequencer Sequence) 
        {
            Sequence.Behaviour.OnBehaviourEnd(this);

            foreach (var child in Sequence.Children)
            {
                child.OnBehaviourEnd(this);
            }
        }
    }
}