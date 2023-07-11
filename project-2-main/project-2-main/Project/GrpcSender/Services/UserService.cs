using Data.Services;
using Grpc.Core;
using GrpcServer.Protos;
using static GrpcServer.Protos.User;

namespace GrpcServer.Services
{
    public class UserService : User.UserBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly UserUtils _userService;
        private readonly RabbitConfiguration _rabbitConfig;

        public UserService(ILogger<UserService> logger, UserUtils userService, RabbitConfiguration rabbitConfig)
        {
            _userService = userService;
            _logger = logger;
            _rabbitConfig = rabbitConfig;
        }

        public override Task<UserReply> GetUserById(UserIdRequest request, ServerCallContext context)
        {
            var user = _userService.GetUserById(request.Id);
            var reply = new UserReply { Id = user.Id , Name = user.Username};
            return Task.FromResult(reply);
        }

        public override Task<UserNameReply> GetAuthenticate(UserNameRequest request, ServerCallContext context)
        {
            var is_auth = _userService.Authenticate(request.Username, request.Password);
            var is_admin = _userService.AdminOrNot(request.Username);

            var reply = new UserNameReply();
            if (is_auth) 
            { 
                reply.State = true;

                string activateQueueName = $"ativar.{request.Username}";
                string deactivateQueueName = $"desativar.{request.Username}";
                string terminateQueueName = $"terminar.{request.Username}";

                _rabbitConfig.SetupQueue(activateQueueName);
                _rabbitConfig.BindQueue(activateQueueName);

                _rabbitConfig.SetupQueue(deactivateQueueName);
                _rabbitConfig.BindQueue(deactivateQueueName);

                _rabbitConfig.SetupQueue(terminateQueueName);
                _rabbitConfig.BindQueue(terminateQueueName);
            }
            else 
            { 
                reply.State = false; 
            };

            if (is_admin) { reply.Admin = true; }
            else { reply.Admin = false; };



            return Task.FromResult(reply);
        }
    }
}
