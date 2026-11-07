namespace Nop.Plugin.Api.Rest.Models.Requests
{
    /// <summary>
    /// Represents a request to create a registered customer. Roles are deliberately not part of the
    /// payload: the API has one shared key with no scopes, so accepting a role here would let any key
    /// holder create an administrator. A new customer always lands in the Registered role.
    /// </summary>
    public class CreateCustomerRequest
    {
        /// <summary>
        /// Gets or sets the email address. Required and must be unique
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the password. Required; stored with the store's default password format
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the username. Required only when the store has usernames enabled, otherwise
        /// the email address is used
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets whether the customer may sign in. Defaults to true
        /// </summary>
        public bool? Active { get; set; }
    }
}
