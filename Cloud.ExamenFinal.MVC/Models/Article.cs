
using System.ComponentModel.DataAnnotations;

namespace Cloud.ExamenFinal.MVC.Models
{
    public class Article
    {

        [Required]
        [Display(Name ="Nom de l'élément")]
        public string Nom { get; set; } = string.Empty;
    }
}
