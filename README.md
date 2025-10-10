# blazorgamequest_VW_LSI2_EF
BlazorGameQuest - Jeu d’aventure - Projet en continu – Développement Agile (Environnement DOTNET et C#)

# Lancer le projet 

- Cloner le projet
- Deplacez vous dans le dossier blazorgame.Client
- Exécutez avec dotnet run

# Tests unitaires prévisionnels

## Tests liés au joueur

- Vérifier la création d’un joueur valide avec nom, classe, points de vie initiaux et inventaire vide.
- Vérifier qu’une exception est levée lors de la création d’un joueur sans nom.
- Vérifier que les statistiques du joueur augmentent après une mise à jour du score ou un gain d’expérience.
- Vérifier la perte de points de vie après un combat.
- Vérifier que le joueur passe à l’état « Game Over » lorsque ses points de vie atteignent zéro.
- Vérifier que l’utilisation d’une potion restaure correctement les points de vie.
- Vérifier que l’ajout d’un objet dans l’inventaire fonctionne correctement.
- Vérifier que l’équipement ou le retrait d’une arme/armure modifie correctement les statistiques du joueur.

## Tests liés aux salles

- Vérifier la création d’une salle.
- Vérifier qu’une salle contient bien des ennemis lorsque cela est prévu.
- Vérifier qu’une salle contient au moins un trésor / piège ou un objet lorsqu’elle est censée en avoir.
- Vérifier que le déplacement du joueur met bien à jour la salle courante.

## Tests liés au donjon

- Vérifier que la génération du donjon crée le nombre attendu de salles.
- Vérifier qu’aucune salle n’est orpheline (chaque salle a au moins une connexion valide).
- Vérifier qu’aucun identifiant de salle n’est dupliqué.

## Tests système et logique

- Vérifier la cohérence des données (inventaire valide, objets existants).
- Vérifier que la sauvegarde et le chargement d’une partie restaurent correctement l’état du joueur et du donjon.
- Vérifier que le calcul des dégâts en combat est correct selon les statistiques du joueur et de l’ennemi.
- Vérifier que l’ordre des tours de combat est respecté (initiative, vitesse, etc.).
- Vérifier qu’une exception est levée lors de la création d’une salle invalide (identifiant manquant, lien incorrect).
- Vérifier que les valeurs par défaut (points de vie initiaux, stats minimales) sont correctes.
