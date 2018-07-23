﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lexicon_LMS.Models
{
    public class Activity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Deadline { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual Module Module { get; set; }

        [Display(Name = "Module ID")]
        public int ModuleID { get; set; }
    }

    public class Assignment : Activity
    {

    }
}