using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DataStructures
{
    public class Action
    {
        //Next move
        public enum NextAction
        {
            Up,
            Right,
            Down,
            Left, None
        }
        public NextAction nextAction;

        public float reinforce { get; private set; }
        public float reward { get; private set; }
        public State NextState { get; set; }

        public Action()
        {
            reward = 0.0f;
            reinforce = 0.0f;
        }

        public void SetReinforce()
        {
            if (!NextState.cellInfo.Walkable)
                reinforce = ReinforcementFunc.punish;
            if (NextState.IsExit)
                reinforce = ReinforcementFunc.prize;
            
        }

        public void SetReward(float newReward)
        {
            reward = newReward;
        }

        public void UpdateReward()
        {
            float learnedAmount = reinforce + LearningRatio.gamma * NextState.GetBestAction().reward;
            learnedAmount *= LearningRatio.alpha;
            reward = (1 - LearningRatio.alpha) * reward + learnedAmount;
        }
    }
}
