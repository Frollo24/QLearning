using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Data;  
using UnityEngine;

namespace Assets.Scripts.DataStructures
{
    public class QTable
    {
        private string CSVfile;
        private int seedUsed;
        public bool isEmpty { get; private set; }

        private int numRows, numColumns;
        private float[,] rewardMap;

        public State CurrentState;

        //States = Rows
        public List<State> States;
        //Actions = Columns

        public QTable()
        {
            isEmpty = true;
            CSVfile = "Assets\\Scripts\\Grupo 12\\LearnedData\\";
            States = new List<State>();
        }

        public void FillTable(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            // Setting up the reward map for future readings
            numRows = boardInfo.NumRows;
            numColumns = boardInfo.NumColumns;

            rewardMap = new float[numColumns * numRows, 4];

            // We obtain the seed value, so we can explore faster if we have already visited
            // this board
            seedUsed = boardInfo.seed;
            CSVfile += seedUsed.ToString() + "-" + numColumns.ToString() + "-" + numRows.ToString() + ".csv";
            Debug.LogWarning(CSVfile);

            // Read the CSV reward values
            ReadCSVContent();
            

            // Add every cell state into the table
            foreach (CellInfo cell in boardInfo.CellInfos)
            {
                State stateToAdd = new State(cell, boardInfo.Exit);
                States.Add(stateToAdd);

                if (cell == currentPos)
                    CurrentState = stateToAdd;
            }

            // Links every action whit its next state
            foreach (State st in States)
            {
                CellInfo[] possibleActions = st.cellInfo.GetNeighbours(boardInfo);
                List<Action> addActions = new List<Action>();
                int i = 0;
                foreach (CellInfo action in possibleActions)
                {
                    if (action == null)
                    {
                        rewardMap[st.cellInfo.ColumnId * numRows + st.cellInfo.RowId, i] = ReinforcementFunc.punish;

                        i++;
                        continue;
                    }

                    // Obtains next state for each action from the State list
                    Action actionToAdd = new Action();
                    bool stateAlreadyAdded = false;
                    foreach (State st1 in States)
                    {
                        if (st1.cellInfo == action)
                        {
                            actionToAdd.NextState = st1;
                            stateAlreadyAdded = true;
                        }
                        if (stateAlreadyAdded)
                            break;
                    }

                    if (!stateAlreadyAdded)
                        actionToAdd.NextState = new State(action, boardInfo.Exit);
                    actionToAdd.SetReinforce();
                    actionToAdd.nextAction = (Action.NextAction)i;
                    actionToAdd.SetReward(rewardMap[st.cellInfo.ColumnId * numRows + st.cellInfo.RowId, i]);

                    addActions.Add(actionToAdd);
                    i++;
                }

                st.possibleActions = addActions;
            }

            isEmpty = false;
        }

        public void LearnTable(int episode, int numEpisodes, int iter, int numIterations)
        {
            for(int ep = episode; ep < numEpisodes; ep++)
            {
                int it = iter;
                bool finishEpisode = false;
                while(it < numIterations && !finishEpisode)
                {
                    Action doAction = GetLearningAction(episode);
                    UpdateCSVContent(doAction);
                    CurrentState = UpdateCurrentState(doAction);

                    if (CurrentState.IsExit) finishEpisode = true;

                    it++;
                }
            }
            SaveCSVContent();
        }

        private Action GetLearningAction(int episode)
        {
            Action doAction = null;

            var rand = UnityEngine.Random.Range(0, 1);
            var reachBest = ExplorationRandomness.rho * ExplorationRandomness.ExploringFunc(episode);
            doAction = (rand <= reachBest) ? GetRandomNextAction() : CurrentState.GetBestAction();

            if (doAction == null)
                Debug.Log("Action is null");

            return doAction;
        }

        private Action GetRandomNextAction()
        {
            var rand = UnityEngine.Random.Range(0, 4);
            foreach (Action act in CurrentState.possibleActions)
            {
                switch (rand)
                {
                    case 0:
                        if (act.nextAction == Action.NextAction.Up)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case 1:
                        if (act.nextAction == Action.NextAction.Right)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case 2:
                        if (act.nextAction == Action.NextAction.Down)
                        {
                            act.UpdateReward();
                            return act;
                        }
                        break;
                    case 3:
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
            return CurrentState.possibleActions[0];
        }

        public void UpdateCurrentState(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            foreach (State st in States)
            {
                if(st.cellInfo == currentPos)
                {
                    CurrentState = st;
                    break;
                }
            }
        }

        private State UpdateCurrentState(Action doAction)
        {
            foreach (Action act in CurrentState.possibleActions)
            {
                if (act.nextAction == doAction.nextAction)
                    return act.NextState;
            }
            return doAction.NextState;
        }

        public void ReadCSVContent()
        {
            try
            {
                using (StreamReader file = new StreamReader(@CSVfile, false))
                {
                    int r = 0;
                    while (!file.EndOfStream)
                    {
                        string line = file.ReadLine();
                        var values = line.Split(',');

                        rewardMap[r, 0] = float.Parse(values[0], CultureInfo.InvariantCulture);
                        rewardMap[r, 1] = float.Parse(values[1], CultureInfo.InvariantCulture);
                        rewardMap[r, 2] = float.Parse(values[2], CultureInfo.InvariantCulture);
                        rewardMap[r, 3] = float.Parse(values[3], CultureInfo.InvariantCulture);
                        r++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("File was not found: " + ex);
            }
        }

        public void UpdateCSVContent(Action action)
        {
            //TODO add function logic
            for(int i = 0; i < States.Count; i++)
            {
                if (CurrentState == States[i])
                {
                    if (action == null)
                        Debug.Log("Action is null");
                    rewardMap[i, (int)action.nextAction] = action.reward;
                }
            }
        }

        public void SaveCSVContent()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@CSVfile, false))
                {
                    for (int i = 0; i < rewardMap.GetLength(0); i++)
                    {
                        file.WriteLine(rewardMap[i, 0].ToString(CultureInfo.InvariantCulture) + ","
                            + rewardMap[i, 1].ToString(CultureInfo.InvariantCulture) + "," 
                            + rewardMap[i, 2].ToString(CultureInfo.InvariantCulture) + ","
                            + rewardMap[i, 3].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Something happened: " + ex);
            }
        }
    }
}
