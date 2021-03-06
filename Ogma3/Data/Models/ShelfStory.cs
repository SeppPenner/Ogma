using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ogma3.Data.Models
{
    public class ShelfStory
    {
        [JsonIgnore]
        [Required]
        public  Shelf Shelf { get; set; }
        [Required]
        public long ShelfId { get; set; }

        [JsonIgnore]
        [Required]
        public  Story Story { get; set; }
        [Required]
        public long StoryId { get; set; }
    }
}