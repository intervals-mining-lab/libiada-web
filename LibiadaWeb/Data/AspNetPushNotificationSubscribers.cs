using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibiadaWeb.Data
{
    public class AspNetPushNotificationSubscribers
    {
        [Key]
        [PersonalData]
        public int Id { get; set; }

        [Required]
        [PersonalData]
        [ForeignKey("FK_AspNetPushNotificationSubscribers_AspNetUsers_UserId")]
        public int UserId { get; set; }

        [Required]
        public string Endpoint { get; set; }

        [Required]
        public string P256dh { get; set; }

        [Required]
        public string Auth { get; set; }

        public IdentityUser<int> User { get; set; }
    }
}
