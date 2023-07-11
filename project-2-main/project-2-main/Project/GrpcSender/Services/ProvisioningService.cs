using Data.Services;
using Grpc.Core;
using System.Text;

namespace GrpcServer.Services
{
    public class ProvisioningService : Provisioning.ProvisioningBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly UserUtils _userService;
        private readonly RabbitConfiguration _rabbitConfig;

        public ProvisioningService(ILogger<UserService> logger, UserUtils userService, RabbitConfiguration rabbitConfig)
        {
            _userService = userService;
            _logger = logger;
            _rabbitConfig = rabbitConfig;
        }


        private static Dictionary<string, object> reserveLocks = new Dictionary<string, object>();

        public override Task<ReserveResponse> Reserve(ReserveRequest request, ServerCallContext context)
        {
            bool is_reserved;
            int numAdmin;
            string estado;

            string key = request.Domicilio.ToString() + "_" + request.Modalidade; // Cria uma chave única para combinação de domicilio e modalidade

            lock (GetLockObject(key)) // Bloqueia usando o objeto de bloqueio correspondente à chave
            {
                is_reserved = _userService.isDomicilioReserved(request.Domicilio, request.Modalidade);

                if (is_reserved)
                {
                    estado = "RESERVADO";
                    numAdmin = _userService.MakeReserve(request.Domicilio, request.Modalidade, request.Username);
                }
                else
                {
                    estado = "NAO RESERVADO";
                    numAdmin = -1; // retornar um -1 em caso de dar erro
                }
            }

            var reply = new ReserveResponse
            {
                Estado = estado,
                NumAdmin = numAdmin,
            };

            return Task.FromResult(reply);
        }

        private object GetLockObject(string key)
        {
            lock (reserveLocks)
            {
                if (!reserveLocks.ContainsKey(key))
                {
                    reserveLocks[key] = new object();
                }
                return reserveLocks[key];
            }
        }


        //public override Task<ActivationResponse> Activation(ActivationRequest request, ServerCallContext context) 
        //{
        //    bool is_activated = _userService.VerifyUserReserve(request.Username, request.NumAdmin);
        //    bool is_available = _userService.IsReserved_orDeactivated(request.NumAdmin);
        //    var reply = new ActivationResponse();

        //    if (is_activated && is_available) 
        //    {
        //        reply.CanActivate = true;
        //        _userService.ChangeState(request.NumAdmin, "ATIVADO");
        //        reply.EstimatedTime = "2";

        //        string message = "Ativado com Sucesso";
        //        _rabbitConfig.PublishMessage($"ativar.{request.Username}", message);

        //        return Task.FromResult(reply);
        //    }
        //    else
        //    {
        //        reply.CanActivate = false;
        //        reply.EstimatedTime = "0";
        //    }
        //    return Task.FromResult(reply);
        //}

        // ------------------- DIFFERENCE BETWEEN ASYNC AND SYNC ---------------------------// 
        public override async Task<ActivationResponse> Activation(ActivationRequest request, ServerCallContext context)
        {
            bool is_activated = _userService.VerifyUserReserve(request.Username, request.NumAdmin);
            bool is_available = _userService.IsReserved_orDeactivated(request.NumAdmin);
            var reply = new ActivationResponse();

            if (is_activated && is_available)
            {
                string message = "Ativado com Sucesso";
                await _rabbitConfig.PublishMessageAsync($"ativar.{request.Username}", message);

                reply.CanActivate = true;
                _userService.ChangeState(request.NumAdmin, "ATIVADO");
                reply.EstimatedTime = "2";

                return await Task.FromResult(reply);
            }
            else
            {
                reply.CanActivate = false;
                reply.EstimatedTime = "0";
            }
            return await Task.FromResult(reply);
        }

        public override Task<ActivationAdminResponse> ActivationAdmin(ActivationAdminRequest request, ServerCallContext context)
        {
            var reply = new ActivationAdminResponse();

            _userService.ChangeState(request.Na, "ATIVADO");
            reply.EstimatedTime = "2";

            return Task.FromResult(reply);
        }

        public override Task<DesactivationResponse> Desactivation(DesactivationRequest request, ServerCallContext context)
        {
            bool is_activated = _userService.VerifyUserReserve(request.Username, request.NumAdmin);
            bool is_available = _userService.IsActivated(request.NumAdmin);
            var reply = new DesactivationResponse();

            if (is_activated && is_available)
            {
                string message = "Desativado com Sucesso";
                _rabbitConfig.PublishMessage($"desativar.{request.Username}", message);

                reply.CanDeactivate = true;
                _userService.ChangeState(request.NumAdmin, "DESATIVADO");
                reply.EstimatedTime = "2";
            }
            else
            {
                reply.CanDeactivate = false;
                reply.EstimatedTime = "0";
            }
            return Task.FromResult(reply);
        }

        public override Task<DesactivationAdminResponse> DesactivationAdmin(DesactivationAdminRequest request, ServerCallContext context)
        {
            var reply = new DesactivationAdminResponse();
            _userService.ChangeState(request.Na, "DESATIVADO");
            reply.EstimatedTime = "2";

            return Task.FromResult(reply);
        }

        public override Task<TerminationResponse> Termination(TerminationRequest request, ServerCallContext context)
        {
            bool is_activated = _userService.VerifyUserReserve(request.Username, request.NumAdmin);
            bool is_available = _userService.IsDesactivated(request.NumAdmin);
            var reply = new TerminationResponse();

            if (is_activated && is_available)
            {
                string message = "Terminado com Sucesso";
                _rabbitConfig.PublishMessage($"terminar.{request.Username}", message);

                reply.CanDeactivate = true;
                _userService.ChangeState(request.NumAdmin, "TERMINADO");
                reply.EstimatedTime = "2";
            }
            else
            {
                reply.CanDeactivate = false;
                reply.EstimatedTime = "0";
            }
            return Task.FromResult(reply);
        }
        public override Task<TerminationAdminResponse> TerminationAdmin(TerminationAdminRequest request, ServerCallContext context)
        {
            var reply = new TerminationAdminResponse();
            _userService.ChangeState(request.Na, "TERMINADO");
            reply.EstimatedTime = "2";

            return Task.FromResult(reply);
        }

        public override Task<VerifyNaResponse> VerifyNa(VerifyNaRequest request, ServerCallContext context)
        {
            var reply = new VerifyNaResponse();
            var isitokay = _userService.VerefyNa(request.Na);
            var state = _userService.GetState(request.Na);
            reply.State = state;

            if (isitokay)
                reply.Allgood = true;
            else 
                reply.Allgood = false;

            return Task.FromResult(reply);
        }


        //Buscar todos os domicilios
        public override async Task ShowAllDomi(ShowAllRequest request, IServerStreamWriter<Domicilio> responseStream, ServerCallContext context)
        {
            var domicilios = _userService.ShowAllDomi();

            foreach (var dom in domicilios)
            {
                var domicilio = new Domicilio
                {
                    NumAdmin = dom.Num_Admin,
                    Estado = dom.Estado,
                    Nome = dom.Nome
                };

                await responseStream.WriteAsync(domicilio);
            }
        }

        //Buscar todos os domicilios com estado terminado
        public override async Task ShowAllDomiTerminatedState(ShowAllDomiTerminatedStateRequest request, IServerStreamWriter<Domicilio> responseStream, ServerCallContext context)
        {
            var domicilios = _userService.ShowAllDomiCanReserve();

           foreach (var dom in domicilios)
            {
                var domicilio = new Domicilio
                {
                    NumAdmin = dom.Num_Admin,
                    Estado = dom.Estado,
                    Nome = dom.Nome
                };

                await responseStream.WriteAsync(domicilio);
            }
        }

        //Função para ir buscar todos os domicilios que um user ja atuou sobre ele
        public override async Task ShowDomiWithModali(ShowDomiWithModaliRequest request, IServerStreamWriter<Reserva> responseStream, ServerCallContext context)
        {
            var domicilios = _userService.ShowAllDomiForUserFunction(request.Username);

            foreach (var dom in domicilios)
            {
                var domicilio = new Reserva
                {
                    NumAdmin = dom.Modalidades_Domicilios.Domicilios.Num_Admin,
                    Estado = dom.Modalidades_Domicilios.Domicilios.Estado,
                    Nome = dom.Modalidades_Domicilios.Domicilios.Nome,
                    Modalidade = dom.Modalidades_Domicilios.Modalidades.Megas
                };

                await responseStream.WriteAsync(domicilio);
            }
        }

        //Buscar todos os dados de cada utilizador:

        public override async Task ShowAllInfoAboutUser(ShowAllInfoAboutUserRequest request, IServerStreamWriter<ReservaInfo> responseStream, ServerCallContext context)
        {
            var infoUsuarios = _userService.ShowInfoAboutUsers();

            foreach (var reserva in infoUsuarios)
            {
                if (reserva.User != null && reserva.Modalidades_Domicilios != null && reserva.Modalidades_Domicilios.Domicilios != null)
                {
                    var reservaInfo = new ReservaInfo
                    {
                        Username = reserva.User.Username,
                        Role = reserva.User.Role,
                        NumAdmin = reserva.Modalidades_Domicilios.DomiciliosNum_Admin,
                        Estado = reserva.Modalidades_Domicilios.Domicilios.Estado,
                        Nome = reserva.Modalidades_Domicilios.Domicilios.Nome,
                        Modalidade = reserva.Modalidades_Domicilios.Modalidades.Megas
                    };

                    await responseStream.WriteAsync(reservaInfo);
                }
            }
        }

    }
}
