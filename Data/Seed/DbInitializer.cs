using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // Necessário para GetRequiredService
using RyujinBites.Models.Identity; // Necessário para ApplicationUser
using System;
using System.Linq; // Necessário para .Any()
using System.Threading.Tasks; // Necessário para Task

namespace RyujinBites.Data.Seed
{
    public static class DbInitializer // Classe estática para fácil acesso
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Obter as instâncias do RoleManager e UserManager do provedor de serviços
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Criar Papéis (Roles) se não existirem
            string[] roleNames = { "Administrador", "Cliente" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Se o papel não existe, cria-o
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Criar Usuário Administrador Principal se não existir
            var adminUser = await userManager.FindByEmailAsync("admin@ryujinbites.com");
            if (adminUser == null)
            {
                // Se o usuário administrador não existe, cria-o
                adminUser = new ApplicationUser
                {
                    UserName = "admin@ryujinbites.com",
                    Email = "admin@ryujinbites.com",
                    Nome = "Administrador Principal",
                    EmailConfirmed = true, // Define como true para que ele possa logar sem confirmar e-mail
                    DataRegistro = DateTime.UtcNow
                };
                // Tenta criar o usuário com a senha
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123"); // <-- ATENÇÃO: Use uma senha forte e mude em produção!
                if (createAdmin.Succeeded)
                {
                    // Se a criação do usuário for bem-sucedida, atribui o papel de Administrador a ele
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                }
                else
                {
                    // Log de erros caso a criação do admin falhe (ex: validação de senha)
                    // Você pode adicionar um log mais detalhado aqui
                    Console.WriteLine($"Erro ao criar usuário admin: {string.Join(", ", createAdmin.Errors.Select(e => e.Description))}");
                }
            }

            // Você pode adicionar mais dados de seeding aqui, se precisar
        }
    }
}