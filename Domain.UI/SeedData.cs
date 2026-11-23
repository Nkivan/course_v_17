using Domain.Core.Enums;
using Domain.Core.Models;
using Domain.Core.Services;

namespace Domain.UI
{
  
    public static class SeedData
    {
        public static void Populate(ProjectManagementService service)
        {
            Project crmProject = service.CreateProject(
                "CRM Portal v2",
                "Веб-портал керування клієнтами та аналітикою продажів",
                DateTime.Today.AddDays(-20),
                DateTime.Today.AddDays(25),
                ProjectStatus.InProgress);

            Project legacyMigration = service.CreateProject(
                "Legacy DB Migration",
                "Міграція застарілої бази даних на PostgreSQL",
                DateTime.Today.AddDays(-45),
                DateTime.Today.AddDays(-5),
                ProjectStatus.InProgress);

            Employee dev1 = service.CreateEmployee("Олена Коваленко", "Senior .NET Developer", 40, 3);
            Employee dev2 = service.CreateEmployee("Максим Ткаченко", "Backend / DevOps Engineer", 40, 3);
            Employee qa1 = service.CreateEmployee("Ірина Мельник", "QA Automation Engineer", 35, 2);

            WorkTask t1 = service.CreateTask(
                crmProject.Id,
                "Проєктування API авторизації",
                "JWT-токени, ролі та refresh-сесії",
                16,
                TaskPriority.High,
                DateTime.Today.AddDays(10),
                dev1.Id);

            WorkTask t2 = service.CreateTask(
                crmProject.Id,
                "Модуль звітів продажів",
                "Експорт звітів та агрегація метрик",
                18,
                TaskPriority.Medium,
                DateTime.Today.AddDays(18),
                dev1.Id);

            WorkTask t3 = service.CreateTask(
                legacyMigration.Id,
                "Скрипти валідації міграції",
                "Перевірка цілісності таблиць після переносу",
                12,
                TaskPriority.Critical,
                DateTime.Today.AddDays(-7),
                dev2.Id);

            WorkTask t4 = service.CreateTask(
                crmProject.Id,
                "Покриття API автотестами",
                "Інтеграційні тести ендпоінтів",
                14,
                TaskPriority.Low,
                DateTime.Today.AddDays(15),
                qa1.Id);

            service.LogTime(t1.Id, dev1.Id, DateTime.Today.AddDays(-2), 6.5, "Реалізація JWT та middleware");
            service.LogTime(t2.Id, dev1.Id, DateTime.Today.AddDays(-1), 4.0, "Базовий запит агрегації");
            service.LogTime(t3.Id, dev2.Id, DateTime.Today.AddDays(-10), 7.5, "Написання перевірок схем");
            service.LogTime(t4.Id, qa1.Id, DateTime.Today, 3.0, "Налаштування тестового оточення");
        }
    }
}
