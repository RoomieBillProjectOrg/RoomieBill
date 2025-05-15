namespace Roomiebill.Server.Common.Validators
{
    /// <summary>
    /// Validates that email addresses conform to BGU University format (@bgu.ac.il or @post.bgu.ac.il).
    /// </summary>
    public class EmailValidator
    {
        private const string BGU_DOMAIN = "@bgu.ac.il";
        private const string POST_BGU_DOMAIN = "@post.bgu.ac.il";

        public string Error { get; set; }

        public EmailValidator()
        {
            Error = "";
        }
        
        /// <summary>
        /// Validates if an email address is a valid BGU University email.
        /// </summary>
        /// <returns>True if email is valid, false otherwise with Error property set.</returns>
        public bool ValidateEmail(string email)
        {
            // Check for null or empty
            if (string.IsNullOrWhiteSpace(email))
            {
                Error = "Email should be a BGU University email";
                return false;
            }

            // Remove any whitespace and convert to lowercase for case-insensitive comparison
            email = email.Trim().ToLower();

            // Check for minimum length and @ symbol
            if (email.Length < BGU_DOMAIN.Length || !email.Contains("@"))
            {
                Error = "Email should be a BGU University email";
                return false;
            }

            // Check for valid BGU domains
            if (!email.EndsWith(BGU_DOMAIN.ToLower()) && !email.EndsWith(POST_BGU_DOMAIN.ToLower()))
            {
                Error = "Email should be a BGU University email";
                return false;
            }

            // Check for spaces after trimming (which would indicate spaces in the middle)
            if (email.Contains(" "))
            {
                Error = "Email should be a BGU University email";
                return false;
            }

            // Check for invalid characters in domain part
            int atIndex = email.IndexOf('@');
            string domain = email.Substring(atIndex);
            if (domain.Any(c => !char.IsLetterOrDigit(c) && c != '@' && c != '.'))
            {
                Error = "Email should be a BGU University email";
                return false;
            }

            // If all checks pass
            Error = "";
            return true;
        }
    }
}
