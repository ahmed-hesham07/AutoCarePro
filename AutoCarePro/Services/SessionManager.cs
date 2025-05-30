using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// SessionManager class handles user session management in the AutoCarePro application.
    /// It provides functionality to save, load, and delete user sessions with encryption.
    /// </summary>
    public class SessionManager
    {
        // File path where the session data will be stored
        private static readonly string SessionFile = "session.json";
        // Encryption key for securing session data (Note: In production, use a secure key management system)
        private static readonly string EncryptionKey = "AutoCarePro2024";

        /// <summary>
        /// SessionData class represents the data stored in a user session
        /// </summary>
        public class SessionData
        {
            // ID of the logged-in user
            public int UserId { get; set; }
            // Username of the logged-in user
            public string Username { get; set; }
            // When the session expires
            public DateTime ExpiryDate { get; set; }
            // Whether the user chose to be remembered
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Saves a user session to disk with encryption
        /// </summary>
        /// <param name="user">The user to create a session for</param>
        /// <param name="rememberMe">Whether to remember the user for a longer period</param>
        public static void SaveSession(User user, bool rememberMe)
        {
            // Create session data with appropriate expiry time
            var sessionData = new SessionData
            {
                UserId = user.Id,
                Username = user.Username,
                ExpiryDate = rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(24),
                RememberMe = rememberMe
            };

            // Serialize and encrypt the session data
            var json = JsonSerializer.Serialize(sessionData);
            var encryptedData = EncryptString(json);
            File.WriteAllText(SessionFile, encryptedData);
        }

        /// <summary>
        /// Loads and decrypts the user session from disk
        /// </summary>
        /// <returns>The session data if valid, null otherwise</returns>
        public static SessionData LoadSession()
        {
            try
            {
                // Check if session file exists
                if (!File.Exists(SessionFile))
                    return null;

                // Read and decrypt the session data
                var encryptedData = File.ReadAllText(SessionFile);
                var json = DecryptString(encryptedData);
                var sessionData = JsonSerializer.Deserialize<SessionData>(json);

                // Check if session has expired
                if (sessionData.ExpiryDate < DateTime.Now)
                {
                    DeleteSession();
                    return null;
                }

                return sessionData;
            }
            catch
            {
                // If any error occurs, delete the session and return null
                DeleteSession();
                return null;
            }
        }

        /// <summary>
        /// Deletes the current session file
        /// </summary>
        public static void DeleteSession()
        {
            if (File.Exists(SessionFile))
                File.Delete(SessionFile);
        }

        /// <summary>
        /// Encrypts a string using AES encryption
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <returns>The encrypted text as a base64 string</returns>
        private static string EncryptString(string text)
        {
            using (var aes = Aes.Create())
            {
                // Set up encryption key and initialization vector
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                // Create encryption stream
                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted string using AES decryption
        /// </summary>
        /// <param name="cipherText">The encrypted text as a base64 string</param>
        /// <returns>The decrypted text</returns>
        private static string DecryptString(string cipherText)
        {
            using (var aes = Aes.Create())
            {
                // Set up decryption key and initialization vector
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                // Create decryption stream
                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
} 