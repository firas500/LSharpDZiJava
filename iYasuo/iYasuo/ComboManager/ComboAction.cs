using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iYasuo.ComboManager
{
    internal class ComboAction
    {
        public delegate void OnAction();

        public ComboListener CurrentActionListener { get; set; }

        public ComboActionType ActionType { get; set; }

        public OnAction Action { get; set; }

        public int MaxActionCompletionTime { get; set; }

        internal bool IsDone()
        {
            return CurrentActionListener.GetOccured();
        }

        internal void ExecuteAction()
        {
            ////TODO: This might not work because of sandbox.
            Action.Invoke();
        }
    }
}
