using System;

namespace MiniBanco.ConsoleApp.Models;

public class ContaPoupanca : Conta
{
    // Taxa em % ao mês (ex.: 0.5 significa 0,5%).
    public double TaxaRendimentoMensal { get; }
    public int DiaAniversario { get; }

    public override string TipoConta => "Conta Poupança";

    public ContaPoupanca(string titular, int numero, decimal saldoInicial, double taxaRendimentoMensal, int diaAniversario)
        : base(titular, numero, saldoInicial)
    {
        if (taxaRendimentoMensal < 0)
        {
            throw new ArgumentException("A taxa de rendimento não pode ser negativa.");
        }

        if (diaAniversario < 1 || diaAniversario > 31)
        {
            throw new ArgumentException("O dia de aniversário deve estar entre 1 e 31.");
        }

        this.TaxaRendimentoMensal = taxaRendimentoMensal;
        this.DiaAniversario = diaAniversario;
    }

    // Credita o rendimento: saldo x taxa / 100. A poupança usa o Sacar() da base (sem cheque especial).
    public override void AplicarFechamentoMensal()
    {
        decimal rendimento = Math.Round(Saldo * (decimal)TaxaRendimentoMensal / 100m, 2);

        // Transacao exige valor > 0; sem saldo (ou taxa zero) não há rendimento a registrar.
        if (rendimento > 0)
        {
            RegistrarTransacao("Rendimento mensal", "Deposito", rendimento);
            Saldo += rendimento;
        }
    }

    public override string DescreverAtributosEspecificos()
    {
        return $"Rendimento: {TaxaRendimentoMensal}% ao mês | Aniversário: dia {DiaAniversario}";
    }
}
