using System.ComponentModel.DataAnnotations;

namespace ESS.Models
{
    public class FileMapping
    {
        [Key]
        public int FileId { get; set; }
        public string FilePath { get; set; }

    }
}
