namespace iYasuo.ComboManager
{
    class ComboListener
    {
        public bool HasOccured;

        public void SetOccurred()
        {
            HasOccured = true;
        }

        public bool GetOccured()
        {
            return HasOccured;
        }
    }
}
