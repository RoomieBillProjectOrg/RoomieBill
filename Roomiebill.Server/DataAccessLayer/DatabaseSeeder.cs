using Roomiebill.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Roomiebill.Server.DataAccessLayer
{
    public class DatabaseSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if the database is already seeded
                if (!context.Users.Any())
                {
                    // Add initial data if not already present
                    var user1 = new User("Inbar", "inbar@bgu.ac.il", "InbarPassword1!", true);
                    var user2 = new User("Metar", "Metar@bgu.ac.il", "MetarPassword2@", true);
                    var user3 = new User("Vladi", "Vladi@bgu.ac.il", "VladiPassword3#", true);
                    var user4 = new User("Tal", "Tal@bgu.ac.il", "TalPassword4$", true);

                    context.Users.AddRange(user1, user2, user3, user4);
                    context.SaveChanges();
                }

                // Ensure the group exists
                var group = context.Groups.FirstOrDefault(g => g.GroupName == "Test Group2");
                if (group == null)
                {
                    var user1 = context.Users.FirstOrDefault(u => u.Username == "Inbar");
                    var user2 = context.Users.FirstOrDefault(u => u.Username == "Metar");
                    var user3 = context.Users.FirstOrDefault(u => u.Username == "Vladi");
                    var user4 = context.Users.FirstOrDefault(u => u.Username == "Tal");

                    group = new Group("Test Group", user1, new List<User> { user1, user2, user3, user4 });
                    context.Groups.Add(group);
                    context.SaveChanges();
                }

                // Create an expense
                var expense = new Expense
                {
                    Amount = 500.0,
                    Description = "Tal's Mesibat Ravakim",
                    IsPaid = false,
                    PayerId = group.Members.First().Id,
                    GroupId = group.Id,
                    ExpenseSplits = new List<ExpenseSplit>
                    {
                        new ExpenseSplit { UserId = group.Members.ElementAt(0).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(1).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(2).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(3).Id, Percentage = 25.0 }
                    }
                };
                context.Expenses.Add(expense);
                context.SaveChanges();
                //update an expense
                var expenseToUpdate = context.Expenses.FirstOrDefault(e => e.Description == "Tal's Mesibat Ravakim");
                if (expenseToUpdate != null)
                {
                    expenseToUpdate.Amount = 200.0;
                    context.Expenses.Update(expenseToUpdate);
                    context.SaveChanges();
                }


                
            }
        }
    }
}