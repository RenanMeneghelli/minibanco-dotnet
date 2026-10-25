using System;

namespace MiniBanco.ConsoleApp.Exceptions;

/// <summary>
/// Lançada por Sacar() quando o saque não é permitido pelas regras da conta.
/// </summary>
public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException(string mensagem) : base(mensagem)
    {
    }
}
