using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunction.ExamenFinal
{
    public class TraiterFichier
    {
        private readonly ILogger<TraiterFichier> _logger;

        public TraiterFichier(ILogger<TraiterFichier> logger)
        {
            _logger = logger;
        }

        [Function("TraiterFichier")]
        [ServiceBusOutput("ExamenFinal", Connection = "ServiceBusConnection")]
        public string Run(
            [BlobTrigger("examenfinal/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
            string name)
        {
            _logger.LogInformation("Début du traitement du fichier: {FileName}", name);

            try
            {
                var dateHeure = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var message = $"le fichier '{name}' a été ajouté dans le compte de stockage à '{dateHeure}'";

                _logger.LogInformation(message);

                _logger.LogInformation("Message envoyé dans la file Service Bus 'ExamenFinal'");

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du fichier {FileName}", name);
                throw;
            }
        }
    }
}
