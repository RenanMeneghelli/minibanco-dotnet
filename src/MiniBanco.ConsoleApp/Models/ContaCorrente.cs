using System;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

public class ContaCorrente : Conta
{
    private const decimal TarifaManutencao = 10.00m;

    public decimal LimiteChequeEspecial { get; }

    public override string TipoConta => "Conta Corrente";

    public ContaCorrente(string titular, int numero, decimal saldoInicial, decimal limiteChequeEspecial)
        : base(titular, numero, saldoInicial)
    {
        if (limiteChequeEspecial < 0)
        {
            throw new ArgumentException("O limite do cheque especial não pode ser negativo.");
        }

        this.LimiteChequeEspecial = limiteChequeEspecial;
    }

    // Permite sacar até saldo + limite do cheque especial.
    public override void Sacar(decimal valor)
    {
        ValidarValor(valor);

        decimal disponivel = Saldo + LimiteChequeEspecial;
        if (valor > disponivel)
        {
            throw new SaldoInsuficienteException(
                $"Saque acima do disponível. Saldo + limite: R$ {disponivel:F2} | Saque solicitado: R$ {valor:F2}.");
        }

        EfetuarSaque(valor);
    }

    public override void AplicarFechamentoMensal()
    {
        CobrarTarifa("Tarifa de manutenção", TarifaManutencao);
    }

    public override string DescreverAtributosEspecificos()
    {
        return $"Limite Cheque Especial: R$ {LimiteChequeEspecial:F2}";
    }
}
