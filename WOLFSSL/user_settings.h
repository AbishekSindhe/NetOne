/* These are the build settings used by the Visual Studio CSharp wrapper */

/* Derived from:
./configure --enable-keygen --enable-eccencrypt --enable-ed25519 --enable-curve25519 --enable-aesgcm \
--disable-sha3 --disable-intelasm --disable-chacha --disable-poly1305 --disable-sys-ca-certs --disable-asn-print
*/

#ifndef _WIN_CSHARP_USER_SETTINGS_H_
#define _WIN_CSHARP_USER_SETTINGS_H_

/* Portability */
#ifndef _WIN32_WCE
#define _WIN32_WCE
#endif
#define WOLFSSL_IGNORE_FILE_WARN
#define NO_WOLFSSL_DIR
#define NO_MULTIBYTE_PRINT
#define NO_WOLFSSL_MSG_EX
#define WOLFSSL_NO_ATOMICS

#define SIZEOF_LONG_LONG 8
#define SIZEOF_LONG 4

#define NO_MAIN_DRIVER
#define BENCH_EMBEDDED

/* For testing use test seed - NEEDS FIXED */
#define WOLFSSL_GENSEED_FORTEST

#define USE_WOLF_STRTOK
#define XSNPRINTF StringCbPrintfA
#include <string.h> /* for strcpy_s */



/* Features */
#define WOLFSSL_TLS13
#define WOLFSSL_ASN_TEMPLATE
#define HAVE_HASHDRBG
#define WOLFSSL_BASE64_ENCODE
#define WOLFSSL_PUBLIC_MP

#if 0 /* optional RSA key generation */
    #define WOLFSSL_KEY_GEN
#endif

#if 1 /* debug messages - logging */
    #define DEBUG_WOLFSSL
#endif

/* TLS Extensions */
#define HAVE_TLS_EXTENSIONS
#define HAVE_SUPPORTED_CURVES
#define HAVE_SERVER_RENEGOTIATION_INFO
#define HAVE_EXTENDED_MASTER
#define HAVE_ENCRYPT_THEN_MAC
#define HAVE_SNI

/* Math - sp_int.c */
#define WOLFSSL_SP_MATH_ALL
#define WOLFSSL_SP_SMALL
#define SP_INT_BITS 2048 /* maximum key size */

/* Timing Resitance */
#define TFM_TIMING_RESISTANT
#define ECC_TIMING_RESISTANT
#define WC_RSA_BLINDING

/* Hashing */
#undef  NO_MD5
#undef  NO_SHA
#undef  NO_SHA256
#define WOLFSSL_SHA224
#define WOLFSSL_SHA512
#define WOLFSSL_SHA384

#define HAVE_HKDF /* required for TLS v1.3 */

/* Cipher */
#undef  NO_AES_CBC
#define HAVE_AESGCM
#define GCM_TABLE_4BIT /* also GCM_SMALL and GCM_TABLE */
#if 0 /* ChaCha20/Poly1305 - Optional */
    #define HAVE_CHACHA
    #define HAVE_POLY1305

    /* required for TLS */
    #define HAVE_ONE_TIME_AUTH
#endif

/* ECC */
#define HAVE_ECC
#define ECC_SHAMIR
#define ECC_MIN_KEY_SZ 224
#define HAVE_COMP_KEY
#if 0 /* optional ECIES */
    #define HAVE_ECC_ENCRYPT
#endif

/* DH */
#undef  NO_DH
#define HAVE_DH_DEFAULT_PARAMS
#define HAVE_FFDHE_2048

/* RSA */
#undef NO_RSA
#ifdef WOLFSSL_TLS13
    #define WC_RSA_PSS
    #define WOLFSSL_PSS_LONG_SALT
#endif

/* ED/Curve25519 - Optional */
#if 0
    #define HAVE_CURVE25519
    #define HAVE_ED25519
    #define CURVED_SMALL
#endif


/* Disabled features / algorithms */
#define NO_OLD_TLS
#define NO_DES3_TLS_SUITES
#define NO_DES3
#define NO_DSA
#define NO_RC4
#define NO_PSK
#define NO_MD4
#define WOLFSSL_NO_SHAKE128
#define WOLFSSL_NO_SHAKE256


#endif /* !_WIN_CSHARP_USER_SETTINGS_H_ */
