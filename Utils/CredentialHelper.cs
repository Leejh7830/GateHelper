using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace GateHelper
{
    public static class CredentialHelper
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("GateHelperEntropy_v1");

        public static string Protect(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return string.Empty;
            var data = Encoding.UTF8.GetBytes(plaintext);
            try
            {
                var protectedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedData);
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
                var protectedData = Convert.FromBase64String(protectedBase64);
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

        // 메모리상 평문 노출 시간을 줄이기 위한 SecureString 반환
        public static SecureString UnprotectToSecureString(string protectedBase64)
        {
            var secure = new SecureString();
            if (string.IsNullOrEmpty(protectedBase64)) return secure;

            try
            {
                var protectedData = Convert.FromBase64String(protectedBase64);
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
                // 실패하면 빈 SecureString 반환
            }
            return secure;
        }
    }
}