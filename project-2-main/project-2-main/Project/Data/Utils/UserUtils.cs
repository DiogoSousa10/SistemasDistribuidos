using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Services
{
    public class UserUtils
    {
        private readonly AppDbContext _dbContext;

        public UserUtils()
        {
            _dbContext = AppDbContext.CreateDbContext();
        }

        public User GetUserById(int id)
        {
            return _dbContext.Users.Find(id);
        }

        public bool Authenticate(string username, string password)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);

            if (user == null)
            {
                return false; // Usuário não encontrado
            }

            if (!user.Password.Equals(password))
            {
                return false; // Senha incorreta
            }

            return true;
        }

        public bool AdminOrNot(string username)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);

            if (user != null && user.Role == "ADMIN")
            {
                return true;
            }
            else if (user != null && user.Role == "OPERATOR")
            {
                return false;
            }
            return false;
        }

        public bool isDomicilioReserved(string nome_domicilio, int num_modalidade)
        {
            var domicilio = _dbContext.Domicilios.SingleOrDefault(d => d.Nome == nome_domicilio);

            if(domicilio == null) { return false; }

            var modalidade = _dbContext.Modalidades.SingleOrDefault(m => m.Megas == num_modalidade);

            if(modalidade == null) { return false; }

            var modalidadesDomicilio = _dbContext.Modalidades_Domicilios.SingleOrDefault(
                md => md.DomiciliosNum_Admin == domicilio.Num_Admin && md.ModalidadesId == modalidade.Id);

            if( modalidadesDomicilio == null) { return false; }

            if( modalidadesDomicilio.Domicilios.Estado != "TERMINADO") { return false; }

            else { return true; }
        }

        public int MakeReserve(string nome_domicilio, int modalidade, string username_atual)
        {
            var modalidadesDomicilio = _dbContext.Modalidades_Domicilios.SingleOrDefault(
                md => md.Domicilios.Nome == nome_domicilio && md.Modalidades.Megas == modalidade);

            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username_atual);

            var reserva = new Reserva();
            reserva.UserId = user.Id;
            reserva.Modalidades_DomiciliosId = modalidadesDomicilio.Id;
            var dom = _dbContext.Domicilios.SingleOrDefault(d => d.Nome == nome_domicilio);
            dom.Estado = "RESERVADO";
            _dbContext.Reserva.Add(reserva);
            _dbContext.SaveChanges();

            return reserva.Modalidades_Domicilios.DomiciliosNum_Admin;
        }

        public bool VerifyUserReserve(string username_atual, int num_admin)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username_atual);

            var reserva = _dbContext.Reserva.SingleOrDefault(
                r => r.Modalidades_Domicilios.DomiciliosNum_Admin == num_admin && user.Id == r.UserId);

            if(reserva != null && user != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsReserved_orDeactivated(int num_admin)
        {
            var domicilio = _dbContext.Domicilios.FirstOrDefault(d => d.Num_Admin == num_admin);
            if(domicilio.Estado == "RESERVADO" || domicilio.Estado == "DESATIVADO")
            {
                return true;
            }
            return false;
        }

        public bool IsActivated(int num_admin)
        {
            var domicilio = _dbContext.Domicilios.FirstOrDefault(d => d.Num_Admin == num_admin);
            if (domicilio.Estado == "ATIVADO")
            {
                return true;
            }
            return false;
        }

        public bool IsDesactivated(int num_admin)
        {
            var domicilio = _dbContext.Domicilios.FirstOrDefault(d => d.Num_Admin == num_admin);
            if (domicilio.Estado == "DESATIVADO")
            {
                return true;
            }
            return false;
        }

        public void ChangeState(int num_admin, string state)
        {
            var domicilio = _dbContext.Domicilios.FirstOrDefault(d => d.Num_Admin == num_admin);
            var reserva_domicilio = _dbContext.Reserva.FirstOrDefault(r => r.Modalidades_Domicilios.DomiciliosNum_Admin == num_admin);
            domicilio.Estado = state;
            _dbContext.SaveChanges();

            if(domicilio.Estado == "TERMINADO")
            {
                _dbContext.Reserva.Remove(reserva_domicilio);
                _dbContext.SaveChanges();
            }
        }

        public bool VerefyNa(int na)
        {
            var domicilio = _dbContext.Domicilios;
            foreach(var dom in domicilio)
            {
                if (dom.Num_Admin == na)
                    return true; 
            }
            return false;
        }

        public string GetState(int na)
        {
            var domicilio = _dbContext.Domicilios;
            foreach (var dom in domicilio)
            {
                if (dom.Num_Admin == na)
                    return dom.Estado;
            }
            return "Erro";
        }

        public List<Domicilio> ShowAllDomi()
        {
            var domicilios = _dbContext.Domicilios.ToList();
            return domicilios;
        }

        public List<Domicilio> ShowAllDomiCanReserve()       //Isto serve para mostrar aos utilizadores as linhas que estao abertas para operar.
        {
            var domicilios = _dbContext.Domicilios.Where(m => m.Estado == "TERMINADO").ToList();
            return domicilios;
        }


        public List<Reserva> ShowAllDomiForUserFunction(string user)
        {
            var userWithReservations = _dbContext.Users
                  .Include(u => u.Reserva)
                      .ThenInclude(r => r.Modalidades_Domicilios)
                          .ThenInclude(md => md.Domicilios)
                  .Include(u => u.Reserva)
                      .ThenInclude(r => r.Modalidades_Domicilios)
                          .ThenInclude(md => md.Modalidades)
                  .FirstOrDefault(u => u.Username == user);


            if (userWithReservations == null)
                return new List<Reserva>();

            var domicilios = new List<Reserva>();

            foreach (var reserva in userWithReservations.Reserva)
            {
                var domicilio = reserva;

                if (reserva.Modalidades_Domicilios.Domicilios.Estado == "RESERVADO" || reserva.Modalidades_Domicilios.Domicilios.Estado == "ATIVADO" || reserva.Modalidades_Domicilios.Domicilios.Estado == "DESATIVADO")
                {
                    domicilios.Add(domicilio);
                }
            }

            return domicilios;
        }


        public List<Reserva> ShowInfoAboutUsers()
        {
            var infoUser = _dbContext.Reserva
            .Include(u => u.User) // Carrega os dados do usuário relacionado
            .Include(u => u.Modalidades_Domicilios) // Carrega os dados da modalidade domiciliar relacionada
                .ThenInclude(md => md.Modalidades) // Carrega os dados da modalidade relacionada
            .Include(u => u.Modalidades_Domicilios) // Carrega os dados da modalidade domiciliar relacionada
                .ThenInclude(md => md.Domicilios) // Carrega os dados do domicílio relacionado
            .ToList();

            return infoUser;
        }
    }
}
