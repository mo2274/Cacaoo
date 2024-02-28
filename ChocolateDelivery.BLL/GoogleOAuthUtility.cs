using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public static class GoogleOAuthUtility
    {
        public static string CreateJwtForFirebaseMessaging(int expirationInMinutes = 30)
        {
            string privateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQClupOvE9fgINBm\nb6TOwXuMqiRt79OlNQAC86lwODvR0Cls4mncbARd63PuI4D7uSAf5BuGew+ylypv\n+2jQDgP6UU1A47SIksFyHFPoLziRP8GauCTcnlu/Fkg9o23cQq4C2jbEuA54G+ao\n1/z6Rei5RERmjzSleezINlVe5/WvGaKjfw0kNl/l5CQVDV4p1B3vhob+6jxEtzx6\nh3Gw6gxyKmuttlVxCrm7FIogFETgYtkxUA1uh9kD8q8xe++h+g4xhKaHKQxNuGwR\nUIk+3b81psFXWmcaAXjwJ9nzrBTF3iRfrB5xwdj1UiubksJuunAKZs+wCtjhElx1\nKXEDl03lAgMBAAECggEAGwvtgTOcVgQU7rrGq0VxS94SjuyGXl5db53jZ6CNxqqR\nsCbtPt9UAR3lcBZUPQCT6RB5m4Nm5u77qoXdr6Kx7H6rPG1M78L/lgdgfDKEqVO9\n3Wmy/lxLvc49oO2Uhs9NBcMmEAm3tsta4/BH5XJWTjIBdblmTqEszunrGc+CTX5+\nbCmtSA0zUVhyY4utOWCax7SGib7gKDXEHtEWivkWn8wR/Am2s20GsQvL+6NT4QXW\ndpTKGy1VMPmqzqardk7xUjoDVtMHT2bXdEiEeJ6C/Z+a2uMYcu9a7iv7EJbMqy5E\nLqciwJQlU2RB6dnP7G8/UHhcqjK6FUGGUs8nv5LrEQKBgQDk/RQj0MIBpLFy8Pcz\n5mGrInSzLmgJEHxXUyDE2IipT3Y8NuC1QEM3IGoLzar97HBtTcvrfc1ZeigXGNnJ\nlLXAHXJZfOmpablHueTMZDRBS+nSCGswcbXP9UnjChqXGBNJhZO9RVnvIhSvunh2\n5PbfvYUos4gIYSYqT+78cMHL/QKBgQC5RzNb4wfok1+f97JupTPzGUvXOxIT0qK+\nE9pYiyyJKNaucDk9HnahRoif0iiRUs8wYzR1nnPO9c2E+ezJxRF5rCkMxAM70WIh\nPScMnYH++AyOW80Djgf/ipOjxX02KSHvTnwn+ZTtDjb6aSIy+YB0gZhAX9I2jFq9\nOber6BBKCQKBgQCr8s54xn2oNj95jDBUrJ6Sn2D6W9KLW/HCsQ1eQyr72Et0k9DQ\nydyvPIvlKR9JZY5WxaBmySS2F+Ca13JSyHRiOrpvMDlVmaojjhec03HZxmNh8Cht\nTDW2Xv6ImkG1S8LP5po1+im6Q+E8w81Jmk03XVzsX0O65xh9lWBj91fPEQKBgBxq\nOgs2l2jo6Tp8X4YumfgHgiUhLxWRMVpbaxo3Rf5HIB2ionSoLmNvkNaKAaTAAXyX\nSl1bjsFH21wwhEsxiQuTBrypdyF+bEFSwqFhqqUy36IZnCiWaM2dMCVmzVw4FLdB\n3zK2SUvN6UqgQxl0QGod0f7Vc0TgY64vouafcZgZAoGBAIm1BH2RF5xNk4ZRTBLg\nDFnRaFTVFHp4ppneudmWAY9x9aMlJxZEH73hRPGf67WAj07MNuR2bLR9E1G0gu3o\nuVLh244GepWRWOJTPZ5fN7cBWr3I/xE9GalVR3zViGW67fb/hLwJohNzLn++XADt\nqYFwBKYXdedpzQ4HwPOTwoPK\n-----END PRIVATE KEY-----\n";
            //var addingExtraTime = 420; //in mins (adding extra time due to time difference of server time)
            long unixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds() ;
            long expirationUnixSeconds = DateTimeOffset.Now.AddMinutes(expirationInMinutes).ToUnixTimeSeconds();
            //long unixSeconds = DateTime.Now.ToUnixTimeSeconds();
            //long expirationUnixSeconds = DateTimeOffset.Now.AddMinutes(expirationInMinutes).ToUnixTimeSeconds();
            RSAParameters rsaParameters = DecodeRsaParameters(privateKey);
           
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("iss", "firebase-adminsdk-shy72@cacaoo-409514.iam.gserviceaccount.com"),
                    new Claim("scope", "https://www.googleapis.com/auth/firebase.messaging"),
                    new Claim("aud", "https://oauth2.googleapis.com/token"),
                    new Claim("exp", expirationUnixSeconds.ToString()),
                    new Claim("iat", unixSeconds.ToString()),
                }),
                Expires = DateTimeOffset.Now.AddMinutes(expirationInMinutes).DateTime ,
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsaParameters), SecurityAlgorithms.RsaSha256) // RS256 with Sha hash required by Google OAuth
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Code of DecodeRsaParameters and below helpers methods and classes copied from https://github.com/googleapis/google-api-dotnet-client 
        private static RSAParameters DecodeRsaParameters(string pkcs8PrivateKey)
        {
            if (string.IsNullOrWhiteSpace(pkcs8PrivateKey))
            {
                throw new ArgumentException("Empty PrivateKey");
            }

            const string PrivateKeyPrefix = "-----BEGIN PRIVATE KEY-----";
            const string PrivateKeySuffix = "-----END PRIVATE KEY-----";

            pkcs8PrivateKey = pkcs8PrivateKey.Trim();
            if (!pkcs8PrivateKey.StartsWith(PrivateKeyPrefix) || !pkcs8PrivateKey.EndsWith(PrivateKeySuffix))
            {
                throw new ArgumentException($"PKCS8 data must be contained within '{PrivateKeyPrefix}' and '{PrivateKeySuffix}'.", nameof(pkcs8PrivateKey));
            }

            string base64PrivateKey =
                pkcs8PrivateKey.Substring(PrivateKeyPrefix.Length, pkcs8PrivateKey.Length - PrivateKeyPrefix.Length - PrivateKeySuffix.Length);
            // FromBase64String() ignores whitespace, so further Trim()ing isn't required.
            byte[] pkcs8Bytes = Convert.FromBase64String(base64PrivateKey);

            object ans1 = Asn1.Decode(pkcs8Bytes);
            object[] parameters = (object[])((object[])ans1)[2];

            var rsaParmeters = new RSAParameters
            {
                Modulus = TrimLeadingZeroes((byte[])parameters[1]),
                Exponent = TrimLeadingZeroes((byte[])parameters[2], alignTo8Bytes: false),
                D = TrimLeadingZeroes((byte[])parameters[3]),
                P = TrimLeadingZeroes((byte[])parameters[4]),
                Q = TrimLeadingZeroes((byte[])parameters[5]),
                DP = TrimLeadingZeroes((byte[])parameters[6]),
                DQ = TrimLeadingZeroes((byte[])parameters[7]),
                InverseQ = TrimLeadingZeroes((byte[])parameters[8]),
            };

            return rsaParmeters;
        }

        private static byte[] TrimLeadingZeroes(byte[] bs, bool alignTo8Bytes = true)
        {
            int zeroCount = 0;
            while (zeroCount < bs.Length && bs[zeroCount] == 0) zeroCount += 1;

            int newLength = bs.Length - zeroCount;
            if (alignTo8Bytes)
            {
                int remainder = newLength & 0x07;
                if (remainder != 0)
                {
                    newLength += 8 - remainder;
                }
            }

            if (newLength == bs.Length)
            {
                return bs;
            }

            byte[] result = new byte[newLength];
            if (newLength < bs.Length)
            {
                Buffer.BlockCopy(bs, bs.Length - newLength, result, 0, newLength);
            }
            else
            {
                Buffer.BlockCopy(bs, 0, result, newLength - bs.Length, bs.Length);
            }
            return result;
        }

        private class Asn1
        {
            private enum Tag
            {
                Integer = 2,
                OctetString = 4,
                Null = 5,
                ObjectIdentifier = 6,
                Sequence = 16,
            }

            private class Decoder
            {
                public Decoder(byte[] bytes)
                {
                    _bytes = bytes;
                    _index = 0;
                }

                private byte[] _bytes;
                private int _index;

                public object Decode()
                {
                    Tag tag = ReadTag();
                    switch (tag)
                    {
                        case Tag.Integer:
                            return ReadInteger();
                        case Tag.OctetString:
                            return ReadOctetString();
                        case Tag.Null:
                            return ReadNull();
                        case Tag.ObjectIdentifier:
                            return ReadOid();
                        case Tag.Sequence:
                            return ReadSequence();
                        default:
                            throw new NotSupportedException($"Tag '{tag}' not supported.");
                    }
                }

                private byte NextByte() => _bytes[_index++];

                private byte[] ReadLengthPrefixedBytes()
                {
                    int length = ReadLength();
                    return ReadBytes(length);
                }

                private byte[] ReadInteger() => ReadLengthPrefixedBytes();

                private object ReadOctetString()
                {
                    byte[] bytes = ReadLengthPrefixedBytes();
                    return new Decoder(bytes).Decode();
                }

                private object ReadNull()
                {
                    int length = ReadLength();
                    if (length != 0)
                    {
                        throw new InvalidDataException("Invalid data, Null length must be 0.");
                    }
                    return null;
                }

                private int[] ReadOid()
                {
                    byte[] oidBytes = ReadLengthPrefixedBytes();
                    List<int> result = new List<int>();
                    bool first = true;
                    int index = 0;
                    while (index < oidBytes.Length)
                    {
                        int subId = 0;
                        byte b;
                        do
                        {
                            b = oidBytes[index++];
                            if ((subId & 0xff000000) != 0)
                            {
                                throw new NotSupportedException("Oid subId > 2^31 not supported.");
                            }
                            subId = (subId << 7) | (b & 0x7f);
                        } while ((b & 0x80) != 0);
                        if (first)
                        {
                            first = false;
                            result.Add(subId / 40);
                            result.Add(subId % 40);
                        }
                        else
                        {
                            result.Add(subId);
                        }
                    }
                    return result.ToArray();
                }

                private object[] ReadSequence()
                {
                    int length = ReadLength();
                    int endOffset = _index + length;
                    if (endOffset < 0 || endOffset > _bytes.Length)
                    {
                        throw new InvalidDataException("Invalid sequence, too long.");
                    }
                    List<object> sequence = new List<object>();
                    while (_index < endOffset)
                    {
                        sequence.Add(Decode());
                    }
                    return sequence.ToArray();
                }

                private byte[] ReadBytes(int length)
                {
                    if (length <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");
                    }
                    if (_bytes.Length - length < 0)
                    {
                        throw new ArgumentException("Cannot read past end of buffer.");
                    }
                    byte[] result = new byte[length];
                    Array.Copy(_bytes, _index, result, 0, length);
                    _index += length;
                    return result;
                }

                private Tag ReadTag()
                {
                    byte b = NextByte();
                    int tag = b & 0x1f;
                    if (tag == 0x1f)
                    {
                        // A tag value of 0x1f (31) indicates a tag value of >30 (spec section 8.1.2.4)
                        throw new NotSupportedException("Tags of value > 30 not supported.");
                    }
                    else
                    {
                        return (Tag)tag;
                    }
                }

                private int ReadLength()
                {
                    byte b0 = NextByte();
                    if ((b0 & 0x80) == 0)
                    {
                        return b0;
                    }
                    else
                    {
                        if (b0 == 0xff)
                        {
                            throw new InvalidDataException("Invalid length byte: 0xff");
                        }
                        int byteCount = b0 & 0x7f;
                        if (byteCount == 0)
                        {
                            throw new NotSupportedException("Lengths in Indefinite Form not supported.");
                        }
                        int result = 0;
                        for (int i = 0; i < byteCount; i++)
                        {
                            if ((result & 0xff800000) != 0)
                            {
                                throw new NotSupportedException("Lengths > 2^31 not supported.");
                            }
                            result = (result << 8) | NextByte();
                        }
                        return result;
                    }
                }

            }

            public static object Decode(byte[] bs) => new Decoder(bs).Decode();

        }
    }
}
