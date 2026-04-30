using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public static class CredentialStore
    {
        private const string Target = "EMS_PJT_Hamburger/API";

        public static void Save(string username, string token)
        {
            var cred = new Credential
            {
                Target = Target,                // 키 이름(저장 식별자)
                Username = username,            // 사용자명
                Password = token,               // 토큰을 Password 슬롯에 저장
                Type = CredentialType.Generic,  // 일반 자격증명
                PersistanceType = PersistanceType.LocalComputer // 이 PC에 유지
            };

            if (!cred.Save())
                throw new InvalidOperationException("Credential 저장 실패");
        }

        public static (string Username, string Token)? Load()
        {
            var cred = new Credential { Target = Target, Type = CredentialType.Generic };
            if (!cred.Load())
                return null;

            return (cred.Username, cred.Password);
        }

        public static void Delete()
        {
            var cred = new Credential { Target = Target, Type = CredentialType.Generic };
            cred.Delete();
        }
    }
}
