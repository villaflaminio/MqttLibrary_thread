# MqttLibrary_thread
Docuemntazione : https://www.notion.so/NET-8ca20b34bd51410fa2f0233858178d64
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
