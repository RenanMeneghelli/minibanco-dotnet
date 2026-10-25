using System;

namespace MiniBanco.ConsoleApp.Models;

/// <summary>
/// Uma movimentação da conta. Tipo aceito: "Deposito", "Saque" ou "Tarifa".
/// Os dados são validados no construtor e não mudam depois de criados.
/// </summary>
public class Transacao
{
    public string Descricao { get; }
    public string Tipo { get; }
    public decimal Valor { get; }

    public Transacao(string descricao, string tipo, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("A descrição da transação não pode ser vazia.");
        }

        if (tipo != "Deposito" && tipo != "Saque" && tipo != "Tarifa")
        {
            throw new ArgumentException($"Tipo de transação inválido: \"{tipo}\". Use Deposito, Saque ou Tarifa.");
        }

        if (valor <= 0)
        {
            throw new ArgumentException("O valor da transação deve ser maior que zero.");
        }

        this.Descricao = descricao.Trim();
        this.Tipo = tipo;
        this.Valor = valor;
    }
}
