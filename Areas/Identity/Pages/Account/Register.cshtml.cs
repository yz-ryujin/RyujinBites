using System; // Para tipos básicos como DateTime, Exception
using System.Collections.Generic; // Para usar IList<T>
using System.ComponentModel.DataAnnotations; // Para atributos como [Required], [Display], [StringLength]
using System.Linq; // Para extensões LINQ como .ToList()
using System.Text; // Para Encoding.UTF8
using System.Text.Encodings.Web; // Para HtmlEncoder.Default
using System.Threading; // Para CancellationToken
using System.Threading.Tasks; // Para Task e async/await
using Microsoft.AspNetCore.Authentication; // Para AuthenticationScheme (login externo)
using Microsoft.AspNetCore.Authorization; // Para o atributo [AllowAnonymous]
using Microsoft.AspNetCore.Identity; // Para classes como UserManager, SignInManager, IdentityRole
using Microsoft.AspNetCore.Identity.UI.Services; // Para a interface IEmailSender
using Microsoft.AspNetCore.Mvc; // Para classes como PageModel, IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages; // Para herdar de PageModel
using Microsoft.AspNetCore.WebUtilities; // Para WebEncoders
using Microsoft.Extensions.Logging; // Para ILogger e logging

using RyujinBites.Models.Identity; // Importa o namespace onde sua classe ApplicationUser está definida

namespace RyujinBites.Areas.Identity.Pages.Account
{
    // Atributo que permite que esta página seja acessada por usuários não autenticados (visitantes).
    // O registro é um processo de pré-autenticação.
    [AllowAnonymous]
    public class RegisterModel : PageModel // Define esta classe como um modelo de página Razor
    {
        // Campos somente leitura ('readonly') para injeção de dependência dos serviços do Identity.
        // São instâncias que serão fornecidas automaticamente pelo ASP.NET Core.
        private readonly SignInManager<ApplicationUser> _signInManager; // Gerencia o processo de login/logout do usuário.
        private readonly UserManager<ApplicationUser> _userManager;     // Gerencia usuários (criação, senhas, roles, etc.).
        private readonly IUserStore<ApplicationUser> _userStore;       // Abstração para persistir usuários.
        private readonly IUserEmailStore<ApplicationUser> _emailStore; // Abstração para persistir emails de usuário.
        private readonly ILogger<RegisterModel> _logger;               // Para registrar mensagens de log (erros, informações).
        private readonly IEmailSender _emailSender;                   // Interface para enviar e-mails (confirmação, recuperação de senha).
        private readonly RoleManager<IdentityRole> _roleManager;       // Gerencia papéis (roles) no Identity (criação, atribuição).

        // Construtor da classe RegisterModel.
        // Aqui, o ASP.NET Core injeta as dependências que a página precisa.
        public RegisterModel(
            UserManager<ApplicationUser> userManager,     // Injeta o gerenciador de usuários.
            IUserStore<ApplicationUser> userStore,       // Injeta a interface de persistência de usuário.
            SignInManager<ApplicationUser> signInManager, // Injeta o gerenciador de login/logout.
            ILogger<RegisterModel> logger,               // Injeta o serviço de logging.
            IEmailSender emailSender,                   // Injeta o serviço de envio de e-mail.
            RoleManager<IdentityRole> roleManager)       // Injeta o gerenciador de papéis (ADICIONADO POR NÓS).
        {
            _userManager = userManager;     // Atribui a instância injetada ao campo _userManager.
            _userStore = userStore;         // Atribui a instância injetada ao campo _userStore.
            _emailStore = GetEmailStore();  // Obtém o store de e-mail a partir do userStore (método auxiliar).
            _signInManager = signInManager; // Atribui a instância injetada ao campo _signInManager.
            _logger = logger;               // Atribui a instância injetada ao campo _logger.
            _emailSender = emailSender;     // Atribui a instância injetada ao campo _emailSender.
            _roleManager = roleManager;     // Atribui a instância injetada ao campo _roleManager (ADICIONADO POR NÓS).
        }

        // Propriedade que armazena os dados de entrada do formulário de registro.
        // [BindProperty] faz com que o ASP.NET Core automaticamente popule esta propriedade
        // com os dados do formulário HTTP POST.
        [BindProperty]
        public InputModel Input { get; set; }

        // Armazena a URL para a qual o usuário deve ser redirecionado após o registro bem-sucedido.
        public string ReturnUrl { get; set; }

        // Uma lista de esquemas de autenticação externa (ex: Google, Facebook) disponíveis.
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Classe aninhada que define o modelo de dados esperado do formulário de entrada.
        // As propriedades aqui correspondem aos campos que o usuário preenche na UI.
        public class InputModel
        {
            [Required] // Campo obrigatório.
            [EmailAddress] // Valida que o input é um endereço de e-mail válido.
            [Display(Name = "Email")] // O rótulo que será exibido na UI.
            public string Email { get; set; } = default!; // Propriedade para o e-mail do usuário.

            [Required] // Campo obrigatório.
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            // Define o tamanho mínimo (6) e máximo (100) da string. {0}, {1}, {2} são placeholders.
            [DataType(DataType.Password)] // Indica que este campo é uma senha (para UI/autocompletar).
            [Display(Name = "Senha")] // O rótulo na UI.
            public string Password { get; set; } = default!; // Propriedade para a senha.

            [DataType(DataType.Password)] // Indica que este campo é uma senha.
            [Display(Name = "Confirmar Senha")] // O rótulo na UI.
            [Compare("Password", ErrorMessage = "A senha e a senha de confirmação não correspondem.")]
            // Compara com a propriedade 'Password' para garantir que as senhas digitadas são iguais.
            public string ConfirmPassword { get; set; } = default!; // Propriedade para a confirmação de senha.

            [Required] // Campo obrigatório.
            [Display(Name = "Nome Completo")] // O rótulo na UI.
            public string Nome { get; set; } = string.Empty; // Propriedade para o nome completo do usuário (ADICIONADO POR NÓS).
        }

        // Método que é executado quando a página é carregada (requisição HTTP GET).
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Define a URL de retorno, usando a URL padrão "~/" se não for fornecida.
            ReturnUrl = returnUrl;
            // Obtém os esquemas de autenticação externa disponíveis (ex: Google, Facebook).
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // Método que é executado quando o formulário de registro é submetido (requisição HTTP POST).
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Define a URL de retorno, usando a URL padrão "~/" se não for fornecida.
            returnUrl ??= Url.Content("~/");
            // Obtém os esquemas de autenticação externa disponíveis.
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Verifica se os dados do modelo (Input) são válidos de acordo com as Data Annotations.
            if (ModelState.IsValid)
            {
                var user = CreateUser(); // Cria uma nova instância da sua classe ApplicationUser.

                // Define o nome de usuário (geralmente o e-mail) no objeto de usuário.
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                // Define o e-mail do usuário no objeto de usuário.
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // --> ADIÇÕES DE PROPRIEDADES PERSONALIZADAS AO USUÁRIO ANTES DA CRIAÇÃO <--
                user.Nome = Input.Nome; // Atribui o nome fornecido no formulário ao usuário.
                user.DataRegistro = DateTime.UtcNow; // Define a data de registro do usuário (hora UTC).
                user.EmailConfirmed = true; // Define o e-mail como confirmado automaticamente (para testes em dev).
                // -------------------------------------------------------------------------

                // Tenta criar o usuário no sistema de Identity com a senha fornecida.
                var result = await _userManager.CreateAsync(user, Input.Password);

                // Verifica se a criação do usuário foi bem-sucedida.
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password."); // Registra o sucesso.

                    // *** LÓGICA PARA ATRIBUIR O PAPEL 'CLIENTE' AO NOVO USUÁRIO ***
                    // Verifica se o papel 'Cliente' já existe no banco de dados.
                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        // Se não existe, cria o papel 'Cliente'.
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }
                    // Adiciona o usuário recém-criado ao papel 'Cliente'.
                    await _userManager.AddToRoleAsync(user, "Cliente");
                    // ********************************************

                    // Este bloco de código lida com a confirmação de e-mail.
                    // Se _userManager.Options.SignIn.RequireConfirmedAccount for verdadeiro, o sistema exigirá confirmação.
                    // No entanto, como definimos user.EmailConfirmed = true para testes, este bloco não será executado.
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        var userId = await _userManager.GetUserIdAsync(user); // Obtém o ID do usuário.
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user); // Gera um token de confirmação.
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); // Codifica o token para URL.
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",      // Página de confirmação de e-mail.
                            pageHandler: null,           // Sem handler específico.
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl }, // Valores para a URL.
                            protocol: Request.Scheme);   // Protocolo (HTTP/HTTPS).

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", // Envia o e-mail de confirmação.
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl }); // Redireciona para página de confirmação.
                    }
                    else // Se a confirmação de e-mail não for exigida ou já estiver confirmada (como em nosso caso de dev).
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false); // Faz o login do usuário recém-registrado.
                        return LocalRedirect(returnUrl); // Redireciona para a URL de retorno.
                    }
                }
                // Se a criação do usuário não foi bem-sucedida, itera sobre os erros e os adiciona ao ModelState.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); // Adiciona erros ao modelo para exibição na UI.
                }
            }

            // Se chegamos até aqui, algo falhou, e o formulário é exibido novamente com mensagens de erro.
            return Page();
        }

        // Método auxiliar para criar uma instância da classe ApplicationUser.
        private ApplicationUser CreateUser()
        {
            try
            {
                // Tenta criar uma nova instância de ApplicationUser usando o construtor padrão.
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                // Captura exceções se a criação falhar (ex: construtor sem parâmetros ausente).
                throw new InvalidOperationException($"Não é possível criar uma instância de '{nameof(ApplicationUser)}'. " +
                    $"Certifique-se de que '{nameof(ApplicationUser)}' não é uma classe abstrata e tem um construtor sem parâmetros, ou " +
                    $"alternativamente sobrescreva a página de registro em /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Método auxiliar para obter o store de e-mail a partir do userStore.
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            // Verifica se o userManager suporta e-mails de usuário.
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("A UI padrão requer um user store com suporte a e-mail.");
            }
            // Retorna o userStore convertido para IUserEmailStore.
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}