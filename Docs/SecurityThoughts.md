# Security Concerns and Best Practices

## Security Concerns 
In the proccess of developing PassCrypt, several security concerns crossed my mind. The main concern is the security of the password storage and encryption methods used in the application. It is crucial to ensure that passwords are stored securely and that the encryption methods used are robust enough to prevent unauthorized access.

## TechStack
### Why AES-256-GCM?
1. **Unbreakable by brute force**
  256-bit key = more combinations than atoms in the observable universe.
  (Guessing isn't a viable attack vector)
2. **Detects tampering** 
  GCM produces an authentication tag alongside the ciphertext. 
  If even one bit of the encrypted vault is altered, decryption fails outright - 
  It doesn't silently hand back corrupted or wrong data. 
3. **One alogrithm does both jobs** 
  Normal AES only hides data. GCM adds the tampering check on top, in the same
  operation. You don't need to attach a second, seperate mechanism to 
  get integrity - fewer moving parts, fewer ways to wire it wrong.
4. **Fast enough to not matter**
  Even tho its safe against brute force, it is still fast enough to encrypt 
  and decrypt instantly, aslong as you have the neccesary pre-requisites.

### Why Argon2id?
1. **Turns a weak password into a strong key**
  A traditional weak password will not be a weak link. Argon2id stretches it
  thorugh a delibereatly expensive process to derive a proper encryption key.
2. **Memory-Hard: GPUs and ASICs can't cheat**
  Most brute-force comes from running billions of guesses on GPUs in parallel, 
  on cheap hardware. Argon2id forces each guess to use a large chunk of memory,
  not just CPU cycles.
3. **"id" = hybrid defence**
Argon has 3 models: (i,d,id). The "id" is a hybrid of i and d.
- **i** = safe against side-channel attacks (safe against people
  who can observe cache/memory access patterns)
- **d** = safe against GPU attacks 
- **id** = a hybrid of both, which is the best of both worlds.

  **Tunable Parameters**
The good thing about Argon2id is that it has tunable parameters. What this 
means for us is that we can adjust the security of out key derivation
function instead of switching when hardware inevitably get faster. The parameters are:

- **Memory**: How much memory to use for the key derivation. More memory means more security, but also more resource usage.
- **Iterations**: How many times to run the key derivation function. More iterations means more security, but also more time taken to derive the key.
- **Parallelism**: How many threads to use for the key derivation. More threads means more security, but also more resource usage.

## In-memory handling
- `Array.Cleat()` on master password bytes after derivation.
- Session key lifecycle `VaultKeySession.Clear()` on lock.
- **Limitation** .NET GC/string immutability means full memory srubbing isn't
  guranteed.

## Clipboard exposure
- A times out-clear (12 seconds) + blur/pagehide handling
- Clipboard is a shared resource, so other apps can read it. This is
  a limitation of the OS and cannot be fully mitigated by the application.
