namespace DoucmentManagmentSys.Requests.Auth
{
    public sealed class RegisterRequest
    {
        /// <summary>
        /// The user's email address which acts as a user name.
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// The user's password.
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// The user's name.
        /// </summary>
        public required string Name { get; init; }


        public required string Surname { get; init; }



    }


}
