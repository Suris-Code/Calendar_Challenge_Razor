namespace Application.Common.Interfaces;

public interface ICrypto
{
    string DecryptString(string cipherText);
    string EncryptString(string text);
}