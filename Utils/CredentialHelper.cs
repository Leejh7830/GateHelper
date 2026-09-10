using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace GateHelper
{
    public static class CredentialHelper
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("GateHelperEntropy_v1");
        private const string Prefix = "DPAPI:";

        public static string Protect(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return string.Empty;
            var data = Encoding.UTF8.GetBytes(plaintext);
            try
            {
                var protectedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                return Prefix + Convert.ToBase64String(protectedData);
            }
            finally
            {
                Array.Clear(data, 0, data.Length);
            }
        }

        public static string Unprotect(string protectedBase64)
        {
            if (string.IsNullOrEmpty(protectedBase64)) return string.Empty;
            try
            {
                if (!protectedBase64.StartsWith(Prefix)) return string.Empty; // 명확한 판별
                var base64 = protectedBase64.Substring(Prefix.Length);
                var protectedData = Convert.FromBase64String(base64);
                var data = ProtectedData.Unprotect(protectedData, _entropy, DataProtectionScope.CurrentUser);
                try
                {
                    return Encoding.UTF8.GetString(data);
                }
                finally
                {
                    Array.Clear(data, 0, data.Length);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static SecureString UnprotectToSecureString(string protectedBase64)
        {
            var secure = new SecureString();
            if (string.IsNullOrEmpty(protectedBase64)) return secure;

            try
            {
                if (!protectedBase64.StartsWith(Prefix)) return secure;
                var base64 = protectedBase64.Substring(Prefix.Length);
                var protectedData = Convert.FromBase64String(base64);
                var data = ProtectedData.Unprotect(protectedData, _entropy, DataProtectionScope.CurrentUser);
                try
                {
                    var chars = Encoding.UTF8.GetChars(data);
                    foreach (var c in chars) secure.AppendChar(c);
                    secure.MakeReadOnly();
                    Array.Clear(chars, 0, chars.Length);
                }
                finally
                {
                    Array.Clear(data, 0, data.Length);
                }
            }
            catch
            {
                // 실패 시 빈 SecureString 반환
            }
            return secure;
        }
    }
}