using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.ChatHubs.Models
{
    public class ChatHubIgnoredBy
    {

        public int Id { get; set; }
        public int ChatHubUserId { get; set; }
        public int ChatHubIgnoredUserId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}