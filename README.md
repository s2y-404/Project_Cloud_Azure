# PROJET CLOUD AZURE

L’application « Blue’Managemnt » est une application web à destination de développeurs permettant la gestion d’environnements jetables. Une fois connecté, et selon le grade de l’utilisateur, il est possible de créer des machines virtuelles qui sont auto-détruite au bout de 10 minutes.


&nbsp;
![Logo](./front/cloud_azure/assets/image.png)


&nbsp;
## TECHNOLOGIES
Cette application est composée de différentes parties :
- Un frontend élaborée en HTML avec des styles CSS, notamment Bootstrap, et des fonctionnalités dynamiques implémentées en JavaScript, utilisant également la bibliothèque JQuery.
- Un système backend développé en C# et reposant sur le framework ASP.NET.
- Une infrastructure cloud basée sur Azure pour fournir des services et des ressources informatiques à l'application.


&nbsp;
## FONCTIONNEMENTS
### CONFIGURATION
Concernant la configuration, il faut tout d’abord ajouter ces propres données Azure. 
Cela se passe dans le fichier *back > api_cloud_azure > azure-configuration.json*.
```
{
  "clientId": "{ votre client id }",
  "clientSecret": "{ votre client secret }",
  "subscriptionId": "{ votre abonnement id }",
  "tenantId": "{ votre tenant id }"
     ... 
```


&nbsp;
### EXECUTION
Via GitHub
```
git clone https://github.com/s2y-404/Project_Cloud_Azure.git
cd Project_Cloud_Azure
```

Via fichier ZIP
```
Extrais le contenu de ton fichier .zip
cd Project_Cloud_Azure-main
```


&nbsp;
- FRONTEND


&nbsp;
Les commandes suivantes ont pour point de départ le dossier 'Project_Cloud_Azure'
```
cd front/cloud_azure
npm install vite --save-dev
npm run dev
```
un serveur web se lance sur l’url suivant : [http://localhost:5173/](http://localhost:5173/)
- BACKEND


&nbsp;
Les commandes suivantes ont pour point de départ le dossier 'Project_Cloud_Azure'
&nbsp;
```
sudo apt update
sudo apt install -y dotnet-sdk-8.0
cd back/api_cloud_azure
dotnet run
```
l’api se lance sur le port 5290 : [http://localhost:5290/](http://localhost:5290/)


&nbsp;
### EXECUTION
Il y a trois utilisateurs préconfigurés, les identifiants sont inscrits dans le fichier *front >cloud_azure > users.json* :
- L’utilisateur « **Bronze** », n’ayant aucun droit
Identifiant : **user1** ; Mot de passe : **mdp1**
- L’utilisateur « **Silver** », qui peut créer une seule machine virtuelle sous le système d’exploitation Debian
Identifiant : **user2** ; Mot de passe : **mdp2**
- L’utilisateur « **Gold** », qui possèdent tous les droits, ici il peut créer jusqu’à trois machines virtuelles maximum en simultané et sous les OS qu’il souhaite entre Windows, Debian et Ubuntu
Identifiant : **user3** ; Mot de passe : **mdp3**


&nbsp;
### CONNEXION
Pour accéder aux différentes machines virtuelles, deux moyens, en SSH pour les machines virtuelles sous Ubuntu ou Debian et en RDP pour les machines virtuelles sous Windows. Dans les deux cas, la connexion est sécurisée :

Identifiant : **azureaminuser** ; Mot de passe : **Pas$m0rd$123**

Pour la connexion en SSH, il faut ouvrir son terminal et taper la commande suivante, puis suivre les instructions :
```
ssh azureadminuser@{ votre ip }
```
Concernant la connexion en RDP, il faut télécharger un logiciel de connexion a distance tel que [Microsoft Remote Desktop](https://www.microsoft.com/store/productId/9WZDNCRFJ3PS?ocid=pdpshare)

