namespace DocumentManagmentSystem_Demo.Helpers
{
    public struct MessageResult
    {
        public string Message;
        public bool Status;

        public MessageResult(string m)
        {
            Message = m ?? "";
            Status = false;
        }
    }
}
