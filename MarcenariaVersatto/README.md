# Marcenaria Versatto — Sistema de Gestão (ASP.NET Core MVC + EF Core + PostgreSQL)

Trabalho de Curricularização / Frameworks 2º semestre (FEMA).
Stack: C# / ASP.NET Core MVC (.NET 10), Entity Framework Core (Npgsql), PostgreSQL, Materialize CSS, LINQ + Razor.

## Como rodar

1. Banco: crie o banco `Versatto` com o script do documento (seção 4) e rode também `Banco/usuarios.sql` (tabela de login).
2. Senha do banco: copie o `appsettings.Local.json` (já vem neste projeto) e ajuste a connection string.
   Esse arquivo está no `.gitignore` — **a senha não vai para o GitHub**. O `appsettings.json` só tem um placeholder.
3. `dotnet restore` e `dotnet run`.
4. Primeiro acesso: se a tabela `usuarios` estiver vazia, a tela de login mostra o link
   **"Criar o primeiro administrador"** (só funciona uma vez, enquanto não houver nenhum usuário).

## Perfis de acesso (documento: UC01, UC04 e requisito 3 "Controle de permissões")

| Função | admin | funcionario |
|---|---|---|
| Login, consultar materiais/saldo, histórico de movimentações | ✔ | ✔ |
| Registrar entrada e saída de estoque (em seu próprio nome) | ✔ | ✔ |
| Clientes e Orçamentos | ✔ | ✔ |
| Cadastrar/editar/excluir Materiais, Categorias e Fornecedores | ✔ | só consulta |
| Funcionários, Usuários e permissões, Pagamentos, Gerar dados | ✔ | — |
| Alertas de estoque baixo, relatório Orçamentos por cliente | ✔ | — |

## Requisitos do professor × onde está

| Requisito | Onde |
|---|---|
| CRUD (mín. 3, sem contar movimento) | Clientes, Funcionários, Fornecedores, Categorias, Materiais, Orçamentos (+itens), Pagamentos, Usuários |
| CSS / Framework CSS | Materialize CSS + `wwwroot/css/site.css` |
| Tabela Movimento e Movimentação | `movimentacoes_estoque` — telas "Registrar Entrada" / "Registrar Saída" (trigger atualiza o saldo) |
| Validação de campos | Data Annotations (mensagens em português) + jQuery Validation no navegador; `[Documento]` valida CPF/CNPJ (`Validation/` e `wwwroot/js/validacao.js`); telefone, e-mail, faixas, duplicidade (CPF, CNPJ, nome do material), estoque insuficiente na saída |
| Página principal e menu | Home (cartões, atalhos, materiais críticos) + menu superior e lateral que mudam conforme o perfil |
| Autenticação/Autorização | Login por cookie + BCrypt; `[Authorize]` / `[Authorize(Roles = "admin")]` |
| Geração de dados em tabelas | Administração → **Gerar dados** (cria dados "Demo - ..." e permite remover) + listagens/relatórios gerados em tabelas pelo Razor |
| Formulário de filtrar dados nas páginas CRUD | Filtro no topo de todas as listagens (nome, categoria, tipo, status, período...) |
| Consultas em múltiplas tabelas com totalização | Relatórios → **Movimento por material** (movimentações + materiais + categorias) e **Orçamentos por cliente** (clientes + orçamentos + pagamentos), ambos com linha de TOTAL |
| Consulta de agrupamento | Relatórios → **Materiais por categoria** e **Movimentações por mês** (LINQ `GroupBy`) |
| Consulta de Pivot | **Não implementada** (fora do pedido) |
| Uso do GitHub | Ver abaixo |

## Documento do software (casos de uso) × sistema

UC01 Login · UC02 Cadastrar funcionários · UC03 Entrada de estoque · UC04 Permissões (Usuários) · UC05 Fornecedores ·
UC06 Materiais · UC07 Saída de estoque (bloqueia estoque insuficiente) · UC08 Saldo (Materiais, filtro "Situação") ·
UC09 Histórico com filtros (Movimentações) · UC10 Alertas de estoque baixo (Relatórios → Alertas e Home).

## GitHub (vale 1,0 — e sobe só no final desconta 0,5)

```
git init
git add .
git commit -m "Versão inicial do sistema"
git branch -M main
git remote add origin https://github.com/SEU_USUARIO/SEU_REPOSITORIO.git
git push -u origin main
```
Depois, faça um commit a cada ajuste (`git add . && git commit -m "o que mudou" && git push`) — o histórico de commits é a prova de uso durante o desenvolvimento.
Antes do primeiro push confira que `appsettings.Local.json` não aparece em `git status`.
**Se alguma versão com a senha do banco já foi enviada ao GitHub, troque a senha do PostgreSQL.**

## Outras correções feitas

- Removido o endpoint público `CriarUsuarioTeste` (qualquer pessoa podia criar `admin/123456`); entrou o "Primeiro acesso".
- `FuncionariosController` estava sem `[Authorize]` (comentado); agora é só admin.
- Colunas `data_orcamento`/`data_pagamento` (date) e `data_movimentacao`/`criado_em` (timestamp) mapeadas com o tipo certo no `AppDbContext`.
- Exclusão de registro com vínculos mostra mensagem em vez de erro 500.
- Tela "Acesso negado" criada (não existia).
