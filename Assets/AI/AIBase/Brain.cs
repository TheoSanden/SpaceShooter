using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI 
{
    [System.Serializable]
    public class Sequencer 
    {
        [SerializeField]
        public AIBehaviour Behaviour = null;
        [SerializeField]
        public AIBehaviour[] Children;
    }
    public class Brain : MonoBehaviour
    {
        [SerializeField]
        bool EnableDebugLog;
        [SerializeField]
        Sequencer[] Sequences;
        int CurrentSequenceIndex = 0;
        private void Start()
        {
            foreach (Sequencer sequence in Sequences) 
            {
                sequence.Behaviour.Initalize(this);
                foreach (var child in sequence.Children) 
                {
                    child.Initalize(this);
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
            AIBehaviour.BehaviourState State = Sequence.Behaviour.Process(this);

            if (State == AIBehaviour.BehaviourState.Finished) 
            {
                //Change this later to be based on where to go in the tree i guess but for now just loop
                CurrentSequenceIndex = (CurrentSequenceIndex + 2 > Sequences.Length)? 0: CurrentSequenceIndex++;
                if (EnableDebugLog)
                {
                    Debug.Log("moving to " + Sequences[CurrentSequenceIndex]);
                }
                return;
            }

            foreach (AIBehaviour child in Sequence.Children) 
            {
                child.Process(this);
            }
        }
    }
}