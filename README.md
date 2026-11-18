# 02-dockerfile · Le Dockerfile multi-stage

Même API qu'en 01 (données en mémoire) + `Dockerfile` et `.dockerignore`.

## Lancer

```bash
docker build -t workshop-api .
docker run -d --name workshop-api -p 8080:8080 workshop-api
docker ps
```

Ouvrir http://localhost:8080 puis http://localhost:8080/info (`machine` = id du conteneur, `inContainer` = true).

## Pour comprendre

- 3 étapes (`base`, `build`, `final`) : l'image finale ne contient que le runtime et les binaires publiés.
- Le `.csproj` est copié avant le reste du code : relancez `docker build` après avoir modifié `Program.cs` et regardez les layers **CACHED**.
- `-p 8080:8080` = `PORT_HÔTE:PORT_CONTENEUR`. Essayez `-p 5000:8080`, puis lancez **sans** `-p` : le conteneur tourne mais reste injoignable.

Nettoyage : `docker rm -f workshop-api`.

Étape suivante : **03-postgres**.
