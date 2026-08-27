using ExCSS;
using Microsoft.Extensions.Hosting;
using Portunus.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Portunus.App.Services
{
    public class BackgroundJobService(INotificationService notificationService, VaultService vaultService) : BackgroundService
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly VaultService _vaultService = vaultService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // mude para TimeSpan.FromSeconds(30)!
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. A TENTATIVA OPORTUNISTA:
                    // Se o cofre estiver trancado, o método GetPasswordEntry() deve retornar null 
                    // ou lançar uma exceção (que será capturada no catch abaixo e ignorada).
                    var passwords = _vaultService.GetPasswordEntry();

                    if (passwords != null && passwords.Count > 0)
                    {
                        var today = DateTime.Today;
                        var warningLimit = today.AddDays(3);

                        // O FILTRO (Mantém apenas de hoje até 3 dias no futuro)
                        var expiringPasswords = passwords.Where(p =>
                            p.DateToChangePass.HasValue &&
                            p.DateToChangePass.Value.Date >= today &&
                            p.DateToChangePass.Value.Date <= warningLimit).ToList();

                        if (expiringPasswords.Count > 0)
                        {
                            var baseDir = AppContext.BaseDirectory;
                            var iconPath = Path.Combine(baseDir, "Assets", "Icons", "export", "png", "portune-icon-48.png");

                            foreach (var password in expiringPasswords)
                            {

                                if (!password.DateToChangePass.HasValue)
                                    return;

                                int daysLeft = (password.DateToChangePass.Value.Date - today).Days;

                                string timeMessage = daysLeft switch
                                {
                                    3 => "em 3 dias",
                                    2 => "em 2 dias",
                                    1 => "amanhã",
                                    0 => "ainda hoje",
                                    _ => $"em {daysLeft} dias"
                                };

                                string title = "Atualização de Senha";
                                string body = $"A senha do item '{password.Name}' precisa ser trocada {timeMessage}.";

                                _notificationService.ShowNative(title, body, iconPath);

                                await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
