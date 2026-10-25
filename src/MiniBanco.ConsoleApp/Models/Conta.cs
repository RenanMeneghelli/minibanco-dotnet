using System;
using System.Collections.Generic;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

/// <summary>
/// Classe base de todas as contas. As subclasses sobrescrevem Sacar() e
/// AplicarFechamentoMensal() para aplicar as regras de cada tipo (polimorfismo).
/// </summary>
public abstract class Conta
{
    public string Titular { get; }
    public int Numero { get; }

    // Qualquer um pode ler o saldo, mas só a Conta e as subclasses podem alterá-lo.
    public decimal Saldo { get; protected set; }

    public List<Transacao> Extrato { get; }

    // Usados pelo relatório, para não precisar verificar o tipo da conta com "is".
    public abstract string TipoConta { get; }
    public abstract string DescreverAtributosEspecificos();

    protected Conta(string titular, int numero, decimal saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(titular))
        {
            throw new ArgumentException("O titular não pode ser vazio.");
        }

        if (numero <= 0)
        {
            throw new ArgumentException("O número da conta deve ser maior que zero.");
        }

        if (saldoInicial < 0)
        {
            throw new ArgumentException("O saldo inicial não pode ser negativo.");
        }

        this.Titular = titular.Trim();
        this.Numero = numero;
        this.Saldo = saldoInicial; // o saldo inicial não gera transação no extrato
        this.Extrato = new List<Transacao>();
    }

    // Só registra no extrato. Quem chama é responsável por ajustar o saldo,
    // por isso o método é protected: de fora da hierarquia ele deixaria extrato e saldo inconsistentes.
    protected void RegistrarTransacao(string descricao, string tipo, decimal valor)
    {
        Transacao transacao = new Transacao(descricao, tipo, valor);
        Extrato.Add(transacao);
    }

    public void Depositar(decimal valor, string descricao = "Depósito em dinheiro")
    {
        RegistrarTransacao(descricao, "Deposito", valor); // valida o valor antes de mexer no saldo
        Saldo += valor;
    }

    public virtual void Sacar(decimal valor)
    {
        ValidarValor(valor);

        if (valor > Saldo)
        {
            throw new SaldoInsuficienteException(
                $"Saldo insuficiente. Saldo atual: R$ {Saldo:F2} | Saque solicitado: R$ {valor:F2}.");
        }

        EfetuarSaque(valor);
    }

    public virtual void AplicarFechamentoMensal()
    {
        // Conta genérica não tem regra de fechamento; cada tipo sobrescreve.
    }

    protected void EfetuarSaque(decimal valor)
    {
        RegistrarTransacao("Saque", "Saque", valor);
        Saldo -= valor;
    }

    protected void CobrarTarifa(string descricao, decimal valor)
    {
        RegistrarTransacao(descricao, "Tarifa", valor);
        Saldo -= valor;
    }

    protected static void ValidarValor(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("O valor deve ser maior que zero.");
        }
    }
}
