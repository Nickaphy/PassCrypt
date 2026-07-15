# Argon2ID
Argon2ID is password hashing algorithm, it turns a password into a fixed size key, in a way that is slow and expensive to bruteforce.
- There are 3 different forms.
    - Argon2d (good against GPU attacks)
    - Argon2i (side channel attacks)
    - Argon2id (Both hybrid)
This is used in out KeyDerivation.cs.
Argon uses salt which is random bytes from salt.bin which is created for the user upon first password hash, it ensures the same password doesnt produce the same key for different users.
- Itterations / Memory / parrallelism = (more = slower for attacker and for you)
**Summary:**
Argon2id is industry standard for deriving a cryptographic key from a password safely.
Same password + same salt = same key everytime.


# AESS-256-GCM
Framework 'System.Security.Cryptography'
**AES** advanced encryption standard (symmetric encryption)
**256** Key size (32 bytes from Argon2id)
**GCM** Galois/countermode (encrypts and authenticates data)

- Plain AES only hides data. GCM also detects tampering.
    When you decrypt, GCM checks the tag. If someone changed the file or the password is wrong (wrong key), decryption fails with CryptographicException.
    That's how your app knows "wrong password" without storing the password anywhere.

## Vault.bin
**nonce** Random 12 byte value per encryption (same plaintext must not encrypt to the same ciphertext twice)
**tag** Authentication proof (detects wrong key or tampered data)
**cipherText** The encrypted bytes