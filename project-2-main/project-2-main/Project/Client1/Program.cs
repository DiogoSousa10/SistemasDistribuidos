using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using GrpcServer.Protos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Client1
{
    class Program
    {

        static void Main(string[] args)
        {
            string exchangeName = "NOME_DA_EXCHANGE";
            string routingKey = "NOME_ROUTING_KEY";
            string activateQueueName = "";
            string deactivateQueueName = "";
            string terminateQueueName = "";

            var channel = GrpcChannel.ForAddress("http://25.43.242.182:5114");
            var client = new User.UserClient(channel);

            var rabbitConfig = new RabbitConfiguration(exchangeName, routingKey);

            var username = "";
            var admin = false;
            var logged = false;
            var firsttime = false;

            while (!logged)
            {
                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine("Bem vindo Cliente1, por favor insira as suas credenciais!");
                Console.WriteLine("----------------------------------------------------------------");

                if (firsttime) Console.WriteLine("Utilizador ou Palavra-passe errados, por favor tente novamente!");
                firsttime = true;

                Console.WriteLine("\nInsira o Username: ");
                username = Console.ReadLine();
                Console.WriteLine("Insira a Password: ");
                var password = Console.ReadLine();

                var client_request = new UserNameRequest { Username = username, Password = password };
                var client_response = client.GetAuthenticate(client_request);

                if (client_response.State)
                {
                    logged = true;
                    if (client_response.Admin) admin = true;

                    activateQueueName = $"ativar." + username;
                    deactivateQueueName = $"desativar." + username;
                    terminateQueueName = $"terminar." + username;

                    rabbitConfig.SetupQueue(activateQueueName);
                    rabbitConfig.BindQueue(activateQueueName);

                    rabbitConfig.SetupQueue(deactivateQueueName);
                    rabbitConfig.BindQueue(deactivateQueueName);

                    rabbitConfig.SetupQueue(terminateQueueName);
                    rabbitConfig.BindQueue(terminateQueueName);
                }
                else
                {
                    Console.Clear();
                }
            }

            var client_reserve = new Provisioning.ProvisioningClient(channel);

            /////////////////////////////////PARA ADMINS APENAS////////////////////////////////////
            /////////////////////////////////PARA ADMINS APENAS////////////////////////////////////
            /////////////////////////////////PARA ADMINS APENAS////////////////////////////////////
            /////////////////////////////////PARA ADMINS APENAS////////////////////////////////////
            /////////////////////////////////PARA ADMINS APENAS////////////////////////////////////

            if (admin)
            {
                while (true)
                {
                    Console.Clear();

                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("Bem vindo Admistrador " + username + "!");
                    Console.WriteLine("----------------------------------------------------\n");

                    Console.WriteLine("1. Mostrar a cobertura disponivel");
                    Console.WriteLine("2. Mostrar os dados referentes a todos os processos em curso");
                    Console.WriteLine("3. Mostrar todas as informaçoes de cada utilizador");
                    Console.WriteLine("4. Agir como Cliente");
                    Console.WriteLine("5. Sair\n");

                    var decision = Console.ReadLine();

                    if (decision == "1")
                    {
                        ///////////////////////////MOSTRA TODOS OS DOMICILIOS QUE ESTAO NA BD, CASO SEJA O ADMIN////////////////////////////////////

                        var repeat = 1;
                        while (repeat == 1)
                        {

                            Console.Clear();

                            Console.WriteLine("----------------------------------------------------");
                            Console.WriteLine("Bem vindo Admistrador " + username + "!");
                            Console.WriteLine("----------------------------------------------------\n");

                            var request = new ShowAllRequest();

                            using var call = client_reserve.ShowAllDomi(request);

                            var task = Task.Run(async () =>
                            {
                                await foreach (var domicilio in call.ResponseStream.ReadAllAsync())
                                {
                                    Console.WriteLine($"Número Admin: {domicilio.NumAdmin}, Estado: {domicilio.Estado}, Nome: {domicilio.Nome}");
                                }
                            });

                            ///////////////////////////MOSTRA TODOS OS DOMICILIOS QUE ESTAO NA BD, CASO SEJA O ADMIN////////////////////////////////////

                            Console.WriteLine("Lista de toda a cobertura:");
                            Console.WriteLine("-----------------------------------------------------------------------------------");
                            task.GetAwaiter().GetResult();
                            Console.WriteLine("-----------------------------------------------------------------------------------\n");


                            Console.WriteLine("Pretende mudar o estado de algum domicilio? Se sim diga o numero administrativo, se não escreva \"SAIR\"");
                            var decision2 = Console.ReadLine();
                            if (int.TryParse(decision2, out int n))
                            {
                                var verifyna = new VerifyNaRequest
                                {
                                    Na = n
                                };
                                var verifyna_response = client_reserve.VerifyNa(verifyna);
                                if (verifyna_response.Allgood && verifyna_response.State != "TERMINADO")
                                {
                                    var repeat2 = 1;
                                    while (repeat2 == 1)
                                    {
                                        Console.WriteLine("---------------");
                                        Console.WriteLine("1. Ativar");
                                        Console.WriteLine("2. Desativar");
                                        Console.WriteLine("3. Terminar");
                                        Console.WriteLine("4. Sair");
                                        Console.WriteLine("---------------");

                                        var decision3 = Console.ReadLine();

                                        if (decision3 == "1")
                                        {
                                            var activate_admin = new ActivationAdminRequest { Na = n };
                                            var activate_admin_response = client_reserve.ActivationAdmin(activate_admin);
                                            repeat2 = 0;
                                            Console.WriteLine("Sucesso! O estado do domicilio foi alterado para \"ATIVADO\"");
                                        }
                                        else if (decision3 == "2")
                                        {
                                            var desactivation_admin = new DesactivationAdminRequest { Na = n };
                                            var desactivation_admin_response = client_reserve.DesactivationAdmin(desactivation_admin);
                                            repeat2 = 0;
                                            Console.WriteLine("Sucesso! O estado do domicilio foi alterado para \"DESATIVADO\"");
                                        }
                                        else if (decision3 == "3")
                                        {
                                            var termination_admin = new TerminationAdminRequest { Na = n };
                                            var termination_admin_response = client_reserve.TerminationAdmin(termination_admin);
                                            repeat2 = 0;
                                            Console.WriteLine("Sucesso! O estado do domicilio foi alterado para \"TERMINADO\"");
                                        }
                                        else if (decision3 == "4")
                                        {
                                            repeat2 = 0;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Erro! Por favor, apenas use um numero de 1 a 5!");
                                        }
                                        Thread.Sleep(2000);
                                    }
                                }
                                else
                                {
                                    if (verifyna_response.Allgood)
                                        Console.WriteLine("Este já domicilio se encontra terminado e sem reserva!");
                                    else
                                        Console.WriteLine("Numero administrativo não existente!");
                                }
                            }
                            else if (decision2.ToUpper() == "SAIR")
                            {
                                repeat = 0;
                            }
                            else
                            {
                                Console.WriteLine("Resposta inválida! Por favor tente novamente!");
                                Thread.Sleep(2000);
                            }
                        }
                    }
                    if (decision == "2")
                    {
                        //rabbitConfig.ListQueueMessages();
                    }
                    if (decision == "3")
                    {
                        Console.Clear();

                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("Bem vindo Admistrador " + username + "!");

                        var request = new ShowAllInfoAboutUserRequest();

                        using var call = client_reserve.ShowAllInfoAboutUser(request);
                        var task = Task.Run(async () =>
                        {
                            await foreach (var reservaInfo in call.ResponseStream.ReadAllAsync())
                            {
                                Console.WriteLine($"Username: {reservaInfo.Username}\nRole: {reservaInfo.Role}\nNumAdmin: {reservaInfo.NumAdmin}\n" +
                                $"Estado: {reservaInfo.Estado}\nNome: {reservaInfo.Nome}\nModalidade: {reservaInfo.Modalidade}\n");
                            }
                        });
                        Console.WriteLine("---------------------------------------------------------");
                        task.GetAwaiter().GetResult();
                        Console.WriteLine("---------------------------------------------------------");

                        Console.WriteLine("Press Enter to continue");
                        Console.ReadLine();

                    }
                    if (decision == "4")
                    {
                        var firstime = 1;
                        var completed1 = 0;
                        while (completed1 == 0)
                        {
                            if (firstime == 1)
                            {
                                Console.Clear();
                                Console.WriteLine("----------------------------------------------------");
                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                Console.WriteLine("----------------------------------------------------\n");
                                firstime = 0;
                            }
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                            var showDomiForUser = new ShowDomiWithModaliRequest
                            {
                                Username = username
                            };

                            var call = client_reserve.ShowDomiWithModali(showDomiForUser);

                            var task = Task.Run(async () =>
                            {
                                Console.WriteLine("Historico de domicilios");
                                Console.WriteLine("---------------------------------------------------------");
                                await foreach (var domicilio in call.ResponseStream.ReadAllAsync())
                                {
                                    Console.WriteLine($"Número Admin: {domicilio.NumAdmin}, Estado: {domicilio.Estado}, Nome: {domicilio.Nome}, Modalidade: {domicilio.Modalidade}");
                                }
                                Console.WriteLine("---------------------------------------------------------");
                            });
                            task.Wait();


                            var request = new ShowAllDomiTerminatedStateRequest();

                            using var callall = client_reserve.ShowAllDomiTerminatedState(request);

                            var taskall = Task.Run(async () =>
                            {
                                await foreach (var domicilio in callall.ResponseStream.ReadAllAsync())
                                {
                                    Console.WriteLine($"Número Admin: {domicilio.NumAdmin}, Estado: {domicilio.Estado}, Nome: {domicilio.Nome}");
                                }
                            });
                            Console.WriteLine("\nLista de toda a cobertura disponivel:");
                            Console.WriteLine("---------------------------------------------------------");
                            task.GetAwaiter().GetResult();
                            taskall.Wait();

                            ///////////////////////////MOSTRA OS DOMICILIOS QUE CADA USER JA TEVE ACESSO////////////////////////////////////

                            Console.WriteLine("---------------------------------------------------------");
                            Console.WriteLine("1. Reservar");
                            Console.WriteLine("2. Ativar");
                            Console.WriteLine("3. Desativar");
                            Console.WriteLine("4. Terminar");
                            Console.WriteLine("5. Sair");
                            Console.WriteLine("---------------------------------------------------------");
                            var decision2 = Console.ReadLine();

                            if (decision2 == "1") // Reservar
                            {
                                var leave = 0;
                                var completed = 0;
                                while (completed == 0)
                                {
                                    Console.WriteLine("Por favor insire o Domicilio que pretende reservar (SAIR para voltar ao menu anterior):");
                                    var dom = Console.ReadLine();

                                    if (dom != null && dom.ToUpper() == "SAIR")
                                    {
                                        leave = 1;
                                        completed = 1;
                                    }

                                    if (leave == 0)
                                    {

                                        Console.WriteLine("Agora insire a modalidade:");
                                        var mod = Console.ReadLine();
                                        var num_mod = 0;
                                        try
                                        {
                                            num_mod = Int32.Parse(mod);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Erro! Modalidade não é um numero!");
                                        }

                                        if (num_mod != 0)
                                        {
                                            var reserve_request = new ReserveRequest
                                            {
                                                Domicilio = dom,
                                                Modalidade = num_mod,
                                                Username = username
                                            };
                                            var reserve_response = client_reserve.Reserve(reserve_request);

                                            if (reserve_response.Estado == "NAO RESERVADO")
                                            {
                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Erro! Domicilio/Modalidade não existente!");
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                            else
                                            {
                                                completed = 1;
                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Sucesso! O Domicilio dado encontra-se no estado \"" + reserve_response.Estado + "\" e o numero administrativo é " + reserve_response.NumAdmin);
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (decision2 == "2") // Ativar
                            {
                                var leave = 0;
                                var completed = 0;

                                while (completed == 0)
                                {
                                    Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                                    var na = Console.ReadLine();

                                    if (na != null && na.ToUpper() == "SAIR")
                                    {
                                        leave = 1;
                                        completed = 1;
                                    }

                                    if (leave == 0)
                                    {
                                        var na_n = 0;
                                        try
                                        {
                                            na_n = Int32.Parse(na);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                        }

                                        if (na_n != 0)
                                        {

                                            var activation_request = new ActivationRequest
                                            {
                                                NumAdmin = na_n,
                                                Username = username
                                            };
                                            var activation_response = client_reserve.Activation(activation_request);

                                            if (activation_response.CanActivate)
                                            {
                                                completed = 1;

                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Sucesso! O seu domicilio está em processo de ativação, o tempo estimado será de " + activation_response.EstimatedTime + " dias!");

                                                rabbitConfig.ConsumeMessages(Console.WriteLine, activateQueueName);
                                                Thread.Sleep(500);
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                            else
                                            {
                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou já se encontra ativado!");
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (decision2 == "3") // Desativar
                            {
                                var leave = 0;
                                var completed = 0;

                                while (completed == 0)
                                {
                                    Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                                    var na = Console.ReadLine();

                                    if (na != null && na.ToUpper() == "SAIR")
                                    {
                                        leave = 1;
                                        completed = 1;
                                    }

                                    if (leave == 0)
                                    {
                                        var na_n = 0;
                                        try
                                        {
                                            na_n = Int32.Parse(na);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                        }

                                        if (na_n != 0)
                                        {

                                            var desactivation_request = new DesactivationRequest
                                            {
                                                NumAdmin = na_n,
                                                Username = username
                                            };
                                            var desactivation_response = client_reserve.Desactivation(desactivation_request);

                                            if (desactivation_response.CanDeactivate)
                                            {
                                                completed = 1;

                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Sucesso! O seu domicilio está em processo de desativação, o tempo estimado será de " + desactivation_response.EstimatedTime + " dias!");

                                                rabbitConfig.ConsumeMessages(Console.WriteLine, deactivateQueueName);
                                                Thread.Sleep(500);
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                            else
                                            {
                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou já se encontra desativado!");
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (decision2 == "4") // Terminar
                            {
                                var leave = 0;
                                var completed = 0;

                                while (completed == 0)
                                {
                                    Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                                    var na = Console.ReadLine();

                                    if (na != null && na.ToUpper() == "SAIR")
                                    {
                                        leave = 1;
                                        completed = 1;
                                    }

                                    if (leave == 0)
                                    {
                                        var na_n = 0;
                                        try
                                        {
                                            na_n = Int32.Parse(na);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                        }

                                        if (na_n != 0)
                                        {

                                            var termination_request = new TerminationRequest
                                            {
                                                NumAdmin = na_n,
                                                Username = username
                                            };
                                            var termination_response = client_reserve.Termination(termination_request);

                                            if (termination_response.CanDeactivate)
                                            {
                                                completed = 1;

                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Sucesso! O seu domicilio está em processo de terminação, o tempo estimado será de " + termination_response.EstimatedTime + " dias!");

                                                rabbitConfig.ConsumeMessages(Console.WriteLine, terminateQueueName);
                                                Thread.Sleep(500);
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                            else
                                            {
                                                Console.Clear();
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Bem vindo Admistrador " + username + "!");
                                                Console.WriteLine("----------------------------------------------------");
                                                Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou se encontra ativado! Se este for o caso por favor desative primeiro");
                                                Console.WriteLine("----------------------------------------------------\n");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (decision2 == "5")
                            {
                                completed1 = 1;
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("---------------------------------------------------------");
                                Console.WriteLine("Bem vindo Operador " + username + "!");
                                Console.WriteLine("---------------------------------------------------------");
                                Console.WriteLine("Erro! Por favor, apenas use um numero de 1 a 5!");
                                Console.WriteLine("---------------------------------------------------------\n");
                            }
                        }
                    }
                    else if (decision == "5") //Sair
                    {
                        Console.Clear();
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Adeus! Volte sempre! o/");
                        Console.WriteLine("---------------------------------------------------------");
                        Thread.Sleep(3000);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Erro! Por favor, apenas use um numero de 1 a 5!");
                    }
                }

            }
            else
            {
                while (true)
                {
                    ///////////////////////////MOSTRA OS DOMICILIOS QUE CADA USER JA TEVE ACESSO////////////////////////////////////
                    var firstime = 1;
                    if (firstime == 1)
                    {
                        Console.Clear();
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("Bem vindo Operador " + username + "!");
                        Console.WriteLine("----------------------------------------------------\n");
                        firstime = 0;
                    }

                    var showDomiForUser = new ShowDomiWithModaliRequest
                    {
                        Username = username
                    };

                    var call = client_reserve.ShowDomiWithModali(showDomiForUser);

                    var task = Task.Run(async () =>
                    {
                        Console.WriteLine("Historico de domicilios");
                        Console.WriteLine("---------------------------------------------------------");
                        await foreach (var domicilio in call.ResponseStream.ReadAllAsync())
                        {
                            Console.WriteLine($"Número Admin: {domicilio.NumAdmin}, Estado: {domicilio.Estado}, Nome: {domicilio.Nome}, Modalidade: {domicilio.Modalidade}");
                        }
                        Console.WriteLine("---------------------------------------------------------");
                    });
                    task.Wait();


                    var request = new ShowAllDomiTerminatedStateRequest();

                    using var callall = client_reserve.ShowAllDomiTerminatedState(request);

                    var taskall = Task.Run(async () =>
                    {
                        await foreach (var domicilio in callall.ResponseStream.ReadAllAsync())
                        {
                            Console.WriteLine($"Número Admin: {domicilio.NumAdmin}, Estado: {domicilio.Estado}, Nome: {domicilio.Nome}");
                        }
                    });
                    Console.WriteLine("\nLista de toda a cobertura disponivel:");
                    Console.WriteLine("---------------------------------------------------------");
                    task.GetAwaiter().GetResult();
                    taskall.Wait();


                    ///////////////////////////MOSTRA OS DOMICILIOS QUE CADA USER JA TEVE ACESSO////////////////////////////////////


                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine("1. Reservar");
                    Console.WriteLine("2. Ativar");
                    Console.WriteLine("3. Desativar");
                    Console.WriteLine("4. Terminar");
                    Console.WriteLine("5. Sair");
                    Console.WriteLine("---------------------------------------------------------");
                    var decision = Console.ReadLine();

                    if (decision == "1") // Reservar
                    {
                        var leave = 0;
                        var completed = 0;
                        while (completed == 0)
                        {
                            Console.WriteLine("Por favor insire o Domicilio que pretende reservar (SAIR para voltar ao menu anterior):");
                            var dom = Console.ReadLine();

                            if (dom != null && dom.ToUpper() == "SAIR")
                            {
                                leave = 1;
                                completed = 1;
                            }

                            if (leave == 0)
                            {

                                Console.WriteLine("Agora insire a modalidade (50, 100, 200, 300, 400, 500 MEGABYTES):");
                                var mod = Console.ReadLine();
                                var num_mod = 0;
                                try
                                {
                                    num_mod = Int32.Parse(mod);
                                }
                                catch
                                {
                                    Console.WriteLine("Erro! Modalidade não é um numero!");
                                }

                                if (num_mod != 0)
                                {
                                    var reserve_request = new ReserveRequest
                                    {
                                        Domicilio = dom,
                                        Modalidade = num_mod,
                                        Username = username
                                    };
                                    var reserve_response = client_reserve.Reserve(reserve_request);

                                    if (reserve_response.Estado == "NAO RESERVADO")
                                    {
                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Erro! Domicilio/Modalidade não existente!");
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                    else
                                    {
                                        completed = 1;

                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Sucesso! O Domicilio dado encontra-se no estado \"" + reserve_response.Estado + "\" e o numero administrativo é " + reserve_response.NumAdmin);
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                }
                            }
                        }
                    }
                    else if (decision == "2") // Ativar
                    {
                        var leave = 0;
                        var completed = 0;

                        while (completed == 0)
                        {
                            Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                            var na = Console.ReadLine();

                            if (na != null && na.ToUpper() == "SAIR")
                            {
                                leave = 1;
                                completed = 1;
                            }

                            if (leave == 0)
                            {
                                var na_n = 0;
                                try
                                {
                                    na_n = Int32.Parse(na);
                                }
                                catch
                                {
                                    Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                }

                                if (na_n != 0)
                                {

                                    var activation_request = new ActivationRequest
                                    {
                                        NumAdmin = na_n,
                                        Username = username
                                    };
                                    var activation_response = client_reserve.Activation(activation_request);

                                    if (activation_response.CanActivate)
                                    {
                                        completed = 1;

                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Sucesso! O seu domicilio está em processo de ativação, o tempo estimado será de " + activation_response.EstimatedTime + " dias!");

                                        rabbitConfig.ConsumeMessages(Console.WriteLine, activateQueueName);
                                        Thread.Sleep(500);
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou já se encontra ativado!");
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                }
                            }
                        }
                    }
                    else if (decision == "3") // Desativar
                    {
                        var leave = 0;
                        var completed = 0;

                        while (completed == 0)
                        {
                            Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                            var na = Console.ReadLine();

                            if (na != null && na.ToUpper() == "SAIR")
                            {
                                leave = 1;
                                completed = 1;
                            }

                            if (leave == 0)
                            {
                                var na_n = 0;
                                try
                                {
                                    na_n = Int32.Parse(na);
                                }
                                catch
                                {
                                    Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                }

                                if (na_n != 0)
                                {

                                    var desactivation_request = new DesactivationRequest
                                    {
                                        NumAdmin = na_n,
                                        Username = username
                                    };
                                    var desactivation_response = client_reserve.Desactivation(desactivation_request);

                                    if (desactivation_response.CanDeactivate)
                                    {
                                        completed = 1;

                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Sucesso! O seu domicilio está em processo de desativação, o tempo estimado será de " + desactivation_response.EstimatedTime + " dias!");

                                        rabbitConfig.ConsumeMessages(Console.WriteLine, deactivateQueueName);
                                        Thread.Sleep(500);
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou já se encontra desativado!");
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                }
                            }
                        }
                    }
                    else if (decision == "4") // Terminar
                    {
                        var leave = 0;
                        var completed = 0;

                        while (completed == 0)
                        {
                            Console.WriteLine("Por favor insire o numero administrativo. (SAIR para voltar ao menu aterior)");
                            var na = Console.ReadLine();

                            if (na != null && na.ToUpper() == "SAIR")
                            {
                                leave = 1;
                                completed = 1;
                            }

                            if (leave == 0)
                            {
                                var na_n = 0;
                                try
                                {
                                    na_n = Int32.Parse(na);
                                }
                                catch
                                {
                                    Console.WriteLine("Erro! Numero Administrativo não é um numero!");
                                }

                                if (na_n != 0)
                                {

                                    var termination_request = new TerminationRequest
                                    {
                                        NumAdmin = na_n,
                                        Username = username
                                    };
                                    var termination_response = client_reserve.Termination(termination_request);

                                    if (termination_response.CanDeactivate)
                                    {
                                        completed = 1;

                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Sucesso! O seu domicilio está em processo de desativação, o tempo estimado será de " + termination_response.EstimatedTime + " dias!");

                                        rabbitConfig.ConsumeMessages(Console.WriteLine, terminateQueueName);
                                        Thread.Sleep(500);
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Bem vindo Operador " + username + "!");
                                        Console.WriteLine("---------------------------------------------------------");
                                        Console.WriteLine("Erro! Domicilio não está reservado no seu nome ou se encontra ativado! Se este for o caso por favor desative primeiro");
                                        Console.WriteLine("---------------------------------------------------------\n");
                                    }
                                }
                            }
                        }
                    }
                    else if (decision == "5")
                    {
                        Console.Clear();
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Adeus! Volte sempre! o/");
                        Console.WriteLine("---------------------------------------------------------");
                        Thread.Sleep(3000);
                        return;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Bem vindo Operador " + username + "!");
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Erro! Por favor, apenas use um numero de 1 a 5!");
                        Console.WriteLine("---------------------------------------------------------\n");
                    }
                }
            }
        }
    }
}
