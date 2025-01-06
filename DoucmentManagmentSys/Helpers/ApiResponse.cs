namespace DoucmentManagmentSys.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public dynamic Data { get; set; }
        public string Message { get; set; }
        public dynamic Errors { get; set; }

        public int StatusCode { get; set; }
    }

}
