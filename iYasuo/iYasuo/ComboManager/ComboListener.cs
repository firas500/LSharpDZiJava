namespace iYasuo.ComboManager
{
    class ComboListener
    {
        public bool HasOccured;

        public void SetOccurred()
        {
            HasOccured = true;
        }

        public void ResetOccurred()
        {
            HasOccured = false;
        }

        public bool GetOccured()
        {
            return HasOccured;
        }
    }
}
