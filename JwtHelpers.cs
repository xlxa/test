using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoxMakerAPI.HelperTool
{
    public class JwtHelpers
    {
        /// <summary>
        /// 創建token
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <param name="signKey"></param>
        /// <param name="userName"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        public static string GenerateToken(string issuer, string audience, string signKey, string userName, int expireMinutes)
        {
            //创建用户身份标识，可按需要添加更多資料
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),               
                new Claim("name", userName)// 用户名             
            };
        
            var userClaimsIdentity = new ClaimsIdentity(claims);

            // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

            // HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // 建立 SecurityTokenDescriptor
            var tokenDescriptor = new JwtSecurityToken
            (
                issuer : issuer,
                audience: audience,
                claims: claims,
                expires : DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: signingCredentials
            );

            // 產出所需要的 JWT securityToken 物件，並取得序列化後的 Token 結果(字串格式)
            var serializeToken =new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return serializeToken;
        }
    }
}
