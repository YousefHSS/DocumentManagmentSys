using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Identity;

namespace DoucmentManagmentSys.Helpers.Auth
{
    public class PrimacyUserValidator : UserValidator<PrimacyUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<PrimacyUser> manager, PrimacyUser user)
        {
            var result = await base.ValidateAsync(manager, user);
            var errors = result.Errors.ToList();

            // Remove the error related to username uniqueness
            errors.RemoveAll(e => e.Code == "DuplicateUserName");

            // Check if Name is provided
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                errors.Add(new IdentityError
                {
                    Code = "NameRequired",
                    Description = "Name is required."
                });
            }

            // Check if Surname is provided
            if (string.IsNullOrWhiteSpace(user.Surname))
            {
                errors.Add(new IdentityError
                {
                    Code = "SurnameRequired",
                    Description = "Surname is required."
                });
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }

}
