using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GruppoReti.Entities.Entities;
using GruppoReti.DAL.Repositories;
using GruppoReti.DAL;

namespace GruppoReti.Managers.Managers
{
    public class EmployeeManager
    {

        

        public static bool AddEmployee(Employees NewEmployee)
        {
            EFRepository<Employees> EmployeesRepo = new EFRepository<Employees>();

            EmployeesRepo.Add(NewEmployee);

            GlobalUnitOfWork.Commit();
            return true;

        }

    }
}
