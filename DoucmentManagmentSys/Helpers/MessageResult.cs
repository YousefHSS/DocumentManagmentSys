using DocumentFormat.OpenXml.EMMA;

namespace DoucmentManagmentSys.Helpers
{
    public struct MessageResult
    {
        public string Message;
        public bool Status;
        public dynamic? Info;

        public MessageResult(string m)
        {
            Message = m ?? "";
            Status = false;
            Info = null;
            
        }
    }
}
