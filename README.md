# TAG GOLD : ChessMulti

Auteur : 
* PIRIS Marius
* LAHALLE François

Temps de Développement : *1 semaine et demie*

[Build](https://drive.google.com/file/d/1VkkqO6QBnrAZdK7O_b27U9LgCwxBi4-h/view?usp=sharing)
[Vidéo](https://youtu.be/el7IXvb3QWc)

## But du sujet

Le but du projet est de créer un mode multijoueur en réseau local pour un jeu d'échecs.
Le programme devra être capable de gérer la connexion et l’échange de paquets réseau entre un client-serveur (listen server) et au moins un autre client.
Vous devez modifier le jeu d’échecs fourni pour qu’il fonctionne en multijoueur.

## Méthode de lancement

**Pour ce projet, il est préférable d'utiliser deux machines différentes sur le même réseau local pour une meilleure expérience.**

Vous arriverez sur le menu principal avec plusieurs choix :

* Créer une "Room" ("Lobby")
* Rejoindre une "Room"
* Changer son pseudo

#### Créer une "Room"

En créant une "Room", vous accédez à un panel de fonctionnalités comme rejoindre deux équipes, lancer la partie ou la quitter.
Tant que deux joueurs n'ont pas rejoint les équipes, la partie ne pourra pas être lancée.
Les autres joueurs seront automatiquement spectateurs (nous n'avons pas terminé la gestion de l'UI pour afficher les spectateurs dans l'emplacement réservé).
Quand l'hôte quitte la "Room", tout le monde est exclu.

#### Joindre une "Room"

Vous devez utiliser l'IP qui se trouve sur l'écran d'affichage de la "Room" créée par l'hôte, puis appuyer sur le bouton "Connecter".
Ensuite, vous vous trouverez dans la "Room", sinon un message d'erreur s'affichera et vous pourrez retourner au lobby.

#### Changer son Pseudo

Vous pouvez mettre le pseudo que vous souhaitez, cela vous permettra d'être identifiable dans le chat par des messages du serveur ou par les messages que vous écrivez.
Également, votre pseudo sera affiché si vous êtes le gagnant d'une partie.

#### En jeu

Les blancs commencent. Chaque personne peut toucher aux pièces présentes sur le terrain (un peu comme dans la vraie vie), mais seuls les mouvements des pièces des joueurs seront acceptés.
Par exemple, je joue les blancs, je peux bouger les pièces noires, mais elles reviendront à leurs emplacements respectifs (le mouvement n'est pas enregistré).

Tour par tour, les deux joueurs s'affrontent et le premier qui fait tomber le roi a gagné.

Un texte de victoire s'affichera avec le nom du joueur, deux boutons : le premier pour relancer une partie, qui affichera les scores modifiés, et le second pour quitter.
Lorsque l'hôte quitte, l'autre joueur n'a que le choix de quitter, pareil pour les spectateurs.
Tandis que si l'opposant quitte, l'hôte peut relancer quand même, sans intérêt puisqu'il n'a plus d'adversaire. Nous devons donc encore rajouter une sécurité.


## Améliorations envisageables : 

* Ajouter l'UI pour les spectateurs qui rejoignent.
* Lorsque l'un des deux joueurs quitte après la fin de la partie, l'autre revient au "Lobby" avec tous les spectateurs.
* Créer des chats spécifiques (only spectators, only players, chat all…).
* Avoir un bouton pour quitter en jeu.
* Lorsqu'un joueur en pleine partie quitte, déclarer la victoire par forfait.
* Afficher un message en UI lorsque la "Room" est fermée à tous les joueurs qui ont été exclus (nous en avons un par le chat avec "Server is shutting down").
* Créer un bouton pour exclure des joueurs

## Déroulement du projet

### Commencement

Tout d'abord, nous avons décidé de commencer par découvrir le TCP/IP en créant un client et un serveur dans un projet C# Console App afin de nous familiariser.
C'est à ce moment-là que nous avons compris comment créer un serveur, connecter plusieurs clients, puis envoyer des messages entre les clients et le serveur (et inversement) ainsi que les recevoir.
Après nous être familiarisés avec le TCP/IP, nous avons commencé à faire le projet ChessMulti.

### ChessMulti


En reprenant le code que nous avions fait auparavant, nous avons pu facilement adapter celui-ci sur Unity en utilisant du multi-threading. Ainsi, le serveur et les clients tournent sur des threads à part de celui d'Unity.

Pour obtenir des tests rapides et concluants sur la façon dont nous envoyons les données, nous avons pris le risque de commencer par la création d'un chat entre deux machines en local. Cela nous semblait plus simple de réaliser cette tâche, car dès que nous avions une base d'envoi solide pour de simples messages, il nous suffisait alors de faire pareil avec d'autres types de données.

Nous nous sommes heurtés à un gros problème, la gestion de l'UI d'Unity avec d'autres threads. Nous avions compris dès lors que le thread principal d'Unity était le seul à pouvoir modifier l'UI. Donc nous avons créé des booléens qui, dans l'Update des MonoBehaviour, nous ont permis de modifier l'UI.

Mais également, des problèmes pour appeler des fonctions. Nous avons donc décidé de créer un blackboard pour contenir des actions, et comme cela au niveau du client, nous avons juste à appeler l'action sans se soucier de comment on traite les données (c'est généralisé).

Très rapidement, notre chat était fonctionnel, alors nous avons enchaîné avec les actions de jeu (bouger les pièces du jeu d'échecs sur toutes les machines, modifier l'UI…).

Au fur et à mesure de notre progression dans la construction du jeu, les bugs se sont accumulés mais restaient souvent les mêmes (problèmes de changement d'UI principalement). Nous avions remarqué également que parfois, au début de la création d'un "Lobby", lorsqu'un nouveau client rejoint le "Lobby", l'ID réclamé pour ce client n'était pas récupéré.
Pour pallier cela, nous avons donc fait la demande plusieurs fois tant que l'ID n'était pas récupéré (nous savons que ce n'est pas propre, mais nous n'avons guère eu le choix ; le bug survenait surtout lorsqu'on essayait de se connecter sur deux builds sur une même machine, tandis qu'entre deux machines différentes le bug n'était jamais présent. Nous pensons donc que c'est dû au focus de la fenêtre).

### Crunch

Pour le crunch, nous avons réglé des bugs principalement visuels et de gameplay (comme la rotation de la caméra quand on est dans l'équipe noire).
Nous avons également essayé de trouver une autre solution que l'Update pour récupérer l'ID du joueur sans devoir le réclamer tant qu'il est vide.
Lorsqu'on quittait la partie à la fin, les pièces restantes étaient toujours affichées, donc nous avons corrigé cela.

