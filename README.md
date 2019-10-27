# CanonicalizeRequest

A basic library to make using MAC easier and more standard. Want to secure
communication between two services? Use an HMAC with symmetric keys! Don't want to
be responsible for your customer's private keys? Use an RSA-MAC and never touch another
customer private key again!

## Background

MAC stands for Message Authentication Code. Basically, when using a MAC security scheme,
HTTP senders pass along a special code that can be used by the reciever to verify that
the sender is in the possession of a particular key. That's basically it! The sender has
a secret (think of it like a really super complex password). When they pass along the MAC,
the recipient can inspect that MAC, and validate that the message indeed came from someone
with that secret. In the case of symmetric key encryption, both parties share the secret key.
In the case of asymmetric key encryption (AKA private key encryption), the sender has
the private key, but the recipient only has a public key. In asymmetric encryption, the MAC
can only be made using the private key, but can be verified using the public key. Due to the
additional complexity, asymmetric key encryption can be slower - a price to pay to allow
only the sender to have the secret.

## Methodology

MAC authentication involves a set of steps:

1. (Sender) Create a string-to-sign including: 
  a. A canonical representation of the request, hashed using MD5.
  b. Identitify information about the sender (e.g. the sender's unique identifier).
  c. Timestamp of the request.
2. (Sender) Create a signature using the string-to-sign and the encryption key.
3. (Sender) Add headers to the request and send the request.
4. (Recipient) Create a string-to-sign from the request (exactly like step 1).
5. (Recipient) Verify the signature of the string-to-sign, either by:
  a. Signing the string-to-sign with the shared symmetric key (and comparing the result with the sent value).
  b. Verifying the string-to-sign with the asymmetric public key.

Both the sender and the recipient need to use the exact same steps to create the
canonical representation of the request. If a single character is out of place,
authentication will fail. This makes the authentication specific to an individual
request. Attackers can't swipe an authentication header from one request and use
it in a separate malicient request.

The recipient is configured to only accept requests whose timestamps are within
some acceptable clock drift. This makes the authentication specific to a
particular time range. If an attacker got their hands on an entire HTTP request,
they would only be able to send that exact request for the duration of the
clock drift tolerance (e.g. 5 mins).

## Advantages over JSON Web Tokens

JWTs are fantastic. There's a reason why they're a standard way of handling
authentication. But they have their drawbacks. Namely:

* If an attacker obtains a JWT while it is in-flight, they can use it
to perform any action the user is authorized to perform.
* Often JWTs are relatively long-lived, e.g. days, weeks, or even months.

Conversely, MACs:

* Are never in flight (except perhaps on the first and only time the keys are distributed).
* Authentication headers are different for each request.

MACs, on the other hand, add a layer of complexity that is not present in JWTs.
JWTs can be held securily in browser local storage and used for repeated web
requests without performing complex operations.
