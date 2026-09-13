using Cloud.ExamenFinal.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Text.Json;

namespace Cloud.ExamenFinal.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDistributedCache _cache;
        private const string PANIER_KEY_PREFIX = "PR_panier_";

        public HomeController(ILogger<HomeController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        private string GetPanierKey()
        {
            const string sessionInitKey = "SessionInitialized";
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(sessionInitKey)))
            {
                HttpContext.Session.SetString(sessionInitKey, "true");
            }

            var sessionId = HttpContext.Session.Id;
            _logger.LogDebug("Utilisation de la clé panier: {PanierKey}", $"{PANIER_KEY_PREFIX}{sessionId}");
            return $"{PANIER_KEY_PREFIX}{sessionId}";
        }

        private async Task<List<Article>> GetPanierFromCache()
        {
            try
            {
                var panierKey = GetPanierKey();
                _logger.LogInformation("Tentative de récupération du panier avec la clé: {PanierKey}", panierKey);
                var panierJson = await _cache.GetStringAsync(panierKey);

                if (string.IsNullOrEmpty(panierJson))
                {
                    _logger.LogInformation("Panier vide pour la clé {PanierKey}", panierKey);
                    return new List<Article>();
                }

                var panier = JsonSerializer.Deserialize<List<Article>>(panierJson);
                _logger.LogInformation("Panier récupéré avec {Count} articles pour la clé {PanierKey}", panier?.Count ?? 0, panierKey);
                return panier ?? new List<Article>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du panier depuis Redis");
                return new List<Article>();
            }
        }

        private async Task SavePanierToCache(List<Article> panier)
        {
            try
            {
                var panierKey = GetPanierKey();
                var panierJson = JsonSerializer.Serialize(panier);

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                await _cache.SetStringAsync(panierKey, panierJson, options);
                _logger.LogInformation("Panier sauvegardé avec {Count} articles pour la clé {PanierKey}", panier.Count, panierKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du panier dans Redis");
                throw;
            }
        }

      
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Affichage du panier");

            List<Article> panier = await GetPanierFromCache();

            if (panier == null || panier.Count == 0)
            {
                ViewBag.Message = "Votre panier est vide";
                panier = panier ?? new List<Article>();
            }

            return View(panier);
        }

        public IActionResult Ajouter()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ajouter(Article article)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Ajout de l'article {NomArticle} au panier", article.Nom);

                    var panier = await GetPanierFromCache();

                    var articleExiste = panier.Any(a => 
                        string.Equals(a.Nom, article.Nom, StringComparison.OrdinalIgnoreCase));
                    
                    if (!articleExiste)
                    {
                        panier.Add(article);
 
                        await SavePanierToCache(panier);
                        
                        TempData["Success"] = $"Article '{article.Nom}' ajouté au panier avec succès!";
                        _logger.LogInformation("Article ajouté avec succès. Total: {Count} articles", panier.Count);
                    }
                    else
                    {
                        TempData["Warning"] = $"L'article '{article.Nom}' est déjà dans votre panier.";
                        _logger.LogInformation("Article '{NomArticle}' déjà présent, non ajouté", article.Nom);
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'ajout de l'article au panier");
                    TempData["Error"] = "Erreur lors de l'ajout de l'article au panier";
                }
            }

            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult GenererException()
        {
            _logger.LogError("Génération intentionnelle d'une exception pour test");
            throw new NotImplementedException("Exception générée intentionnellement pour tester la journalisation");
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}