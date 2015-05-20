using System;
using System.Collections.Generic;
using LeagueSharp.Common;

namespace iYasuo.ComboManager
{
    internal class Combo
    {
        public List<ComboAction> ActionsList { get; set; }
        public ComboBehaviour ComboBehaviour { get; set; }
        public ComboListener ComboListener { get; set; }

        internal int CurrentComboIndex;

        internal bool InProgress;

        internal ComboListener GetComboListener()
        {
            return ComboListener;
        }

        internal List<ComboAction> GetComboActions()
        {
            return ActionsList;
        }

        internal ComboBehaviour GetComboBehaviour()
        {
            return ComboBehaviour;
        }

        internal int GetCurrentIndex()
        {
            return CurrentComboIndex;
        }

        internal ComboAction GetCurrentAction()
        {
            return CurrentComboIndex <= ActionsList.Count ? ActionsList[GetCurrentIndex()] : null;
        }

        internal ComboAction GetNextAction()
        {
            return CurrentComboIndex + 1 <= ActionsList.Count ? ActionsList[GetCurrentIndex() + 1] : null;
        }
        internal ComboAction GetPreviousAction()
        {
            return CurrentComboIndex - 1 >= 0 ? ActionsList[GetCurrentIndex() + 1] : null;
        }

        internal void IncrementIndex()
        {
            if (GetCurrentAction().IsDone())
            {
                CurrentComboIndex += 1;
                InProgress = false;
            }
        }

        internal void ExecuteCombo()
        {
            if (!InProgress)
            {
                var currentAction = GetCurrentAction();
                if (currentAction != null)
                {
                    GetCurrentAction().ExecuteAction();
                    InProgress = true;
                    Utility.DelayAction.Add(
                        GetCurrentAction().MaxActionCompletionTime, () =>
                        {
                            CurrentComboIndex += 1;
                            InProgress = false;
                        });
                }
                else
                {
                    Console.WriteLine("Something went wrong!");
                }
                
            }
            IncrementIndex();
        }
    }
}
