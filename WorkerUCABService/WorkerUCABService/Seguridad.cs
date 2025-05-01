using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WorkerUCABService.Procesos;
using System.IO;
using System;

namespace WorkerUCABService
{
    internal class Seguridad
    {
        #region Variables
        public string codNonce { get; set; }
        public Seguridad()
        {
            codNonce = ""; // o cualquier otro valor predeterminado que desees
        }
        static Logs l = new Logs();
        #endregion


        #region Variables Globales
       
        private static readonly string SecretIv = "SecretIv"; 
        private static readonly string SecretKey = "SecretKey"; 
        private static readonly int tamanoClave = 128; 
        private static readonly string algo = "algo"; 
        private static readonly int iteraciones = 22;

        #endregion


        #region Descrifrado de Datos

        public static string descifrarTextoAES(string textoCifrado, string vectorInicial)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vectorInicial);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(SecretIv);
                byte[] cipherTextBytes = Convert.FromBase64String(textoCifrado);

                using (var password = new Rfc2898DeriveBytes(SecretKey, saltValueBytes, iteraciones))
                {
                    byte[] keyBytes = password.GetBytes(tamanoClave / 8);

                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = keyBytes;
                        aesAlg.IV = InitialVectorBytes;
                        aesAlg.Mode = CipherMode.CBC;
                        aesAlg.Padding = PaddingMode.PKCS7;

                        using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                        {
                            using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                            {
                                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                                    {
                                        // Leemos el texto descifrado y lo devolvemos
                                        return srDecrypt.ReadToEnd();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo del error, ajusta según sea necesario
                Console.WriteLine($"Error en {nameof(descifrarTextoAES)}: {ex.Message}");
                return string.Empty;
            }
        }


        #endregion

        #region Cifrado de Datos

        public string cifrarTextoAES(string textoCifrar, string vectorInicial)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vectorInicial);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(SecretIv);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(textoCifrar);

                using (var password = new Rfc2898DeriveBytes(SecretKey, saltValueBytes, iteraciones))
                {
                    byte[] keyBytes = password.GetBytes(tamanoClave / 8);

                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = keyBytes;
                        aesAlg.IV = InitialVectorBytes;
                        aesAlg.Mode = CipherMode.CBC;
                        aesAlg.Padding = PaddingMode.PKCS7;

                        using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                        {
                            using (MemoryStream msEncrypt = new MemoryStream())
                            {
                                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                                {
                                    csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    csEncrypt.FlushFinalBlock();

                                    byte[] cipherTextBytes = msEncrypt.ToArray();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo del error, ajusta según sea necesario
                Console.WriteLine($"Error en {nameof(cifrarTextoAES)}: {ex.Message}");
                return string.Empty;
            }
        }


        #endregion

        #region Genera Salt

        public byte[] CreateSalt(int size)
        {
            byte[] buff = new byte[size];
            RandomNumberGenerator.Fill(buff);
            return buff;
        }

        #endregion

        #region Modelos de Seguridad

        public static Tuple<byte[], string> pass(string Password)
        {
            var password_bytes = Encoding.ASCII.GetBytes(Password);
            byte[] data_imput = new byte[password_bytes.Length];

            for (int i = 0; i < password_bytes.Length; i++)
            {
                data_imput[i] = password_bytes[i];
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                var hashed_byte_array = sha512.ComputeHash(data_imput);
                string hash_result = Convert.ToBase64String(hashed_byte_array);

                return new Tuple<byte[], string>(new byte[20], hash_result);
            }
        }

        public void GeneraNonce()
        {
            try
            {
                //Generación de NONCE a partir de Datetime
                TimeSpan timspa = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                long timeresult = (long)timspa.TotalMilliseconds;
                string nuevacadena = timeresult.ToString();
                codNonce = nuevacadena;
            }
            catch (Exception ex)
            { l.ErrorLog(this, ex); }
        }

        #endregion

    }
}
