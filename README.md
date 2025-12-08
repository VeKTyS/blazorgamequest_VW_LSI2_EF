# BlazorGameQuest (blazorgamequest_VW_LSI2_EF)

Jeu d'aventure développé en Blazor WebAssembly (client) + ASP.NET Core Web API (AuthenticationServices).

Contributeurs
- VONG Lucas
- WATINE David

Résumé des fonctionnalités développées
- Génération aléatoire de donjons (suite de salles) via `IDungeonGenerator` / `DungeonGeneratorService`.
- Mécanique d'aventure interactive (`IAdventureService` / `AdventureService`) : exploration, attaque, fuite, repos, fouille, utilisation d'objets.
- Gestion d'objets persistants côté client (`IItemService` / `EFItemService`) et administration via `Pages/Admin.razor`.
- État joueur stocké en session client via `PlayerStateService` et `localStorage` (clé `bg_currentPlayerId`).
- Sauvegarde des résultats d'aventure : `AdventureResult` POSTé vers `api/AdventureResults` (score, date, événements).
- UI : pages `Index`, `Adventure`, `Admin`, `Scores`, `Unauthorized`.
- Affichage de l'historique du joueur connecté ainsi que du tableau des scores prenant le meilleur score de chaque utilisateur (`EFPlayerService` / `InMemoryService` / `IPlayerService`)
- Visualisation des API du fichier `Controller` via Swagger (`AuthenticationServices/Program.cs`) 
- Authentification Keycloak avec gestion des rôles et redirection automatique après login

Tests unitaires
- Emplacement : `BlazorGame.Tests`

Tests ajoutés :

### Tests de Services (13 tests)
- `InMemoryPlayerService_GetAllPlayers_ReturnsEmptyList` - Vérification liste vide initiale
- `InMemoryPlayerService_CreatePlayer_AddsToList` - Création d'un joueur
- `InMemoryPlayerService_GetPlayerById_ReturnsCorrectPlayer` - Récupération par ID
- `InMemoryPlayerService_GetPlayerById_ReturnsNull_WhenNotFound` - Retour null si joueur inexistant
- `InMemoryPlayerService_Authenticate_ReturnsPlayer_WhenCredentialsMatch` - Authentification réussie
- `InMemoryPlayerService_Authenticate_CaseInsensitive` - Authentification insensible à la casse
- `InMemoryPlayerService_Authenticate_ReturnsNull_WhenNoMatch` - Authentification échouée
- `InMemoryPlayerService_AddOrUpdate_AddsNewPlayer` - Ajout d'un nouveau joueur
- `InMemoryPlayerService_AddOrUpdate_UpdatesExistingPlayer` - Mise à jour d'un joueur existant
- `InMemoryPlayerService_EndAndSaveAsync_AddsResult` - Sauvegarde d'un résultat d'aventure
- `InMemoryPlayerService_GetGameResultsForPlayerAsync_ReturnsOrdered` - Résultats triés par date
- `InMemoryPlayerService_GetAllGameResultsAsync_ReturnsAll` - Récupération de tous les résultats

### Tests de Base de Données (13 tests)
- `DbContext_CanAddAndRetrievePlayers` - Ajout et récupération de joueurs
- `DbContext_CanAddAndRetrieveItems` - Ajout et récupération d'objets
- `DbContext_CanAddAndRetrieveMonsters` - Ajout et récupération de monstres
- `DbContext_CanAddAndRetrieveDonjons` - Ajout et récupération de donjons
- `DbContext_CanAddAndRetrieveSalles` - Ajout et récupération de salles
- `DbContext_CanAddAndRetrieveAdventureResults` - Ajout et récupération de résultats
- `DbContext_CanQueryAdventureResultsByPlayer` - Requêtes filtrées par joueur
- `DbContext_CanIncludeSallesInDonjons` - Include/Join avec salles dans donjons
- `DbContext_CanUpdatePlayerScore` - Mise à jour du score d'un joueur
- `DbContext_CanDeletePlayer` - Suppression d'un joueur
- `DbContext_CanDeleteItem` - Suppression d'un objet
- `DbContext_CanHandleComplexDonjonWithAllEntities` - Gestion de donjons complexes avec relations
- `DbContext_CanUpdateMultipleEntities` - Mise à jour multiple simultanée

### Tests PlayersController (5 tests)
- `PlayersController_GetPlayers_ReturnsAll` - Récupération de tous les joueurs
- `PlayersController_PostPlayer_AddsPlayer` - Création d'un nouveau joueur
- `PlayersController_GetPlayerScores_ReturnsScores` - Récupération des scores d'un joueur
- `PlayersController_GetPlayerScores_ReturnsEmptyForNoResults` - Retourne NotFound si pas de scores
- `PlayersController_PutScore_UpdatesValue` - Mise à jour du score

### Tests ItemsController (17 tests)
- `ItemsController_GetAll_ReturnsAll` - Récupération de tous les objets
- `ItemsController_Get_ReturnsItem` - Récupération d'un objet spécifique (2 tests)
- `ItemsController_Get_ReturnsNotFound` - Erreur 404 si objet inexistant (2 tests)
- `ItemsController_Create_AddsNewItem` - Création d'un nouvel objet (2 tests)
- `ItemsController_Update_UpdatesItem` - Mise à jour réussie d'un objet
- `ItemsController_Update_ReturnsBadRequest_WhenIdMismatch` - Erreur 400 si ID ne correspond pas (2 tests)
- `ItemsController_Update_ReturnsNotFound_WhenItemDoesNotExist` - Erreur 404 si objet inexistant (2 tests)
- `ItemsController_Delete_RemovesItem` - Suppression d'un objet
- `ItemsController_Delete_ReturnsNotFound_WhenMissing` - Erreur 404 lors de suppression
- `ItemsController_GetAll_ReturnsMultipleItems` - Retour de plusieurs objets
- `ItemsController_GetAll_ReturnsEmptyList_WhenNoItems` - Liste vide si aucun objet

### Tests DonjonsController (13 tests)
- `DonjonsController_GetAll_ReturnsAll` - Récupération de tous les donjons
- `DonjonsController_Get_ReturnsDonjon` - Récupération d'un donjon spécifique
- `DonjonsController_Get_ReturnsDonjonWithSalles` - Récupération avec salles incluses (2 tests)
- `DonjonsController_Get_ReturnsNotFound` - Erreur 404 si donjon inexistant (2 tests)
- `DonjonsController_Create_AddsNewDonjon` - Création d'un nouveau donjon (2 tests)
- `DonjonsController_Update_UpdatesDonjon` - Mise à jour réussie d'un donjon
- `DonjonsController_Update_ReturnsBadRequest_WhenIdMismatch` - Erreur 400 si ID ne correspond pas (2 tests)
- `DonjonsController_Update_ReturnsNotFound_WhenDonjonDoesNotExist` - Erreur 404 si donjon inexistant (2 tests)
- `DonjonsController_Delete_RemovesDonjon` - Suppression d'un donjon
- `DonjonsController_Delete_ReturnsNotFound_WhenMissing` - Erreur 404 lors de suppression
- `DonjonsController_GetAll_ReturnsMultipleDonjons` - Retour de plusieurs donjons
- `DonjonsController_GetAll_ReturnsEmptyList_WhenNoDonjons` - Liste vide si aucun donjon

### Tests MonstersController (15 tests)
- `MonstersController_GetAll_ReturnsAll` - Récupération de tous les monstres
- `MonstersController_Get_ReturnsNotFound` - Erreur 404 si monstre inexistant (2 tests)
- `MonstersController_Create_AddsNewMonster` - Création d'un nouveau monstre (2 tests)
- `MonstersController_Update_UpdatesMonster` - Mise à jour réussie d'un monstre
- `MonstersController_Update_ReturnsBadRequest_WhenIdMismatch` - Erreur 400 si ID ne correspond pas (2 tests)
- `MonstersController_Update_ReturnsNotFound_WhenMonsterDoesNotExist` - Erreur 404 si monstre inexistant (2 tests)
- `MonstersController_Delete_RemovesMonster` - Suppression d'un monstre
- `MonstersController_Delete_ReturnsNotFound_WhenMissing` - Erreur 404 lors de suppression
- `MonstersController_GetAll_ReturnsMultipleMonsters` - Retour de plusieurs monstres
- `MonstersController_GetAll_ReturnsEmptyList_WhenNoMonsters` - Liste vide si aucun monstre
- `DbContext_CanQueryBossMonstersOnly` - Requête pour filtrer les boss uniquement

### Tests SallesController (15 tests)
- `SallesController_GetAll_ReturnsAll` - Récupération de toutes les salles
- `SallesController_Get_ReturnsNotFound` - Erreur 404 si salle inexistante (2 tests)
- `SallesController_Create_AddsNewSalle` - Création d'une nouvelle salle (2 tests)
- `SallesController_Update_UpdatesSalle` - Mise à jour réussie d'une salle
- `SallesController_Update_ReturnsBadRequest_WhenIdMismatch` - Erreur 400 si ID ne correspond pas (2 tests)
- `SallesController_Update_ReturnsNotFound_WhenSalleDoesNotExist` - Erreur 404 si salle inexistante (2 tests)
- `SallesController_Delete_RemovesSalle` - Suppression d'une salle
- `SallesController_Delete_ReturnsNotFound_WhenMissing` - Erreur 404 lors de suppression
- `SallesController_GetAll_ReturnsMultipleSalles` - Retour de plusieurs salles
- `SallesController_GetAll_ReturnsEmptyList_WhenNoSalles` - Liste vide si aucune salle

### Tests AdventureResultsController (5 tests)
- `AdventureResultsController_Create_AddsNewResult` - Création d'un nouveau résultat
- `AdventureResultsController_GetAll_ReturnsAll` - Récupération de tous les résultats
- `AdventureResultsController_Get_NotFound_WhenMissing` - Erreur 404 si résultat inexistant
- `AdventureResultsController_Delete_RemovesResult` - Suppression d'un résultat
- `AdventureResultsController_Delete_ReturnsNotFound_WhenMissing` - Erreur 404 lors de suppression

### Tests de Domaine/Modèles (14 tests)
- `Player_DefaultValues_AreCorrect` - Vérification des valeurs par défaut du joueur
- `Player_CanSetAllProperties` - Modification de toutes les propriétés du joueur
- `Item_DefaultValues_AreCorrect` - Vérification des valeurs par défaut d'un objet
- `Item_CanSetAllProperties` - Modification de toutes les propriétés d'un objet
- `Monstre_DefaultValues_AreCorrect` - Vérification des valeurs par défaut d'un monstre
- `Monstre_CanSetAllProperties` - Modification de toutes les propriétés d'un monstre
- `Donjon_DefaultValues_AreCorrect` - Vérification des valeurs par défaut d'un donjon
- `Salle_DefaultValues_AreCorrect` - Vérification des valeurs par défaut d'une salle
- `AdventureResult_DefaultValues_AreCorrect` - Vérification des valeurs par défaut d'un résultat
- `AdventureResult_CanSetAllProperties` - Modification de toutes les propriétés d'un résultat
- `Salle_CanAddMultipleItems` - Ajout de plusieurs objets dans une salle
- `Salle_CanAddMultipleMonstres` - Ajout de plusieurs monstres dans une salle
- `Salle_CanAddConnectedRooms` - Connexion de plusieurs salles
- `Donjon_CanAddMultipleSalles` - Ajout de plusieurs salles dans un donjon

### Tests de Requêtes et Scénarios Complexes (5 tests)
- `DbContext_CanQueryPlayersByScore` - Requête pour filtrer les joueurs par score
- `DbContext_CanQueryAdmins` - Requête pour filtrer les administrateurs
- `DbContext_CanQueryItemsByHealthEffect` - Requête pour filtrer les objets par effet de santé
- `DbContext_CanQueryAdventureResultsByDateRange` - Requête pour filtrer les résultats par plage de dates
- `Player_InventoryManagement` - Gestion de l'inventaire du joueur

### Couverture par module
- **SharedModels** : 100% de couverture (tous les modèles testés)
- **AuthenticationServices** : 80% de couverture

Comment lancer le projet (développement local)
1) Cloner le dépôt

2) Lancer avec Docker Compose (recommandé)
```bash
docker compose up --build
```
