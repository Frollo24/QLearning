using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        private QTable learningTable = new QTable();

        public bool StartsLearning = true;
        public bool WatchLearning = true;

        public bool isLearning { get; private set; }
        public int numEpisodes = 10;
        public int numIterations = 200;

        private int episode = 0;
        private int iter = 0;

        private float waitLearn = 0;
        string dots = "";
        public override void Repath()
        {
            //TODO clear Q-table
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            // If the Q-table is empty, fill the Q-table and checks if it starts learning
            if (learningTable.isEmpty)
            {
                learningTable.FillTable(boardInfo, currentPos, goals);
                isLearning = StartsLearning;
            }

            // When learning, it shows a animated waiting message
            waitLearn += Time.deltaTime;
            if (waitLearn > 0.025f) dots = ".";
            if (waitLearn > 0.05f) dots = "..";
            if (waitLearn > 0.075f) dots = "...";
            if (waitLearn > 0.1f) { waitLearn = 0.0f; dots = ""; }

            // Actual Learning
            if (isLearning)
            {
                return Learn(boardInfo, currentPos, goals);
            }


            //TODO not learning code
            learningTable.UpdateCurrentState(boardInfo, currentPos, goals);
            var bestMove = GetBestMove();
            learningTable.CurrentState.GetNextAction(bestMove);

            return bestMove;
            //return Locomotion.MoveDirection.Left;
        }

        private Locomotion.MoveDirection GetLearningMove()
        {
            Locomotion.MoveDirection nextMove;
            var rand = UnityEngine.Random.Range(0, 1);
            var reachBest = ExplorationRandomness.rho * ExplorationRandomness.ExploringFunc(episode);
            nextMove = (rand <= reachBest) ? GetRandomMove() : GetBestMove();
            return nextMove;
        }

        private Locomotion.MoveDirection GetRandomMove()
        {
            var val = Random.Range(0, 4);
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;
        }

        private Locomotion.MoveDirection GetBestMove()
        {
            Action.NextAction action = learningTable.CurrentState.GetBestAction().nextAction;
            if (action == Action.NextAction.Up)
                return Locomotion.MoveDirection.Up;
            if (action == Action.NextAction.Right)
                return Locomotion.MoveDirection.Right;
            if (action == Action.NextAction.Down)
                return Locomotion.MoveDirection.Down;
            if (action == Action.NextAction.Left)
                return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.None;
        }
        private Locomotion.MoveDirection Learn(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            var newDir = Locomotion.MoveDirection.None;
            if (WatchLearning)
            {
                Time.timeScale = 50.0f;
                newDir = ShowLearningProcess(boardInfo, currentPos, goals);
                Debug.LogWarning("Player is learning, please wait...");
            }
            else
            {
                Time.timeScale = 1.0f;
                learningTable.LearnTable(episode, numEpisodes, iter, numIterations);
                learningTable.UpdateCurrentState(boardInfo, currentPos, goals);
                isLearning = false;
            }


            return newDir;
        }

        private Locomotion.MoveDirection ShowLearningProcess(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (episode < numEpisodes)
            {
                if (iter < numIterations)
                {
                    //Get a random new action
                    learningTable.UpdateCurrentState(boardInfo, currentPos, goals);
                    var randomMove = GetLearningMove();
                    var nextAction = learningTable.CurrentState.GetNextAction(randomMove);

                    if (nextAction != null)
                        learningTable.UpdateCSVContent(nextAction);

                    //New iteration done
                    iter++;

                    // If the player reaches the goal, it finish the episode
                    if (nextAction != null)
                        if (nextAction.NextState.IsExit) iter = numIterations;

                    //Make the move
                    return randomMove;
                }
                else
                {
                    //Finish the episode
                    episode++;
                    iter = 0;
                    Debug.Log("Episode Finished");
                    learningTable.SaveCSVContent();

                    return Locomotion.MoveDirection.None;


                }
            }
            else
            {
                //Finished learning
                Debug.Log("Finished learning");
                learningTable.SaveCSVContent();
                isLearning = false;
                Time.timeScale = 1.0f;
                return Locomotion.MoveDirection.None;
            }
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = 24
            };

            if (isLearning)
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(20, 15, 400, 400), "Player is now learning, please wait" + dots, style);
                style.normal.textColor = Color.white;
                GUI.Label(new Rect(20, 55, 400, 400), string.Format("Episode: {0}", episode), style);
                GUI.Label(new Rect(20, 95, 400, 400), string.Format("Iteration: {0}", iter), style);
            }
            else if(WatchLearning)
            {
                style.normal.textColor = Color.white;
                GUI.Label(new Rect(20, 15, 400, 400), "Finished learning!", style);
            }
        }
    }
}
