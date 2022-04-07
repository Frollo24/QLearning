using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assets.Scripts.DataStructures
{
    public class State
    {
        public CellInfo cellInfo;
        public List<Action> possibleActions;
        public bool IsExit { get; private set; }

        public State(CellInfo cell, CellInfo exit)
        {
            cellInfo = cell;
            possibleActions = new List<Action>();
            IsExit = (exit == cellInfo);
        }

        public Action GetNextAction(Locomotion.MoveDirection nextMove)
        {
            foreach (Action act in possibleActions)
            {
                switch (nextMove)
                {
                    case Locomotion.MoveDirection.Up:
                        if(act.nextAction == Action.NextAction.Up)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case Locomotion.MoveDirection.Right:
                        if (act.nextAction == Action.NextAction.Right)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case Locomotion.MoveDirection.Down:
                        if (act.nextAction == Action.NextAction.Down)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case Locomotion.MoveDirection.Left:
                        if (act.nextAction == Action.NextAction.Left)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    default:
                        return act;
                }
            }
            return null;
        }

        public Action GetBestAction()
        {
            Action bestAction = possibleActions[0];
            foreach (Action act in possibleActions)
            {
                if (act.reward > bestAction.reward)
                    bestAction = act;
            }
            return bestAction;
        }
    }
}