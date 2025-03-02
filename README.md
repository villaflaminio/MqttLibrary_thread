# MqttLibrary_thread
- implementazione di Publisher , Subscriber , Broker MQTT 
- implementazione di Entity framework con mysql lite
- implementazione di multithreading per gestire i messaggi piu' velocemente
- implementazione di un "Dispatcher", che pendeva i messaggi dal subscriber e li elabborasse con dei thread (tipo se fossero dei worker)
- Il dispatcher misura il numero di richieste al secondo in arrivo ed istanzia un thread ogni fattore di 10 richieste al secondo (20req/s = 2th , 60 req/s = 6 th, etc...). fino a raggiungere il massimo numero di thread (che dobbiamo definire noi ).
- chiaramente quando sono disponibili piu' thread di quelli necessari , il dispatcher li arresta.
- dopo aver instanziato i thread il dispatcher li salva in una sua mappa e successivamente "distribuisce" un messaggio da elaborare a ciascun worker.
- Se il worker non ha messaggi da elaborare per "X" secondi , si arresta da solo e comunica al dispatcher l'arresto.
- Abbiamo quindi studiato l'uso degli eventi in c# e la trasmissione di eventi fra classi tramite EventHandler
- poi abbiamo iniziato a studiare la Reflection in c#
- adesso dobbiamo implementare un sistema che prenda le classi e le interfaccie tramite la reflection , ed in base ad esse, instanzia degli assembly per creare delle instanze.
- I client / server si connettono tramite Tls

# MQTT

MQTT è un protocollo ISO standard di messaggistica leggero di tipo publish-subscribe posizionato in cima a TCP/IP. È stato progettato per le situazioni in cui è richiesto un basso impatto e dove la banda è limitata. Il pattern publish-subscribe richiede un broker di messaggistica.

### Come connettersi ad un broker MQTT?

Importare i seguenti pacchetti da NuGet e scrivere le successive righe di codice.

![Untitled](MQTT%205b549/Untitled.png)

![Untitled](MQTT%205b549/Untitled%201.png)

### Cos’è e come si definisce un publisher?

Il publisher ci permette di pubblicare messaggi tramite il broker. La pubblicazione avviene su un topic, che dovrà essere condiviso anche al subscriber per consentire la trasmissione di messaggi. 

![Untitled](MQTT%205b549/Untitled%202.png)

Nell’esempio di sopra stiamo pubblicando il messaggio **text + i** sul topic **Data/**.

### Cos’è e come si definisce un subscriber?

Il subscriber ci consente di poter leggere i messaggi pubblicati dal publisher, si “iscrive” ad un topic e rimane in ascolto in attesa di ricevere nuovi messaggi da un publisher.

![Untitled](MQTT%205b549/Untitled%203.png)

Di sopra vediamo come creare ed istanziare un subscriber che si iscrive al topic **Data/**. Per processare i messaggi ricevuti utilizziamo il metodo **client_receivedMessage**, che non appena viene pubblicato un messaggio sul topic a cui il subscriber è iscritto, intercetta il messaggio e ci da’ la possibilità di gestirlo. Un esempio: 

![Untitled](MQTT%205b549/Untitled%204.png)

Nell’esempio, letto il messaggio (**string**), lo processiamo creando un oggetto di tipo **Message**, che possiamo poi elaborare in base alle nostra necessità (inserimento in DB, query o analisi).

### Cos’è e come si definisce un dispatcher?

Un dispatcher viene creato per gestire molteplici richieste (messaggi) in questo caso nel modo che più ci fa comodo. Nell’esempio, il dispatcher, veniva istanziato (singleton) nel momento in cui il subscriber riceveva il primo messaggio. Successivamente, in base al numero di messaggi ricevuti dal subscriber e quindi da processare, il dispatcher creava N thread (worker), che elaboravano i dati e li processavano (nel nostro caso li inserivano banalmente in un DB. 

Esempio: 

![Untitled](MQTT%205b549/Untitled%205.png)

In questa prima parte abbiamo la definizione delle proprietà del dispatcher. Le più importanti che ci serviranno sono:

- messageQueue → coda che contiene i messaggi ricevuti dal thread del subscriber;
- mapThread → dizionario che associa un id ad un Worker in modo da mappare tutti i thread che stanno funzionando;
- listLock → un oggetto utilizzato per effettuare il lock della risorsa condivisa (messageQueue);
- Dispatcher → nuovo thread di tipo DispatcherThread (definito nei prossimi passaggi).

```csharp
// Private constructor to implement singleton.
private Dispatcher()
{
    calls = 0;
    sw = new Stopwatch();
    sw.Start();
    tDispatcher.Start();
    Console.WriteLine("Dispatcher has been successfully created.");
}

// Method GetInstance to implement singleton pattern.
public static Dispatcher GetInstance(int nThread)
{
    // Set the max number of thread equals to the passed nThread.
    nThreadMax = nThread;

    // If null, create new instance, else return the already created instance.
    if (_instance == null)
    {
        _instance = new Dispatcher();
    }
    return _instance;
}
```

```csharp
// Method to retrieve a request from the MqttServer.
public void AddRequest(Message message)
{
    // Method that measures the number of request/second.
    Measure();

    // To add a new element, lock the list. 
    // The list is the shared resource, so I must guarantee mutually exclusion access.
    lock (listLock)
    {
        // Add element into queue.
        messageQueue.Enqueue(message);

        // Notify that there are new elements into the queue,
        // then unlock the queue.
        Monitor.Pulse(listLock); 
    }

    // Process the request.
    ProcessRequest();
}
```

```csharp
// Dispatcher thread, 
static void DispatcherThread()
{
    while (true)
    {
        // Check if there are thread into mapThread dictionary.
        if (mapThread.Count > 0)
        {   
            // Lock the dictionary mapThread resource, to guarantee mutally exclusive access.
            lock (mapThread)
            {
                // Cycle each thread into the dictionary.
                for (int x = 0; x < mapThread.Count; x++)
                {
                    // The list is a shared resource, so I must guarantee mutually exclusion access.
                    lock (listLock)
                    {
                        // If the queue is empty, wait until I'll get a new element.
                        if (messageQueue.Count == 0)
                        {
                            // Release listLock resource, obtaining it again only after a Pulse call.
                            // Monitor.Wait(listLock);
                            break;
                        }

                        // Get the first element of the queue and process it.
                        Message messageDeque = messageQueue.Dequeue();

                        // Check if the thread dictionary contains the key at x position.
                        if (mapThread.ContainsKey(mapThread.ElementAt(x).Key))
                        {
                            try
                            {
                                // Try to add data to the x-th thread into mapThread.
                                mapThread.ElementAt(x).Value.AddData(messageDeque);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
```

```csharp
// Method that instances a thread.
private static void InstanceThread()
{
    // Create new Worker object.
    Worker w = new Worker();

    // Start the worker thread.
    Thread t = new Thread(w.Start);

    // Lock share resource mapThread.
    lock (mapThread)
    {
        try
        {   
            // Try add the new thread into mapThread dictionary.
            // Increment thread counter.
            mapThread.Add(mapThread.Count + 1, w);
        }catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    t.Start();
}
```

```csharp
// Method that process a request.
private static void ProcessRequest()
{
    // Lock shared resource mapThread.
    lock (mapThread)
    {
        // If there are threads into mapThread dictionary evaluate how many they are.
        if (mapThread.Count > 0)
        {
            // The number of thread into the list must be less or equal the numberOfRequestPerSec/10.
            if (requestPerSecond / 10 > mapThread.Count){
                // If the number of thread into mapThread is less than the maxThread I could create, 
                // then create thread until mapThread.Count == nThreadMax.
                if (mapThread.Count <= nThreadMax)
                {
                    // Instance new thread and add it into mapThread dictionary.
                    InstanceThread();
                }
            }
            else
            {
                /* If there more threads than numberRequestPerSec, eliminate not necessary threads,
                if (mapThread.Count > 1 && requestPerSecond / 10 != mapThread.Count)
                {
                    // Worker w;

                    if (mapThread.Remove(mapThread.Count, w))
                    {
                        w.Stop();

                        // Notify that there are new elements into the queue.
                        // Unlock the queue.
                        Monitor.Pulse(mapThread); 
                    }
                }*/

            }

        }else {
            // Start the first thread, because I have no one into the dictionary.
            InstanceThread();
        }
        Console.WriteLine("request_per_second = " + requestPerSecond + " thread_live = " + mapThread.Count + " message_to_be_processed " + messageQueue.Count);
    }
}
```

```csharp
// Set the max number of threads.
public void SetNThreadMax(int n)
{
    nThreadMax = n;
}

// Measure the number of request/second.
private void Measure()
{
    // Increment number of calls. 
    // This method is called every time AddRequest is called.
    calls++;

    // If it is elapsed 1000 milliseconds => it is elapsed 1 second, so 
    // update requestPerSecond = calls and restart StopWatch.
    if (sw.ElapsedMilliseconds > 1000)
    {
        sw.Stop();
        requestPerSecond = calls;
        calls = 0;
        sw.Restart();

    }
}
```

### Cos’è e come si definisce un worker?

Il worker consiste in un thread che gestisce un determinato numero di richieste e viene creato dal dispatcher. In questo modo è possibile creare un sistema di Load balancing che gestisce il carico in modo “automatico” (non appena si supera la soglia X di richieste/sec → creo nuovi Worker).

```csharp
public void WorkerThread()
{
    while (isRunning)
    {
        try
        {
            // Try to lock the resource messageQueue.
            lock (listLock)
            {
                // If the queue is empty, wait until new element is received.
                while (messageQueue.Count == 0)
                {
                    // Release listLock resource, obtaining it again only after a Pulse call.
                    Monitor.Wait(listLock);
                }

                // Get the first element of the queue and process it.
                Message messageDeque = messageQueue.Dequeue();

                // Printing console log.
                Console.WriteLine("Saving -> " + messageDeque.ToString());

                // Flush changes.
                repo.SaveMessage(messageDeque);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}
```

### Come gestire autenticazione a Broker MQTT (Mosquitto)?

Per gestire l’autenticazione al Broker MQTT, è necessario configurare l’utilizzo della password per effettuare l’accesso. Per farlo eseguiamo da riga di comando con i privilegi di amministratore:

```bash
mosquitto_passwd -c <password file_name> <username>

# Esempio di creazione di un file con nome "password_file" e dell'utente "nome_cognome".
mosquitto_passwd -c password_file nome_cognome 
```

Una volta eseguito il comando ci verrà richiesto di inserire la password da associare al nostro account “nome_cognome”. 

Creato il file, dobbiamo specificare al Broker di utilizzarlo, per farlo eseguiamo il comando: 

```bash
password_file <path to the configuration file>
```

Il file deve essere leggibile da qualunque utente stia runnando Mosquitto. In sistemi Linux/POSIX sarà tipicamente l’utente **mosquitto**, e **/etc/mosquitto/password_file** potrebbe essere un buon percorso per salvare il file della password. 

Per rendere effettive le modifiche fatte al file della password, eseguire il comando.

```bash
kill -HUP <process id of mosquitto>
```

Successivamente riavviare il broker Mosquitto.

## MQTT 5.0 vs MQTT 3.1.1

![Untitled](MQTT%205b549/Untitled%206.png)

### 1. Communication features

- **Enhanced authentication**, which can be implemented via Authentication Method and Authentication Data properties inside payload.
- **Session expiration**, which can be implemented with the Session Expiry Interval property. For instance, the Topic could include the subscription time, the amount of time the message will be stored. **(TODO)**
- **Client and Server restrictions**. Limitations can be built-in, including maximum packet size (number of bytes to transmit) and maximum to receive (number of messages to be sent simultaneously for a client or server).
- **Will Delay Interval**, a property to send messages on a delay.
- **Server Reference or Server Redirect**, properties that can help transfer packets to different brokers or servers.

### 2. Posting features

- **Message Expiry Interval**, the time to keep messages stored.
- **Payload format indicator and content type.** This attribute defines the type of payload flags that can be used: bytes (binary), UTF-8, or MIME types.
- **Topic Aliases.** For instance, the topic topic/v1/device/ has alias 1. This feature can minimize the number of data packages needed.
- **Response Topics.** With the use of Response Topics, the MQTT protocol can work similarly to an HTTP protocol with a response-request scheme.

### **3. Subscription features**

- **Non-local publishing.** Users can optionally opt out of receiving messages that the client published.
- **Retained message control.** This parameter controls message sorting.
- **Subscription identifier**, which is used for server identification of the subscription.
- **Shared subscriptions** to enable more flexible subscriptions with additional symbols and filtering features.

### **4. General features**

- **Reason codes on all ACK messages.** Errors can occur at any stage. In MQTTv3.1.1, not much guidance from the server was available as to what went wrong at different stages, such as establishing communication, posting a message, or subscription to topics.
- **Server disconnections.** Unlike MQTT 3.1, packets regarding disconnections can be delivered from both client and server sides in MQTT 5.0.
- **User properties.** Keys can be used as value storage for different properties.

![Example of an MQTT 5.0 System scheme.](MQTT%205b549/Untitled%207.png)

Example of an MQTT 5.0 System scheme.

## MQTT 5 - Session & Message Expiry Intervals

Utilizzando MQTT 3.1.1 l’unico modo per rimuovere le **persistent sessions** fornite dalle specifiche era quello di connettere un **MQTT Client** con lo stesso *clientId* della sessione che si voleva rimuovere, con la property **cleanSession = true.** Tuttavia in scenari di dispositivi IoT che non si riconnettono più, la mancata eliminazione delle sessione creerebbe ulteriore carico al broker. 

MQTT 5.0 permette di gestire più facilmente questo problema, utilizzando il parametro **session expiry interval**, che ci permette di settare un tempo limite prima che il broker elimini automaticamente le sessioni rimaste inattive per il tempo limite impostato.

![Flusso della sessione per MQTT 3.1.1 v MQTT 5.0.](MQTT%205b549/Untitled%208.png)

Flusso della sessione per MQTT 3.1.1 v MQTT 5.0.

Così come per la gestione della sessione, è stato pensato un campo per gestire il tempo in cui i messaggi vengono salvati in caso di mancata ricezione. Settando correttamente la proprietà **message expiry interval**, possiamo specificare al broker quanto tempo tenere in memoria messaggi non ricevuti da eventuali client. Molti dispositivi IoT, come le automobili, sono sviluppati gestendo il problema della possibilità di una mancata connessione per lunghi periodi di tempo. Per queste casistiche MQTT fornisce **persistent sessions and message queueing**. I messaggi che devono essere recapitati a dispositivi che potenzialmente possono essere offline, vengono immagazzinati dal broker ed inviati quando viene stabilita nuovamente la connessione. Non tutti i messaggi avranno la stessa priorità ed alcuni potrebbero non servire più una volta trascorso un certo lasso di tempo, quindi è molto importante settare correttamente questo parametro, in modo tale che quando viene stabilita nuovamente la connessione il client non venga sommerso da messaggi che non sono più utili. **Quando la sessione per un client scade, tutti i messaggi inseriti nella coda vengono persi, indipendentemente dal parametro settato.**

```bash
# MQTT 3.x.x
cleanSession=true 

# MQTT 5.x.x
sessionExpiry=0 # Could be absent, by default take this configuration
cleanStart=true

# -----------

# MQTT 3.x.x
cleanSession=false

# MQTT 5.x.x
sessionExpiry=k # k > 0
```


# MQTT Authentication (SSL/TLS)

## Cosa sono SSL e TLS?

**SSL** vuol dire "Secure Sockets Layer" (Livello di socket sicuri), una tecnologia standard che garantisce la sicurezza di una connessione a Internet e protegge i dati sensibili scambiati fra due sistemi impedendo ai criminali informatici di leggere e modificare le informazioni trasferite, che potrebbero comprendere anche dati personali. La comunicazione fra sistemi può riguardare un server o client (ad es. un sito Web di e-commerce e un browser) o due server (ad es. un'applicazione basata su informazioni personalmente identificabili o dati sul libro paga).
 
In questo modo è possibile impedire la lettura e l'intercettazione di qualsiasi dato trasferito fra utenti e siti o due sistemi. È possibile utilizzare algoritmi di crittografia per crittografare i dati in transito, impedendone la lettura agli hacker durante il transito su una connessione digitale. Queste informazioni possono essere di natura sensibile o personale, come ad esempio numeri di carta di credito, altre informazioni finanziarie, nomi e indirizzi.
 
**TLS** (Transport Layer Security, sicurezza del livello di trasporto) è una versione aggiornata e più sicura di SSL. Indichiamo i nostri certificati di sicurezza con la dicitura SSL poiché si tratta di un termine di utilizzo più frequente. Tuttavia, al momento di [acquistare SSL](https://www.websecurity.digicert.com/it/it/ssl-certificate?inid=infoctr_buylink_sslhome) da DigiCert, i clienti otterranno i certificati TLS più aggiornati con [crittografia ECC, RSA o DSA](https://www.websecurity.digicert.com/it/it/security-topics/how-ssl-works).

## Come implementare autenticazione SSL per broker MQTT?

Il Broker mqtt richiede username e password per autenticare publisher e subscriber. Nel caso in cui vogliamo registrare anche client anonimi occorre aggiungere nel file di configurazione del broker :

```csharp
allow_anonymous true
```

## Step 1 - Creazione dei certificati

- **CA**

Per creare i certificati TLS occorre prima di tutto creare un certificato che ci permette di “firmare” i certificati che verranno generati in seguito :

```bash
openssl req -newkey rsa:4096 -x509 -nodes -sha256 -days 365 
-extensions v3_ca -keyout CACERT.key -out CACERT.crt -subj /C=IT/ST=Rome/L=Rome/O=Elis/OU=CACERT/CN=flaminioHost
```

IMPORTANTE = CN (e’ il nome host) e deve essere diverso dal CN che verra’ usato per gli altri certificati

con -subj inserisco direttamente tutti i campi del certificato. Possono essere modificati senza vincoli particolari.

Per verificare che il certificato sia stato creato correttamente:

```bash
openssl x509 -in CACERT.crt -nameopt multiline -subject -noout
```

- **Broker**

Successivamente occorre generare la chiave per il broker mqtt:

```bash
openssl genrsa -out broker.key 4096

openssl req -new -sha256 -out broker.csr -key broker.key -subj /C=IT/ST=Rome/L=Rome/O=Elis/OU=broker/CN=localhost

openssl x509 -req -sha256 -in broker.csr -CA CACERT.crt -CAkey CACERT.key -CAcreateserial -CAserial ca.srl -out broker.crt -days 365 -extensions JPMextensions
```

Per verificare che il certificato sia stato creato correttamente:

```bash
openssl x509 -in broker.crt -nameopt multiline -subject -noout
```

- **Client**

Per generare le chiavi per il client il procedimento e’ simile a quanto visto sopra:

```bash
openssl genrsa -out CLIENT.key 4096

openssl req -new -sha256 -out CLIENT.csr -key CLIENT.key -subj /C=IT/ST=Rome/L=Rome/O=Elis/OU=CLIENT/CN=localhost

openssl x509 -req -sha256 -in CLIENT.csr -CA CACERT.crt -CAkey CACERT.key -CAcreateserial -CAserial ca.srl -out CLIENT.crt -days 365 -extensions JPMextensions

```

Per verificare che il certificato sia stato creato correttamente:

```bash
openssl x509 -in CLIENT.crt -nameopt multiline -subject -noout
```

- **Per utilizzare i certificati con MQTTnet**

l certificato CA è in formato *.crt, il certificato client deve essere in *.pfx e deve avere la password utilizzata per esportare il file dalla chiave privata e dal certificato originariamente. Il file *.pfx può essere creato usando openssl come di seguito:

```bash
openssl pkcs12 -export -out certificateClient.pfx -inkey CLIENT.key -in CLIENT.crt
```

## Step 2 - Creazione del file di configurazione di mosquitto

Prima di tutto occorre scegliere la cartella in cui salvare il file di configurazione di mosquitto Server e le sue password (io ho utilizzato la cartella d’installazione di mosquitto).

- **creazione del file di password**

Per creare un file di password, utilizzare l' `mosquitto_passwd`utilità, utilizzare la riga seguente. Ti verrà richiesta la password. Nota che `-c`significa che un file esistente verrà sovrascritto:

Per aggiungere più utenti a un file di password esistente o per modificare la password di un utente esistente, ometti l' `-c`argomento:

```bash
mosquitto_passwd -c <password file> <username>
```

Per rimuovere un utente da un file di password:

```bash
mosquitto_passwd -D <password file> <username>
```

Puoi anche aggiungere/aggiornare un nome utente e una password in una singola riga, ma tieni presente che ciò significa che la password è visibile sulla riga di comando e in qualsiasi cronologia dei comandi:

```bash
mosquitto_passwd <password file> <username> <password>
```

- File di configurazione del Broker

Creare un file <nome_file>.conf e aggiungere i seguenti campi: (Sostituire i campi con i path reali )

```bash
listener  8883
password_file <password file>
cafile <../CACERT.crt> // sostituire il path
certfile <..broker.crt>
keyfile <../broker.key>
require_certificate true
```

- Avvio di mosquitto con il file di configurazione custom

```bash
mosquitto -v -c ../<nome_file>.conf
```

## Step 3 - Test funzionamento dei certificati

- mosquitto -v -c "C:\Program Files"\mosquitto\mosquittoSecurity.conf”

Modificare i path dei certificati, username ( -u) e password (-p) con quelle create nello step2

```bash
mosquitto_pub -p 8883 --cafile "C:\certs\CACERT.crt" --cert "C:\certs\CLIENT.crt" --key "C:\certs\CLIENT.key" -u root -P flaminio --tls-version tlsv1.2 -h localhost -m Ciao -t /world
```

```bash
mosquitto_sub -p 8883 --cafile "C:\certs\CACERT.crt" --cert "C:\certs\CLIENT.crt" --key "C:\certs\CLIENT.key" -u root -P flaminio --tls-version tlsv1.2 -h localhost -t /world
```

Se tutto ha funzionato correttamente, il broker ed il client si sono connessi al server, il broker ha inviato un messaggio sul topic World con scritto “Ciao” 

## Step 4 - Utilizzo dei certificati con C#

Sto utilizzando la libreria : using MQTTnet.Client;

per riferimenti : [https://github-wiki-see.page/m/chkr1011/MQTTnet/wiki/Client](https://github-wiki-see.page/m/chkr1011/MQTTnet/wiki/Client)

```csharp
MqttFactory mqttFactory = new MqttFactory();

X509Certificate caCert = X509Certificate.CreateFromCertFile(@"C:\certs\CACERT.crt");
X509Certificate clientCert = new X509Certificate2(@"C:\certs\certificateClient.pfx", "flaminio");

IMqttClient client = mqttFactory.CreateMqttClient();
var tlsOptions = new MqttClientOptionsBuilderTlsParameters
{
    UseTls = true,
    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
    Certificates = new List<X509Certificate>
            {
                clientCert, caCert

            },
    AllowUntrustedCertificates = true,
    IgnoreCertificateChainErrors = true,
    IgnoreCertificateRevocationErrors = true
};

var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer("localhost", 8883)
                .WithCredentials("root", "flaminio")
                .WithTls(tlsOptions)
                .WithCleanSession()
                .Build();
```
