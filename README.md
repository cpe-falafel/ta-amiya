# Documentation : projet ta-amiya

## Description du projet

Le projet ``ta-amiya`` est l'une des couches backend du projet ``Falafel``. Ce projet est en charge des traitements vidéos en temps réel des flux rtmp. Il permet de traiter les flux vidéos en temps réel en utilisant les filtres pris en charge par ``FFmpeg``. 

## Fonctionnalités
- Filtres pris en charge :
    - ``Drawbox`` : Ajoute une boîte de dessin sur la vidéo.
    - ``Drawtext`` : Ajoute du texte sur la vidéo.
    - ``Flip`` : Applique un effet de retournement horizontal ou vertical.
    - ``Zoom`` : Applique un effet de zoom sur la vidéo.
    - ``Stack`` : Superpose plusieurs flux vidéo.
    - ``Split`` : Divise un flux vidéo en plusieurs flux.

## Prérequis 

1. Installer ``FFmpeg`` sur votre machine afin de pouvoir tester la diffusion de flux RTMP.
    - Windows : ```winget install ffmpeg``` ou [Télécharger ffmpeg](https://ffmpeg.org/download.html)
    - Linux : ```sudo apt install ffmpeg```
    - MacOS : ```brew install ffmpeg```

2. Installer ``.NET SDK 8.0`` ou supérieur.
    - [Télécharger .NET SDK](https://dotnet.microsoft.com/download)

3. Installer ``Docker`` et ``Docker Compose``.
    - [Installer Docker](https://docs.docker.com/get-docker/)
    - [Installer Docker Compose](https://docs.docker.com/compose/install/)

4. Installer ``Git`` pour cloner le projet.
    - [Installer Git](https://git-scm.com/downloads)	

5. Optionnel : Installer ``VLC`` pour visualiser les flux vidéos.
    - [Télécharger VLC](https://www.videolan.org/vlc/index.html)

## Installation et exécution du projet

1. Cloner le projet sur votre machine.
    ```bash
    git clone <URL-du-dépôt>
    cd ta-amiya/WorkerApi
    ```
2. Mdifier la variable d'environnement ``AMIYA_APIKEY`` dans le fichier ``docker-compose.yml``.

3. Lancer le projet en utilisant ``Docker Compose``.
    ```bash
    docker-compose up --build
    ```

4. Ouvrir votre navigateur et accéder à l'adresse suivante : 
``http://localhost:5000/swagger/index.html``.


### Exemple d'utilisation

1. Diffusion d'une vidéo en boucle sur : rtmp://localhost:1935/live/test
    ```bash
    ffmpeg -re -f lavfi -i testsrc=size=1280x720:rate=30 -f lavfi -i sine=frequency=1000 -c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac -ar 44100 -ac 2 -f flv rtmp://localhost:1935/live1/test
    ```

2. Envoie d'une requête POST pour appliquer un filtre ``Drawbox`` sur le flux vidéoe et renvoyer le flux traité sur : rtmp://localhost:1935/live2/test

    ```bash
    curl -X POST "http://localhost:5000/worker/" -H "X-Api-Key: {{token}}" -H "Accept: application/json" -H "Content-Type: application/json" -d "{\"jsonWorkerConfiguration\":\"{\\\"in1\\\":{\\\"type\\\":\\\"_IN\\\",\\\"in\\\":[],\\\"out\\\":[\\\"stream_1\\\",null],\\\"properties\\\":{\\\"src\\\":\\\"rtmp://liveserver:1935/live1/test\\\"}},\\\"filter1\\\":{\\\"type\\\":\\\"drawbox\\\",\\\"in\\\":[\\\"stream_1\\\"],\\\"out\\\":[\\\"stream_2\\\"],\\\"properties\\\":{\\\"color\\\":\\\"#FFFF00\\\",\\\"top\\\":0,\\\"bottom\\\":0,\\\"left\\\":0,\\\"right\\\":0,\\\"thickness\\\":5}},\\\"out1\\\":{\\\"type\\\":\\\"_OUT\\\",\\\"in\\\":[\\\"stream_2\\\",null],\\\"out\\\":[],\\\"properties\\\":{\\\"dst\\\":\\\"rtmp://liveserver:1935/live2/test\\\"}}}\"}"
    ```

3. Visualisation du flux vidéo traité sur : rtmp://localhost:1935/live2/test
    ```bash
    ffplay rtmp://localhost:1935/live2/test
    ```