﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexicon_LMS;
using Lexicon_LMS.Controllers;
using System.Data.Entity;

namespace Lexicon_LMS.Tests.Controllers
{
    public class MyDbContext : DbContext
    {
        public MyDbContext()
        {
        }
    }

    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            CoursesController controller = new CoursesController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            CoursesController controller = new CoursesController();

            // Act
            ViewResult result = controller.Create() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.Model);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            CoursesController controller = new CoursesController();
            int Id = 0;

            // Act
            ViewResult result = controller.Details(Id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
