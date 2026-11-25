using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Data;
using MarcenariaVersatto.Models;
using MarcenariaVersatto.ViewModels;

namespace MarcenariaVersatto.Controllers
{
    // Geração de dados de demonstração nas tabelas (somente administrador).
    // Todo registro gerado leva o prefixo "Demo - " no nome, para poder ser removido depois sem tocar nos dados reais.
    [Authorize(Roles = Perfis.Admin)]
    public class GerarDadosController : Controller
    {
        private const string Prefixo = "Demo - ";

        private readonly AppDbContext _context;

        public GerarDadosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await ContarDemoAsync());
        }

        private async Task<DadosDemoResumo> ContarDemoAsync()
        {
            var matIds = await _context.Materiais.Where(m => m.Nome.StartsWith(Prefixo)).Select(m => m.Id).ToListAsync();
            var cliIds = await _context.Clientes.Where(c => c.Nome.StartsWith(Prefixo)).Select(c => c.Id).ToListAsync();
            var orcIds = await _context.Orcamentos.Where(o => cliIds.Contains(o.ClienteId)).Select(o => o.Id).ToListAsync();

            return new DadosDemoResumo
            {
                Clientes = cliIds.Count,
                Fornecedores = await _context.Fornecedores.CountAsync(f => f.RazaoSocial.StartsWith(Prefixo)),
                Materiais = matIds.Count,
                Movimentacoes = await _context.MovimentacoesEstoque.CountAsync(m => matIds.Contains(m.MaterialId)),
                Orcamentos = orcIds.Count,
                Pagamentos = await _context.Pagamentos.CountAsync(p => orcIds.Contains(p.OrcamentoId))
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gerar()
        {
            if ((await ContarDemoAsync()).Existe)
            {
                TempData["Erro"] = "Já existem dados de demonstração. Remova-os antes de gerar novamente.";
                return RedirectToAction(nameof(Index));
            }

            var funcionarioId = int.TryParse(User.FindFirst("FuncionarioId")?.Value, out var fid) ? fid : 0;
            if (funcionarioId == 0 || !await _context.Funcionarios.AnyAsync(f => f.Id == funcionarioId))
            {
                var primeiro = await _context.Funcionarios.Where(f => f.Ativo).OrderBy(f => f.Id).FirstOrDefaultAsync();
                if (primeiro == null)
                {
                    TempData["Erro"] = "Cadastre pelo menos um funcionário ativo antes de gerar os dados.";
                    return RedirectToAction(nameof(Index));
                }
                funcionarioId = primeiro.Id;
            }

            var rnd = new Random(2026);

            // ---- Categorias (reaproveita as que já existem) ----
            var nomesCategorias = new[] { "Chapas", "Ferragens", "Acabamentos" };
            var categorias = new Dictionary<string, int>();
            foreach (var nome in nomesCategorias)
            {
                var existente = await _context.CategoriasMaterial.FirstOrDefaultAsync(c => c.Nome.ToLower() == nome.ToLower());
                if (existente == null)
                {
                    existente = new CategoriaMaterial { Nome = nome };
                    _context.CategoriasMaterial.Add(existente);
                    await _context.SaveChangesAsync();
                }
                categorias[nome] = existente.Id;
            }

            // ---- Fornecedores ----
            var fornecedores = new List<Fornecedor>();
            foreach (var nome in new[] { "Madeireira Assis", "Ferragens Paraná", "Tintas e Vernizes Vale", "Distribuidora MDF Sul", "Acabamentos Pacaembu" })
                fornecedores.Add(new Fornecedor
                {
                    RazaoSocial = Prefixo + nome,
                    Telefone = "(18) 3322-" + rnd.Next(1000, 9999),
                    Email = "contato@" + nome.ToLower().Replace(" ", "") + ".demo"
                });
            _context.Fornecedores.AddRange(fornecedores);
            await _context.SaveChangesAsync();

            // ---- Materiais (nome, categoria, unidade, mínimo, valor unitário) ----
            var modelos = new (string Nome, string Cat, string Un, decimal Min, decimal Valor)[]
            {
                ("MDF Branco 15mm", "Chapas", "chapa", 10, 289.90m),
                ("MDF Amadeirado 18mm", "Chapas", "chapa", 8, 349.90m),
                ("MDF Cru 6mm", "Chapas", "chapa", 5, 159.00m),
                ("Compensado 15mm", "Chapas", "chapa", 6, 219.00m),
                ("Dobradiça 35mm", "Ferragens", "un", 100, 3.50m),
                ("Corrediça telescópica 45cm", "Ferragens", "par", 40, 18.90m),
                ("Puxador de alumínio", "Ferragens", "un", 80, 7.90m),
                ("Parafuso 4x40", "Ferragens", "cx", 10, 22.00m),
                ("Fita de borda branca", "Acabamentos", "m", 200, 0.89m),
                ("Cola de contato", "Acabamentos", "lata", 6, 38.00m),
                ("Verniz PU", "Acabamentos", "l", 5, 64.00m),
                ("Lixa 120", "Acabamentos", "un", 30, 2.20m)
            };
            var materiais = new List<Material>();
            for (int i = 0; i < modelos.Length; i++)
            {
                var m = modelos[i];
                materiais.Add(new Material
                {
                    Nome = Prefixo + m.Nome,
                    CategoriaId = categorias[m.Cat],
                    FornecedorPadraoId = fornecedores[i % fornecedores.Count].Id,
                    UnidadeMedida = m.Un,
                    EstoqueMinimo = m.Min,
                    EstoqueAtual = 0, // o trigger soma as entradas abaixo
                    ValorUnitario = m.Valor
                });
            }
            _context.Materiais.AddRange(materiais);
            await _context.SaveChangesAsync();

            // ---- Movimentações (SQL direto para poder espalhar as datas nos últimos meses; o trigger atualiza o estoque) ----
            int totalMov = 0;
            for (int i = 0; i < materiais.Count; i++)
            {
                var mat = materiais[i];
                var min = modelos[i].Min;
                var forn = fornecedores[i % fornecedores.Count].Id;

                var entrada1 = Math.Round(min * 3m, 2);
                var entrada2 = Math.Round(min * 1.5m, 2);
                // a cada 3 materiais, um termina abaixo do mínimo (aparece nos alertas)
                var saidaTotal = Math.Round(min * (i % 3 == 0 ? 3.9m : 2.5m), 2);
                var s1 = Math.Round(saidaTotal * 0.4m, 2);
                var s2 = Math.Round(saidaTotal * 0.35m, 2);
                var s3 = Math.Round(saidaTotal - s1 - s2, 2);

                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO movimentacoes_estoque (material_id, tipo, quantidade, data_movimentacao, motivo, fornecedor_id, funcionario_id) VALUES ({mat.Id}, 'entrada', {entrada1}, now() - make_interval(days => {75}), 'Compra inicial (demo)', {forn}, {funcionarioId})");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO movimentacoes_estoque (material_id, tipo, quantidade, data_movimentacao, motivo, fornecedor_id, funcionario_id) VALUES ({mat.Id}, 'entrada', {entrada2}, now() - make_interval(days => {40}), 'Reposição (demo)', {forn}, {funcionarioId})");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO movimentacoes_estoque (material_id, tipo, quantidade, data_movimentacao, motivo, funcionario_id) VALUES ({mat.Id}, 'saida', {s1}, now() - make_interval(days => {50}), 'Uso em móvel (demo)', {funcionarioId})");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO movimentacoes_estoque (material_id, tipo, quantidade, data_movimentacao, motivo, funcionario_id) VALUES ({mat.Id}, 'saida', {s2}, now() - make_interval(days => {25}), 'Uso em móvel (demo)', {funcionarioId})");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO movimentacoes_estoque (material_id, tipo, quantidade, data_movimentacao, motivo, funcionario_id) VALUES ({mat.Id}, 'saida', {s3}, now() - make_interval(days => {5}), 'Uso em móvel (demo)', {funcionarioId})");
                totalMov += 5;
            }

            // ---- Clientes ----
            var dadosClientes = new[]
            {
                ("Maria Souza", "Assis"), ("João Pereira", "Assis"), ("Ana Lima", "Cândido Mota"), ("Carlos Almeida", "Paraguaçu Paulista"),
                ("Fernanda Rocha", "Maracaí"), ("Pedro Santos", "Assis"), ("Juliana Costa", "Tarumã"), ("Ricardo Gomes", "Echaporã")
            };
            var clientes = new List<Cliente>();
            foreach (var (nome, cidade) in dadosClientes)
                clientes.Add(new Cliente
                {
                    Nome = Prefixo + nome,
                    TipoPessoa = "F",
                    Cidade = cidade,
                    Telefone = "(18) 99" + rnd.Next(100, 999) + "-" + rnd.Next(1000, 9999),
                    Email = nome.ToLower().Replace(" ", ".") + "@demo.com"
                });
            _context.Clientes.AddRange(clientes);
            await _context.SaveChangesAsync();

            // ---- Orçamentos (+ itens) e pagamentos ----
            var statusCiclo = new[] { "aprovado", "aprovado", "pendente", "aprovado", "recusado", "pendente", "aprovado", "aprovado", "pendente", "aprovado" };
            var formas = Pagamento.FormasPagamento;
            int n = 0;
            foreach (var cliente in clientes)
            {
                for (int k = 0; k < (n % 3 == 0 ? 2 : 1); k++)
                {
                    var status = statusCiclo[n % statusCiclo.Length];
                    var orc = new Orcamento
                    {
                        ClienteId = cliente.Id,
                        Versao = 1,
                        DataOrcamento = DateTime.Today.AddDays(-rnd.Next(5, 90)),
                        ValorMaoObra = Math.Round((decimal)rnd.Next(800, 3500), 2),
                        Status = status
                    };

                    decimal somaMateriais = 0;
                    var escolhidos = materiais.OrderBy(_ => rnd.Next()).Take(rnd.Next(2, 5)).ToList();
                    foreach (var mat in escolhidos)
                    {
                        var qtd = Math.Round((decimal)rnd.Next(1, 8), 2);
                        orc.Materiais.Add(new OrcamentoMaterial { MaterialId = mat.Id, QuantidadeEstimada = qtd, ValorUnitario = mat.ValorUnitario });
                        somaMateriais += qtd * mat.ValorUnitario;
                    }
                    orc.ValorMateriais = Math.Round(somaMateriais, 2);
                    _context.Orcamentos.Add(orc);
                    await _context.SaveChangesAsync();

                    if (status == "aprovado")
                    {
                        var total = orc.ValorMateriais + orc.ValorMaoObra;
                        var sinal = Math.Round(total * 0.5m, 2);
                        _context.Pagamentos.Add(new Pagamento
                        {
                            OrcamentoId = orc.Id, Valor = sinal, FormaPagamento = formas[rnd.Next(formas.Length)],
                            DataPagamento = orc.DataOrcamento.AddDays(2), Status = "pago"
                        });
                        _context.Pagamentos.Add(new Pagamento
                        {
                            OrcamentoId = orc.Id, Valor = Math.Round(total - sinal, 2), FormaPagamento = formas[rnd.Next(formas.Length)],
                            DataPagamento = n % 2 == 0 ? DateTime.Today.AddDays(-1) : (DateTime?)null,
                            Status = n % 2 == 0 ? "pago" : "pendente"
                        });
                        await _context.SaveChangesAsync();
                    }
                    n++;
                }
            }

            TempData["Info"] = $"Dados de demonstração gerados: {fornecedores.Count} fornecedores, {materiais.Count} materiais, {totalMov} movimentações, {clientes.Count} clientes e {n} orçamentos.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover()
        {
            try
            {
                var matIds = await _context.Materiais.Where(m => m.Nome.StartsWith(Prefixo)).Select(m => m.Id).ToListAsync();
                var cliIds = await _context.Clientes.Where(c => c.Nome.StartsWith(Prefixo)).Select(c => c.Id).ToListAsync();

                // orçamentos apagam seus itens e pagamentos em cascata
                await _context.Orcamentos.Where(o => cliIds.Contains(o.ClienteId)).ExecuteDeleteAsync();
                await _context.MovimentacoesEstoque.Where(m => matIds.Contains(m.MaterialId)).ExecuteDeleteAsync();
                await _context.Materiais.Where(m => matIds.Contains(m.Id)).ExecuteDeleteAsync();
                await _context.Clientes.Where(c => cliIds.Contains(c.Id)).ExecuteDeleteAsync();
                await _context.Fornecedores.Where(f => f.RazaoSocial.StartsWith(Prefixo)).ExecuteDeleteAsync();

                TempData["Info"] = "Dados de demonstração removidos. (As categorias criadas continuam cadastradas.)";
            }
            catch (DbUpdateException)
            {
                TempData["Erro"] = "Não foi possível remover tudo: algum dado de demonstração foi usado em registros reais (orçamento ou movimentação).";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
