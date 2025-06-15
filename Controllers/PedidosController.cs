using Microsoft.AspNetCore.Authorization; // Para o atributo [Authorize]
using Microsoft.AspNetCore.Identity; // Para UserManager<ApplicationUser>
using Microsoft.AspNetCore.Mvc; // Para Controller, IActionResult
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectList em ViewDatas
using Microsoft.EntityFrameworkCore; // Para métodos de extensão do EF Core (Include, ToListAsync, FirstOrDefaultAsync)
using RyujinBites.Data; // Seu DbContext
using RyujinBites.Models.Identity; // Sua classe ApplicationUser
using RyujinBites.Models.Lanchonete; // Suas classes de modelo (Pedido, Cliente, Cupom)

namespace RyujinBites.Controllers
{
    // O nome do controlador está com "Pedidoes" (plural incorreto). Vou corrigir para "PedidosController".
    // [Authorize] pode ser colocado aqui se TODAS as ações do controlador exigirem autenticação.
    // Mas se MeusPedidos for mais aberto, é melhor colocar [Authorize] nas ações.
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Injetado para obter o usuário logado

        // Construtor do controlador, injetando o contexto do banco de dados e o gerenciador de usuários
        public PedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES PARA CLIENTES E ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // GET: Pedidos/MeusPedidos
        // Permite que usuários com papel "Cliente" ou "Administrador" vejam seus próprios pedidos.
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> MeusPedidos()
        {
            // Obtém o ID do usuário logado (UserId é do tipo string/GUID no Identity)
            var userId = _userManager.GetUserId(User); // Pega o ID do usuário da sessão atual

            // Filtra os pedidos para mostrar apenas aqueles pertencentes ao usuário logado.
            // Inclui as propriedades de navegação Cliente e Cupom para exibição.
            var pedidosDoUsuario = await _context.Pedidos
                                                .Where(p => p.ClienteId == userId) // Filtra pelo ClienteId associado ao usuário logado
                                                .Include(p => p.Cliente) // Carrega os dados do cliente associado ao pedido
                                                .Include(p => p.Cupom)   // Carrega os dados do cupom associado ao pedido
                                                .ToListAsync();

            // Retorna a view com a lista de pedidos do usuário.
            return View("Index", pedidosDoUsuario); // Reutiliza a view 'Index' para exibir 'Meus Pedidos'
        }


        // GET: Pedidos/Create (CRIAR PEDIDO)
        // Permite que usuários com papel "Cliente" ou "Administrador" criem um novo pedido.
        [Authorize(Roles = "Cliente, Administrador")]
        public IActionResult Create()
        {
            // O ClienteId para o novo pedido deve ser o ID do usuário logado, não uma lista.
            // O scaffolding cria um dropdown para ClienteId, mas queremos que seja automático.
            // O CupomId pode ser um dropdown se o cliente puder selecionar cupons.
            ViewData["CupomId"] = new SelectList(_context.Cupons, "CupomId", "Codigo");

            // Não precisamos de SelectList para ClienteId aqui, pois será preenchido automaticamente.
            return View();
        }

        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Protege contra ataques de falsificação de solicitação entre sites.
        [Authorize(Roles = "Cliente, Administrador")] // Protege a ação POST de criação.
        public async Task<IActionResult> Create([Bind("DataPedido,StatusPedido,ValorTotal,TipoEntrega,EnderecoEntrega,Observacoes,CupomId")] Pedido pedido)
        {
            // O ClienteId vem do usuário logado, não do formulário.
            var userId = _userManager.GetUserId(User);
            pedido.ClienteId = userId;

            // Define a DataPedido e o StatusPedido inicial (se não vierem do formulário).
            if (pedido.DataPedido == DateTime.MinValue) // Se não foi setado no formulário
            {
                pedido.DataPedido = DateTime.UtcNow;
            }
            if (string.IsNullOrEmpty(pedido.StatusPedido)) // Se não foi setado no formulário
            {
                pedido.StatusPedido = "Pendente"; // Status inicial padrão
            }

            // Você pode adicionar lógica para calcular ValorTotal aqui, com base nos ItensPedido.
            // Por enquanto, o bind permite que venha do formulário, mas em um sistema real, isso seria calculado.

            if (ModelState.IsValid)
            {
                _context.Add(pedido); // Adiciona o pedido ao contexto.
                await _context.SaveChangesAsync(); // Salva as alterações no banco de dados.

                // Redireciona para a página de "Meus Pedidos" após a criação.
                return RedirectToAction(nameof(MeusPedidos));
            }

            // Se o modelo não for válido, recarrega os dados para os dropdowns e retorna a view com os erros.
            ViewData["CupomId"] = new SelectList(_context.Cupons, "CupomId", "Codigo", pedido.CupomId);
            return View(pedido);
        }


        // ---------------------------------------------------------------------------------------------------
        // AÇÕES EXCLUSIVAS PARA ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // GET: Pedidos/GerenciarPedidos
        // Permite que apenas usuários com papel "Administrador" vejam TODOS os pedidos no sistema.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GerenciarPedidos()
        {
            // Retorna todos os pedidos no sistema, incluindo Cliente e Cupom.
            var todosOsPedidos = await _context.Pedidos
                                                .Include(p => p.Cliente)
                                                .Include(p => p.Cupom)
                                                .ToListAsync();
            // Retorna uma view específica para gerenciamento ou a view Index adaptada.
            return View("Index", todosOsPedidos); // Reutiliza a view 'Index' para exibir todos os pedidos
        }

        // GET: Pedidos/Details/5 (Detalhes de QUALQUER pedido)
        // Apenas Administradores podem ver detalhes de pedidos arbitrários.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Retorna 404 se o ID não for fornecido.
            }

            // Busca o pedido pelo ID, incluindo Cliente e Cupom.
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cupom)
                .FirstOrDefaultAsync(m => m.PedidoId == id);

            if (pedido == null)
            {
                return NotFound(); // Retorna 404 se o pedido não for encontrado.
            }

            return View(pedido); // Retorna a view de detalhes com os dados do pedido.
        }

        // GET: Pedidos/Edit/5
        // Apenas Administradores podem editar pedidos.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            // Popula os dropdowns para Cliente e Cupom na view de edição.
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "ClienteId", pedido.ClienteId);
            ViewData["CupomId"] = new SelectList(_context.Cupons, "CupomId", "Codigo", pedido.CupomId);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")] // Protege a ação POST de edição.
        public async Task<IActionResult> Edit(int id, [Bind("PedidoId,ClienteId,DataPedido,StatusPedido,ValorTotal,TipoEntrega,EnderecoEntrega,Observacoes,CupomId")] Pedido pedido)
        {
            if (id != pedido.PedidoId)
            {
                return NotFound(); // Retorna 404 se o ID na URL não corresponder ao ID do objeto.
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido); // Marca o objeto como modificado no contexto.
                    await _context.SaveChangesAsync(); // Salva as alterações no banco de dados.
                }
                catch (DbUpdateConcurrencyException) // Lida com problemas de concorrência (duas pessoas editando ao mesmo tempo).
                {
                    if (!PedidoExists(pedido.PedidoId)) // Verifica se o pedido ainda existe no DB.
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Lança a exceção novamente se não for um problema de item não encontrado.
                    }
                }
                return RedirectToAction(nameof(GerenciarPedidos)); // Redireciona para a lista de gerenciamento.
            }
            // Se o modelo não for válido, recarrega os dados dos dropdowns e retorna a view com os erros.
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "ClienteId", pedido.ClienteId);
            ViewData["CupomId"] = new SelectList(_context.Cupons, "CupomId", "Codigo", pedido.CupomId);
            return View(pedido);
        }

        // GET: Pedidos/Delete/5
        // Apenas Administradores podem acessar a página de confirmação de exclusão.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca o pedido, incluindo Cliente e Cupom.
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cupom)
                .FirstOrDefaultAsync(m => m.PedidoId == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido); // Retorna a view de exclusão com os dados do pedido.
        }

        // POST: Pedidos/Delete/5
        // Apenas Administradores podem executar a exclusão.
        [HttpPost, ActionName("Delete")] // Mapeia para a URL Delete, mas usa ActionName para a ação real.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")] // Protege a ação POST de exclusão.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido); // Remove o pedido do contexto.
            }

            await _context.SaveChangesAsync(); // Salva as alterações no banco de dados (exclui o pedido).
            return RedirectToAction(nameof(GerenciarPedidos)); // Redireciona para a lista de gerenciamento.
        }

        // Método auxiliar para verificar se um pedido existe.
        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.PedidoId == id);
        }
    }
}