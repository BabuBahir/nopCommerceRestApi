namespace Nop.Plugin.Api.Rest.Models.Requests
{
    /// <summary>
    /// Represents a request to update a customer. Every property is optional: only the properties
    /// that are supplied are written, the omitted ones keep the value they already have. Roles are
    /// deliberately not part of the payload, because assigning one would be privilege escalation
    /// through an endpoint that accepts no role on creation either.
    /// </summary>
    public class UpdateCustomerRequest
    {
        /// <summary>
        /// Gets or sets the email address. Must be a valid address and unique across customers
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the username. Accepted only when the store has usernames enabled
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the first name. An empty string clears it
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name. An empty string clears it
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the phone number. An empty string clears it
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the admin comment. An empty string clears it
        /// </summary>
        public string? AdminComment { get; set; }

        /// <summary>
        /// Gets or sets whether the customer may sign in
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or sets a new password. When supplied the old one is not checked, so this is an
        /// administrator reset rather than a self-service change
        /// </summary>
        public string? Password { get; set; }
    }
}
