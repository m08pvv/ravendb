using System;
using System.Runtime.InteropServices;

namespace Sparrow.Platform.Win32
{
    public static unsafe class WinSodium
    {
        private const string LIB_SODIUM = "libsodium.dll";

        static WinSodium()
        {
            var rc = sodium_init();
            if (rc != 0)
                throw new InvalidOperationException("Unable to initialize sodium, error code: " + rc);
        }

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_kdf_keybytes();

        [DllImport(LIB_SODIUM)]
        public static extern int sodium_init();

        [DllImport(LIB_SODIUM)]
        public static extern int randombytes_buf(
            byte* buffer,
            int size);

        [DllImport(LIB_SODIUM)]
        public static extern void crypto_kdf_keygen(
            byte* masterkey);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_kdf_derive_from_key(
            byte* subkey,
            int subkeylen,
            long subkeyid,
            byte* ctx,
            byte* key);


        [DllImport(LIB_SODIUM)]
        public static extern int crypto_aead_chacha20poly1305_encrypt_detached(
            byte* c,
            byte* mac,
            ulong* maclen_p,
            byte* m,
            ulong mlen,
            byte* ad,
            ulong adlen,
            byte* nsec,
            byte* npub,
            byte* k);


        [DllImport(LIB_SODIUM)]
        public static extern int crypto_aead_chacha20poly1305_decrypt_detached(
            byte* m,
            byte* nsec,
            byte* c,
            ulong clen,
            byte* mac,
            byte* ad,
            ulong adlen,
            byte* npub,
            byte* k);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_seal(byte* b, byte* b1, ulong mlen, byte* pk);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_seal_open(byte* m, byte* c, ulong clen, byte* pk, byte* sk);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_sealbytes();

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_generichash(byte* @out, IntPtr outlen, byte* @in,
                ulong inlen, byte* key, IntPtr keylen);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_generichash_bytes_max();

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_keypair(byte* pk, byte* sk);

        [DllImport(LIB_SODIUM)]
        public static extern int sodium_memcmp(byte* b1, byte* b2, IntPtr len);

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_secretkeybytes();

        [DllImport(LIB_SODIUM)]
        public static extern int crypto_box_publickeybytes();
    }
}