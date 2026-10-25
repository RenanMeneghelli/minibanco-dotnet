using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MiniBanco.ConsoleApp.Exceptions;
using MiniBanco.ConsoleApp.Models;
using MiniBanco.ConsoleApp.Services;

namespace MiniBanco.ConsoleApp;

public class Program
{
    // Lista central de contas do banco.
    private static readonly List<Conta> contas = new List<Conta>();

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        ConfigurarCultura();

        string opcao;
        do
        {
            ExibirMenu();
            opcao = (Console.ReadLine() ?? "0").Trim(); // fim da entrada (null) encerra o programa

            // RF12 - nenhuma entrada inválida ou operação não permitida derruba o programa.
            try
            {
                switch (opcao)
                {
                    case "1":
                        AbrirNovaConta();
                        break;
                    case "2":
                        RegistrarMovimentacao();
                        break;
                    case "3":
                        AplicarFechamentoMensal();
                        break;
                    case "4":
                        ExibirRelatorio();
                        break;
                    case "0":
                        Console.WriteLine("Encerrando o MiniBanco. Até logo!");
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Escolha uma das opções do menu.");
                        break;
                }
            }
            catch (SaldoInsuficienteException ex)
            {
                Console.WriteLine($"[Operação negada] {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Dado inválido] {ex.Message}");
            }
            catch (FormatException)
            {
                Console.WriteLine("[Entrada inválida] Digite um número válido onde um número é esperado (ex.: 1500,50).");
            }
            catch (Exception ex)
            {
                // Rede de segurança para casos não previstos (ex.: número grande demais).
                Console.WriteLine($"[Erro inesperado] {ex.Message}");
            }
        } while (opcao != "0");
    }

    private static void ExibirMenu()
    {
        Console.WriteLine();
        Console.WriteLine("========== MINIBANCO .NET ==========");
        Console.WriteLine("1 - Abrir Nova Conta");
        Console.WriteLine("2 - Registrar Movimentação");
        Console.WriteLine("3 - Aplicar Fechamento Mensal");
        Console.WriteLine("4 - Exibir Relatório das Contas");
        Console.WriteLine("0 - Sair");
        Console.Write("Escolha uma opção: ");
    }

    // ---------- Opção 1 ----------
    private static void AbrirNovaConta()
    {
        Console.WriteLine("Tipo de conta: 1 - Conta Corrente | 2 - Conta Poupança | 3 - Conta Salário");
        string tipo = LerTexto("Tipo: ");

        if (tipo != "1" && tipo != "2" && tipo != "3")
        {
            Console.WriteLine("Tipo de conta inválido.");
            return;
        }

        string titular = LerTexto("Titular: ");
        int numero = LerInt("Número da conta: ");

        if (BuscarConta(numero) != null)
        {
            throw new ArgumentException($"Já existe uma conta com o número {numero}.");
        }

        decimal saldoInicial = LerDecimal("Saldo inicial (R$): ");
        Conta novaConta;

        if (tipo == "1")
        {
            decimal limite = LerDecimal("Limite do cheque especial (R$): ");
            novaConta = new ContaCorrente(titular, numero, saldoInicial, limite);
        }
        else if (tipo == "2")
        {
            double taxa = LerDouble("Taxa de rendimento mensal (%): ");
            int diaAniversario = LerInt("Dia de aniversário (1-31): ");
            novaConta = new ContaPoupanca(titular, numero, saldoInicial, taxa, diaAniversario);
        }
        else
        {
            string empregador = LerTexto("Empregador: ");
            int limiteSaques = LerInt("Limite de saques mensais: ");
            novaConta = new ContaSalario(titular, numero, saldoInicial, empregador, limiteSaques);
        }

        contas.Add(novaConta);
        Console.WriteLine($"{novaConta.TipoConta} nº {novaConta.Numero} aberta para {novaConta.Titular}.");
    }

    // ---------- Opção 2 ----------
    private static void RegistrarMovimentacao()
    {
        int numero = LerInt("Número da conta: ");
        Conta? conta = BuscarConta(numero);

        if (conta == null)
        {
            Console.WriteLine($"Nenhuma conta encontrada com o número {numero}.");
            return;
        }

        Console.WriteLine("Operação: 1 - Depósito | 2 - Saque");
        string operacao = LerTexto("Operação: ");

        if (operacao != "1" && operacao != "2")
        {
            Console.WriteLine("Operação inválida.");
            return;
        }

        decimal valor = LerDecimal("Valor (R$): ");

        if (operacao == "1")
        {
            conta.Depositar(valor, "Depósito em dinheiro");
            Console.WriteLine($"Depósito realizado. Saldo atual: R$ {conta.Saldo:F2}");
        }
        else
        {
            conta.Sacar(valor);
            Console.WriteLine($"Saque realizado. Saldo atual: R$ {conta.Saldo:F2}");
        }
    }

    // ---------- Opção 3 ----------
    private static void AplicarFechamentoMensal()
    {
        if (contas.Count == 0)
        {
            Console.WriteLine("Nenhuma conta aberta até o momento.");
            return;
        }

        // Polimorfismo: cada conta executa a sua própria regra, sem verificar o tipo.
        foreach (Conta conta in contas)
        {
            conta.AplicarFechamentoMensal();
        }

        Console.WriteLine($"Fechamento mensal aplicado em {contas.Count} conta(s).");
    }

    // ---------- Opção 4 ----------
    private static void ExibirRelatorio()
    {
        if (contas.Count == 0)
        {
            Console.WriteLine("Nenhuma conta aberta até o momento.");
            return;
        }

        MotorAnalise.ExibirRelatorioGeral(contas);
    }

    // ---------- Auxiliares ----------
    // Busca com laço tradicional (sem LINQ).
    private static Conta? BuscarConta(int numero)
    {
        foreach (Conta conta in contas)
        {
            if (conta.Numero == numero)
            {
                return conta;
            }
        }
        return null;
    }

    private static string LerTexto(string mensagem)
    {
        Console.Write(mensagem);
        return (Console.ReadLine() ?? "").Trim();
    }

    // int.Parse lança FormatException quando o texto não é número (tratado no menu).
    private static int LerInt(string mensagem)
    {
        return int.Parse(LerTexto(mensagem), CultureInfo.InvariantCulture);
    }

    // Aceita vírgula ou ponto como separador decimal, independente da cultura do computador.
    private static decimal LerDecimal(string mensagem)
    {
        string texto = LerTexto(mensagem).Replace(',', '.');
        return decimal.Parse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    private static double LerDouble(string mensagem)
    {
        string texto = LerTexto(mensagem).Replace(',', '.');
        return double.Parse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    // Exibe valores no padrão brasileiro (vírgula decimal) quando a cultura está disponível.
    private static void ConfigurarCultura()
    {
        try
        {
            CultureInfo cultura = new CultureInfo("pt-BR");
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;
        }
        catch (CultureNotFoundException)
        {
            // Mantém a cultura padrão do sistema.
        }
    }
}
