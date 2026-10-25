using System;
using System.Collections.Generic;
using MiniBanco.ConsoleApp.Models;

namespace MiniBanco.ConsoleApp.Services;

/// <summary>
/// Motor de análise: pontuação, índice de saúde, classificação e relatório.
/// Todas as varreduras usam foreach/for e if/else (sem LINQ, conforme RF09).
/// </summary>
public static class MotorAnalise
{
    private const string LinhaDupla = "====================================================================";
    private const string LinhaSimples = "--------------------------------------------------------------------";

    // RF04 - Tabela de pontuação por tipo de transação.
    public static int ObterPontos(string tipo)
    {
        if (tipo == "Deposito")
        {
            return 10;
        }
        else if (tipo == "Saque")
        {
            return 5;
        }
        else
        {
            return 0; // Tarifa
        }
    }

    public static int CalcularPontuacaoObtida(Conta conta)
    {
        int total = 0;
        foreach (Transacao transacao in conta.Extrato)
        {
            total += ObterPontos(transacao.Tipo);
        }
        return total;
    }

    // Pontuação máxima possível = total de transações x 10.
    public static int CalcularPontuacaoMaxima(Conta conta)
    {
        int quantidade = 0;
        foreach (Transacao transacao in conta.Extrato)
        {
            quantidade++;
        }
        return quantidade * 10;
    }

    // RF05 - Percentual = (obtida / máxima) x 100, com cast para double e proteção contra divisão por zero.
    public static double CalcularPercentual(Conta conta)
    {
        int obtida = CalcularPontuacaoObtida(conta);
        int maxima = CalcularPontuacaoMaxima(conta);

        if (maxima == 0)
        {
            return 0.0; // conta sem transações
        }

        return ((double)obtida / maxima) * 100;
    }

    // RF06 - Classificação por faixas.
    public static string Classificar(double percentual)
    {
        if (percentual >= 90)
        {
            return "Conta Saudável";
        }
        else if (percentual >= 60)
        {
            return "Conta em Atenção";
        }
        else
        {
            return "Conta em Risco";
        }
    }

    public static string ObterAcaoCorporativa(double percentual)
    {
        if (percentual >= 90)
        {
            return "Oferecer investimentos e avaliar aumento de limite.";
        }
        else if (percentual >= 60)
        {
            return "Contato da gerência para orientação financeira e revisão de tarifas.";
        }
        else
        {
            return "Bloquear aumento de limite e abrir análise de crédito.";
        }
    }

    // RF08 - Recomendações com base nas tarifas e saques registrados.
    public static List<string> GerarRecomendacoes(Conta conta)
    {
        int tarifas = 0;
        int saques = 0;

        foreach (Transacao transacao in conta.Extrato)
        {
            if (transacao.Tipo == "Tarifa")
            {
                tarifas++;
            }
            else if (transacao.Tipo == "Saque")
            {
                saques++;
            }
        }

        List<string> recomendacoes = new List<string>();

        if (tarifas > 0)
        {
            recomendacoes.Add("Revisar o pacote de tarifas do cliente e avaliar isenção ou negociação.");
        }

        if (conta is ContaSalario && tarifas > 0)
        {
            recomendacoes.Add("Orientar sobre o limite de saques mensais e sugerir outro tipo de conta (ex.: Conta Corrente).");
        }

        if (conta.Saldo < 0)
        {
            recomendacoes.Add("Conta usando o limite do cheque especial: contatar o cliente para orientação financeira.");
        }
        else if (saques > 0)
        {
            recomendacoes.Add("Acompanhar as retiradas e entrar em contato com o cliente se o padrão de saques continuar.");
        }

        return recomendacoes;
    }

    // RF10 - Relatório formatado de uma conta.
    public static void ExibirRelatorio(Conta conta, int posicao, int total)
    {
        double percentual = CalcularPercentual(conta);
        int obtida = CalcularPontuacaoObtida(conta);
        int maxima = CalcularPontuacaoMaxima(conta);
        string classificacao = Classificar(percentual);

        Console.WriteLine($"[{posicao}/{total}] PROCESSANDO CONTA");
        Console.WriteLine(LinhaSimples);

        Console.WriteLine("> DADOS DA CONTA:");
        Console.WriteLine($"  - Tipo: {conta.TipoConta}");
        Console.WriteLine($"  - Titular: {conta.Titular} | Conta: {conta.Numero}");
        Console.WriteLine($"  - Atributo Específico: {conta.DescreverAtributosEspecificos()}");
        Console.WriteLine($"  - Saldo Atual: {FormatarMoeda(conta.Saldo)}");
        Console.WriteLine();

        Console.WriteLine($"> MOVIMENTAÇÕES REGISTRADAS ({conta.Extrato.Count} TRANSAÇÕES):");
        if (conta.Extrato.Count == 0)
        {
            Console.WriteLine("  (nenhuma movimentação registrada)");
        }
        foreach (Transacao transacao in conta.Extrato)
        {
            string marcador = ObterMarcador(transacao.Tipo);
            string descricao = (transacao.Descricao + " ").PadRight(30, '-');
            Console.WriteLine($"  {marcador} {descricao} {transacao.Tipo}: {FormatarMoeda(transacao.Valor)} ({ObterPontos(transacao.Tipo)} pts)");
        }
        Console.WriteLine();

        Console.WriteLine("> RESUMO DA PONTUAÇÃO:");
        Console.WriteLine($"  - Pontuação Atingida: {obtida} de {maxima} pontos possíveis");
        Console.WriteLine($"  - Índice de Saúde: {percentual:F1}%");
        if (conta.Extrato.Count == 0)
        {
            Console.WriteLine("  - Observação: conta sem movimentações; índice considerado 0,0%.");
        }
        Console.WriteLine($"  - Classificação Final: [ {classificacao.ToUpper()} ]");
        Console.WriteLine($"  - Ação Corporativa: {ObterAcaoCorporativa(percentual)}");
        Console.WriteLine();

        Console.WriteLine("> RELATÓRIO DE PENDÊNCIAS E RECOMENDAÇÕES DA GERÊNCIA:");
        ExibirPendencias(conta);
        Console.WriteLine(LinhaSimples);
        Console.WriteLine();
    }

    // Relatório de todas as contas, com cabeçalho e rodapé.
    public static void ExibirRelatorioGeral(List<Conta> contas)
    {
        Console.WriteLine();
        Console.WriteLine(LinhaDupla);
        Console.WriteLine("        MINIBANCO .NET - MOTOR DE GESTÃO DE CONTAS");
        Console.WriteLine(LinhaDupla);
        Console.WriteLine();

        for (int i = 0; i < contas.Count; i++)
        {
            ExibirRelatorio(contas[i], i + 1, contas.Count);
        }

        Console.WriteLine(LinhaDupla);
        Console.WriteLine("        FIM DO PROCESSAMENTO DE CONTAS");
        Console.WriteLine(LinhaDupla);
    }

    // RF07 + RF08 - Itens críticos (tarifas), itens de atenção (saques) e recomendações.
    private static void ExibirPendencias(Conta conta)
    {
        bool temTarifa = false;
        bool temSaque = false;

        Console.WriteLine("  🔴 ITENS CRÍTICOS (TARIFAS):");
        foreach (Transacao transacao in conta.Extrato)
        {
            if (transacao.Tipo == "Tarifa")
            {
                temTarifa = true;
                Console.WriteLine($"    - {transacao.Descricao} ({FormatarMoeda(transacao.Valor)}): avaliar isenção ou migração para pacote de serviços.");
            }
        }
        if (!temTarifa)
        {
            Console.WriteLine("    - Nenhuma tarifa registrada.");
        }
        Console.WriteLine();

        Console.WriteLine("  🟡 ITENS DE ATENÇÃO (SAQUES):");
        foreach (Transacao transacao in conta.Extrato)
        {
            if (transacao.Tipo == "Saque")
            {
                temSaque = true;
                Console.WriteLine($"    - Saque de {FormatarMoeda(transacao.Valor)}: retirada que exige acompanhamento.");
            }
        }
        if (!temSaque)
        {
            Console.WriteLine("    - Nenhum saque registrado.");
        }
        Console.WriteLine();

        if (!temTarifa && !temSaque)
        {
            Console.WriteLine("  🟢 Nenhuma pendência identificada. Oferecer produtos de investimento ao cliente.");
            return;
        }

        Console.WriteLine("  📋 RECOMENDAÇÕES PARA A GERÊNCIA:");
        List<string> recomendacoes = GerarRecomendacoes(conta);
        foreach (string recomendacao in recomendacoes)
        {
            Console.WriteLine($"    - {recomendacao}");
        }
    }

    private static string ObterMarcador(string tipo)
    {
        if (tipo == "Deposito")
        {
            return "[OK]";
        }
        else if (tipo == "Saque")
        {
            return "[ ! ]";
        }
        else
        {
            return "[ X ]";
        }
    }

    private static string FormatarMoeda(decimal valor)
    {
        return $"R$ {valor:F2}";
    }
}
