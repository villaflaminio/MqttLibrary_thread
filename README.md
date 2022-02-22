# MqttLibrary_thread
Docuemntazione : https://www.notion.so/NET-8ca20b34bd51410fa2f0233858178d64
- implementazione di Publisher , Subscriber , Broker MQTT 
- implementazione di Entity framework con mysql lite
- implementazione di multithreading per gestire i messaggi piu' velocemente
- 
- implementazione di un "Dispatcher", che pendeva i messaggi dal subscriber e li elabborasse con dei thread (tipo se fossero dei worker)
- Il dispatcher misura il numero di richieste al secondo in arrivo ed istanzia un thread ogni fattore di 10 richieste al secondo (20req/s = 2th , 60 req/s = 6 th, etc...). fino a raggiungere il massimo numero di thread (che dobbiamo definire noi ).
- chiaramente quando sono disponibili piu' thread di quelli necessari , il dispatcher li arresta.
- dopo aver instanziato i thread il dispatcher li salva in una sua mappa e successivamente "distribuisce" un messaggio da elaborare a ciascun worker.
- Se il worker non ha messaggi da elaborare per "X" secondi , si arresta da solo e comunica al dispatcher l'arresto.
- Abbiamo quindi studiato l'uso degli eventi in c# e la trasmissione di eventi fra classi tramite EventHandler
- poi abbiamo iniziato a studiare la Reflection in c#
- adesso dobbiamo implementare un sistema che prenda le classi e le interfaccie tramite la reflection , ed in base ad esse, instanzia degli assembly per creare delle instanze.
- I client / server si connettono tramite Tls
