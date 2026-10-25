# MiniBanco.ConsoleApp — Motor de Gestão de Contas Bancárias

## Identificação

- **Aluno(a):**Douglas Renan Meneghelli 
- **Curso:** Desenvolvedor Back-End .NET — Mini-Projeto Avaliativo, Módulo 2, Semana 07
- **Vídeo de apresentação:** _(cole aqui o link do vídeo — Google Drive com acesso por link ou YouTube não listado)_

## Objetivo

Aplicação de console em C# que simula o núcleo de um banco digital: abre contas de três tipos, registra movimentações, aplica o fechamento mensal de cada tipo de conta e emite um relatório com a **saúde financeira** de cada conta (pontuação, percentual, classificação e recomendações para a gerência).

O projeto pratica: tipos primitivos, `List<T>`, `foreach`/`for`/`while`, `if/else`/`switch`, classes, construtores, encapsulamento, herança, polimorfismo (`virtual`/`override`), exceções (`try/catch/throw`) e Git/GitHub.

## Funcionalidades (menu principal)

| Opção | O que faz |
|-------|-----------|
| 1 - Abrir Nova Conta | Escolhe o tipo, coleta os dados, cria a subclasse correspondente e guarda em `List<Conta>` |
| 2 - Registrar Movimentação | Busca a conta pelo número e executa `Depositar()` ou `Sacar()` |
| 3 - Aplicar Fechamento Mensal | Chama `AplicarFechamentoMensal()` de cada conta, sem verificar o tipo (polimorfismo) |
| 4 - Exibir Relatório das Contas | Mostra dados, extrato, pontuação, índice, classificação, pendências e recomendações |
| 0 - Sair | Encerra o programa |

O menu roda em um laço `do-while` e **não encerra por entrada inválida**: `SaldoInsuficienteException`, `ArgumentException` e `FormatException` são tratadas com `try/catch`, mostrando uma mensagem e voltando ao menu.

## Como executar

Pré-requisito: [.NET SDK 8.0](https://dotnet.microsoft.com/download) ou superior (se usar uma versão mais nova, ajuste `TargetFramework` no `.csproj`).

```bash
git clone <url-do-seu-repositorio>
cd minibanco-dotnet/src/MiniBanco.ConsoleApp
dotnet run
```

Ao digitar valores decimais, use vírgula ou ponto (ex.: `1500,50`), sem separador de milhar.

## Estrutura do projeto

```
minibanco-dotnet/
├── src/
│   └── MiniBanco.ConsoleApp/
│       ├── Program.cs                      # menu e leitura de dados (try/catch)
│       ├── Models/
│       │   ├── Transacao.cs                # Descricao, Tipo, Valor (validados no construtor)
│       │   ├── Conta.cs                    # classe base abstrata
│       │   ├── ContaCorrente.cs
│       │   ├── ContaPoupanca.cs
│       │   └── ContaSalario.cs
│       ├── Exceptions/
│       │   └── SaldoInsuficienteException.cs
│       ├── Services/
│       │   └── MotorAnalise.cs             # pontuação, índice, classificação e relatório
│       └── MiniBanco.ConsoleApp.csproj
└── README.md
```

## Regras de negócio

### Transação
- `Tipo` aceita somente `"Deposito"`, `"Saque"` ou `"Tarifa"`; `Valor` deve ser maior que zero. Caso contrário, lança `ArgumentException`.

### Contas
- **Conta (base):** `Saldo` é lido de fora, mas só alterado pela própria hierarquia (`protected set`). O saldo inicial não gera transação no extrato.
- **ContaCorrente:** permite sacar até `saldo + limite do cheque especial`; o fechamento mensal registra **Tarifa de manutenção de R$ 10,00**.
- **ContaPoupanca:** o fechamento mensal registra um **Depósito de rendimento** = `saldo × taxa / 100` (arredondado a 2 casas). Se o rendimento for zero, nada é registrado.
- **ContaSalario:** a partir do saque que ultrapassa o limite mensal, além do saque é registrada uma **Tarifa de R$ 5,00** (o saldo precisa cobrir saque + tarifa). O fechamento mensal apenas zera a contagem de saques.

### Pontuação e classificação
| Tipo | Pontos |
|------|--------|
| Deposito | 10 |
| Saque | 5 |
| Tarifa | 0 |

`Percentual = (PontuaçãoObtida / (TotalDeTransações × 10)) × 100`, com cast para `double`.

| Percentual | Classificação | Ação corporativa |
|-----------|---------------|------------------|
| 90% a 100% | Conta Saudável | Oferecer investimentos e avaliar aumento de limite |
| 60% a 89% | Conta em Atenção | Contato da gerência para orientação financeira e revisão de tarifas |
| 0% a 59% | Conta em Risco | Bloquear aumento de limite e abrir análise de crédito |

### Relatório
- 🔴 **Itens críticos:** todas as transações do tipo `Tarifa`.
- 🟡 **Itens de atenção:** todas as transações do tipo `Saque`.
- Recomendações da gerência geradas a partir das tarifas e saques registrados.

Todas as varreduras de lista usam `foreach`/`for` e `if` — **sem LINQ**.

## Decisões de projeto e limitações

- `RegistrarTransacao` é `protected`: ele só grava no extrato, e deixá-lo público permitiria registrar uma transação sem ajustar o saldo.
- Conta **sem transações** tem índice **0,0%** (evita divisão por zero), então aparece como "Conta em Risco" até ter movimentações. O relatório avisa isso.
- Os dados ficam apenas em memória: ao fechar o programa, as contas são perdidas.
- O fechamento mensal da conta corrente pode deixar o saldo abaixo do limite do cheque especial, porque a tarifa é sempre cobrada.
- Não há testes automatizados nem controle de datas (o "mês" é definido pelo uso da opção 3).
- Os atributos específicos de cada conta são exibidos por `DescreverAtributosEspecificos()` (polimorfismo), para o relatório não precisar verificar o tipo da conta.
