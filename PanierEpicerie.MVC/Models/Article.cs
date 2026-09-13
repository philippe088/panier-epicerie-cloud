
using System.ComponentModel.DataAnnotations;

namespace PanierEpicerie.MVC.Models
{
    public class Article
    {

        [Required]
        [Display(Name ="Nom de l'élément")]
        public string Nom { get; set; } = string.Empty;
    }
}
