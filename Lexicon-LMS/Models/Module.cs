﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lexicon_LMS.Models
{
    public class Module
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public string Description { get; set; }

        [Display(Name = "Module activities")]
        public virtual ICollection<Activity> ModuleActivities { get; set; }

        [Display(Name = "Module documents")]
        public virtual ICollection<Document> Documents { get; set; }
        
        [Display(Name = "Course code")]
        public string CourseCode { get; set; }

        public virtual Course Course { get; set; }
    }
}