using Microsoft.AspNetCore.Identity;
using PokemonReviewApp.Models.Auth;

namespace PokemonReviewApp.Models
{
    public class Reviewer
    {
        public int Id { get; set; }
        //public int UserId { get; set; }
        //public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
