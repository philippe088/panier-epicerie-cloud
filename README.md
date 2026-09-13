# Panier d'épicerie cloud – ASP.NET Core, Redis et Azure Functions

Application web de panier d'épicerie conçue pour Azure : le panier de chaque visiteur est conservé dans un cache distribué Redis, l'application est instrumentée avec OpenTelemetry et une Azure Function réagit à l'ajout de fichiers dans un compte de stockage.

> Projet réalisé dans le cours *Déploiement sur l'infonuagique* – AEC Développement d'applications sécuritaires, Cégep de Limoilou (2026).

## Architecture

```
Navigateur ──► PanierEpicerie.MVC ──► Azure Cache for Redis (panier par session)
                     │
                     └──► Application Insights (OpenTelemetry)

Blob Storage (conteneur « fichiers »)
      │  BlobTrigger
      ▼
PanierEpicerie.Functions ──► Azure Service Bus (file « fichiers-ajoutes »)
```

| Projet | Rôle |
|---|---|
| `PanierEpicerie.MVC` | Application MVC : affichage et ajout d'articles au panier |
| `PanierEpicerie.Functions` | Azure Function (isolated) `TraiterFichier` : déclenchée par l'ajout d'un blob, elle publie un message dans Service Bus |

## Points techniques

- **Cache distribué** : panier stocké dans Redis via `IDistributedCache`, avec une clé par session et une expiration glissante de 5 minutes. L'application reste sans état et peut donc être répartie sur plusieurs instances.
- **Sessions** : cookie `HttpOnly`, expiration après 30 minutes d'inactivité.
- **Health checks** : `/health`, `/health/live` (application) et `/health/ready` (inclut la connexion Redis).
- **Observabilité** : OpenTelemetry avec Azure Monitor, journalisation structurée ; une page génère volontairement une exception pour valider la collecte des erreurs.
- **Architecture événementielle** : Blob Storage → Azure Function → Service Bus.
- **Conteneurisation** : Dockerfile multi-stage (image finale `aspnet:8.0`, port 8080).

## Technologies

.NET 8 · ASP.NET Core MVC · Azure Cache for Redis · Azure Functions · Azure Blob Storage · Azure Service Bus · Application Insights · OpenTelemetry · Docker

## Exécution locale

Prérequis : .NET 8 SDK, Docker et Azure Functions Core Tools.

**Application MVC**

```bash
docker run -d -p 6379:6379 redis
cd PanierEpicerie.MVC
dotnet user-secrets init
dotnet user-secrets set "AzureMonitor:ConnectionString" "<chaîne Application Insights>"
dotnet run
```

L'intégration Azure Monitor exige une chaîne de connexion Application Insights valide au démarrage.

**Azure Function**

```bash
cd PanierEpicerie.Functions
cp local.settings.example.json local.settings.json   # puis renseigner les chaînes de connexion
func start
```

**Image Docker**

```bash
cd PanierEpicerie.MVC
docker build -t panier-epicerie .
```
