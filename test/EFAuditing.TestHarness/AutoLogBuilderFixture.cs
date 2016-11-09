﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EFAuditing.TestHarness.Helpers;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EFAuditing.TestHarness
{
    public class AutoLogBuilderFixture
    {

        protected IServiceProvider _provider = null;
        protected string _currentUser = null;

        public AutoLogBuilderFixture()
        {
            var fixture = new InMemoryFixture();
            _provider = fixture.GetServiceProvider();
            _currentUser = "tsmith"; // Thread.CurrentPrincipal.Identity.Name;
            
        }

        [Fact, Trait("Category", "SanityCheck")]
        public void OneEqualsOne()
        {
            Assert.Equal(1, 1);
        }

        [Fact, Trait("Category", "A")]
        public void GetAddedAuditLogsTest()
        {
            //This test ensures only one log is created for an Added Entity Entry
            using (var db = _provider.GetService<TestDbContext>())
            {
                //Arrange
                var customer = new Customer { CustomerId = 5, FirstName = "Trevor", LastName = "Smith" };
                db.Customers.Add(customer);
                var addedEntityEntry = db.ChangeTracker.Entries().Where(p => p.State == EntityState.Added).First();

                //Act
                var result = AuditLogBuilder.GetAddedAuditLogs(addedEntityEntry, _currentUser, EntityState.Added);
               

                //Assert
                Assert.NotNull(result);
            }
        }

        [Fact, Trait("Category", "A")]
        public void GetModifiedAuditLogsTest()
        {
            //This test ensures that one log is create for each property changed
            using (var db = _provider.GetService<TestDbContext>())
            {
                //Arrange
                db.SeedTestData();
                var customer = db.Customers.First();
                customer.FirstName = "Susan";
                customer.LastName = "Smith";
                var modifiedEntry = db.ChangeTracker.Entries().Where(p => p.State == EntityState.Modified).First();

                //Act
                var result = AuditLogBuilder.GetAuditLogs(modifiedEntry, _currentUser, EntityState.Modified);
                

                //Assert
                Assert.Equal(2, result.Count());
            }
        }

        [Fact, Trait("Category", "A")]
        public void GetDeletedAuditLogsTest()
        {
            //This test ensures only one log is created for a Deleted Entity Entry
            using (var db = _provider.GetService<TestDbContext>())
            {
                //Arrange
                db.SeedTestData();
                var customer = db.Customers.First();
                db.Customers.Remove(customer);
                var deletedEntry = db.ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted).First();

                //Act
                var result = AuditLogBuilder.GetAddedAuditLogs(deletedEntry, _currentUser, EntityState.Deleted);
                

                //Assert
                Assert.NotNull(result);
            }
        }
    }
}
