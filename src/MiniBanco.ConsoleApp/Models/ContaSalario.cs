using System;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

public class ContaSalario : Conta
{
    private const decimal TarifaSaqueExcedente = 5.00m;

    public string Empregador { get; }
    public int LimiteSaquesMensais { get; }
    public int SaquesNoMes { get; private set; }

    public override string TipoConta => "Conta Salário";

    public ContaSalario(string titular, int numero, decimal saldoInicial, string empregador, int limiteSaquesMensais)
        : base(titular, numero, saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(empregador))
        {
            throw new ArgumentException("O empregador não pode ser vazio.");
        }

        if (limiteSaquesMensais < 0)
        {
            throw new ArgumentException("O limite de saques mensais não pode ser negativo.");
        }

        this.Empregador = empregador.Trim();
        this.LimiteSaquesMensais = limiteSaquesMensais;
        this.SaquesNoMes = 0;
    }

    // A partir do saque que ultrapassa o limite mensal, além do saque cobra tarifa de R$ 5,00.
    public override void Sacar(decimal valor)
    {
        ValidarValor(valor);

        bool ultrapassaLimite = (SaquesNoMes + 1) > LimiteSaquesMensais;
        decimal totalDebitado = ultrapassaLimite ? valor + TarifaSaqueExcedente : valor;

        if (totalDebitado > Saldo)
        {
            throw new SaldoInsuficienteException(
                $"Saldo insuficiente (conta salário não tem limite). Saldo atual: R$ {Saldo:F2} | " +
                $"Total do saque{(ultrapassaLimite ? " + tarifa" : "")}: R$ {totalDebitado:F2}.");
        }

        EfetuarSaque(valor);
        SaquesNoMes++;

        if (ultrapassaLimite)
        {
            CobrarTarifa("Tarifa por saque acima do limite", TarifaSaqueExcedente);
        }
    }

    // Sem tarifa de manutenção: só zera a contagem de saques do mês.
    public override void AplicarFechamentoMensal()
    {
        SaquesNoMes = 0;
    }

    public override string DescreverAtributosEspecificos()
    {
        return $"Empregador: {Empregador} | Limite de saques mensais: {LimiteSaquesMensais} (usados: {SaquesNoMes})";
    }
}
